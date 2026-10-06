using Microsoft.EntityFrameworkCore;
using System.Text;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Trading.Queries.ExportWeeklySuggestionTemplate;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Application.UnitTests.Features.Trading.Queries.ExportWeeklySuggestionTemplate;

public sealed class ExportWeeklySuggestionTemplateQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenReportKeyIsMissing_ShouldThrowInvalidRequest()
    {
        var dbContext = new Mock<IApplicationDbContext>();
        var fileStorage = new Mock<IFileStorage>();
        var handler = new ExportWeeklySuggestionTemplateQueryHandler(dbContext.Object, fileStorage.Object);

        await handler.Invoking(x => x.Handle(new ExportWeeklySuggestionTemplateQuery
        {
            ReportKey = string.Empty,
            AssetClass = SuggestionAssetClass.Crypto,
            Format = "csv"
        }, CancellationToken.None))
            .Should().ThrowAsync<OperationFailedException>()
            .WithMessage("Report key is required.");
    }

    [Fact]
    public async Task Handle_WhenFileExistsInStorage_ReturnsStoredBytes()
    {
        var storedBytes = Encoding.UTF8.GetBytes("stored content");
        var fileStorage = new Mock<IFileStorage>();
        fileStorage
            .Setup(x => x.GetAsync("reports/weekly/crypto/weekly-report.csv", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(storedBytes));

        var handler = new ExportWeeklySuggestionTemplateQueryHandler(Mock.Of<IApplicationDbContext>(), fileStorage.Object);
        var result = await handler.Handle(new ExportWeeklySuggestionTemplateQuery
        {
            ReportKey = "weekly-report",
            AssetClass = SuggestionAssetClass.Crypto,
            Format = "csv"
        }, CancellationToken.None);

        result.FileName.Should().Be("crypto_weekly-report.csv");
        result.ContentType.Should().Be("text/csv");
        result.FileContent.Should().Equal(storedBytes);
        fileStorage.Verify(x => x.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenFileNotFoundRendersCsvAndSavesIt()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new TestApplicationDbContext(options);
        var report = WeeklySuggestionReport.Create(
            DateTime.UtcNow,
            8,
            120,
            5,
            SuggestionAssetClass.Crypto,
            SuggestionReportType.TopSuggestionsWeekly,
            WeeklySuggestionReportStatus.Succeeded,
            "weekly-report");
        report.AddItem("1h", 1, "BTCUSDT", SuggestionAssetClass.Crypto, 0.9m, "LONG", 0.7m, 30000m, 29500m, 31000m, 32000m);
        context.Add(report);
        await context.SaveChangesAsync();

        var fileStorage = new Mock<IFileStorage>();
        fileStorage
            .Setup(x => x.GetAsync("reports/weekly/crypto/weekly-report.csv", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationFailedException(ErrorCodes.FILE_NOT_FOUND, "not found"));
        fileStorage
            .Setup(x => x.SaveAsync(It.IsAny<Stream>(), "reports/weekly/crypto/weekly-report.csv", "text/csv", It.IsAny<CancellationToken>()))
            .ReturnsAsync("reports/weekly/crypto/weekly-report.csv");

        var handler = new ExportWeeklySuggestionTemplateQueryHandler(context, fileStorage.Object);
        var result = await handler.Handle(new ExportWeeklySuggestionTemplateQuery
        {
            ReportKey = "weekly-report",
            AssetClass = SuggestionAssetClass.Crypto,
            Format = "csv"
        }, CancellationToken.None);

        result.FileName.Should().Be("crypto_weekly-report.csv");
        result.ContentType.Should().Be("text/csv");
        result.FileContent.Should().Contain(Encoding.UTF8.GetBytes("BTCUSDT"));
        fileStorage.Verify(x => x.SaveAsync(It.IsAny<Stream>(), "reports/weekly/crypto/weekly-report.csv", "text/csv", It.IsAny<CancellationToken>()), Times.Once);
    }
}
