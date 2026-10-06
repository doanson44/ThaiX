using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.Trading.Queries.GetWeeklySuggestionHistory;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Application.UnitTests.Features.Trading.Queries.GetWeeklySuggestionHistory;

public sealed class GetWeeklySuggestionHistoryQueryHandlerTests
{
    private static DbContextOptions<TestApplicationDbContext> CreateOptions()
        => new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_WithAssetClassFilter_ReturnsOnlyMatchingReports()
    {
        var options = CreateOptions();

        await using (var context = new TestApplicationDbContext(options))
        {
            var stockReport = WeeklySuggestionReport.Create(
                DateTime.UtcNow.AddDays(-1),
                10,
                100,
                5,
                SuggestionAssetClass.Stock,
                SuggestionReportType.TopSuggestionsWeekly,
                WeeklySuggestionReportStatus.Succeeded,
                "stock-report");
            stockReport.AddItem("Day1", 1, "AAPL", SuggestionAssetClass.Stock, 0.8m, "LONG", 0.9m, 150m, 145m, 155m, 160m);

            var cryptoReport = WeeklySuggestionReport.Create(
                DateTime.UtcNow,
                12,
                200,
                5,
                SuggestionAssetClass.Crypto,
                SuggestionReportType.TopSuggestionsWeekly,
                WeeklySuggestionReportStatus.Succeeded,
                "crypto-report");
            cryptoReport.AddItem("1h", 1, "BTCUSDT", SuggestionAssetClass.Crypto, 0.95m, "LONG", 0.95m, 30000m, 29500m, 31000m, 32000m);

            context.Add(stockReport);
            context.Add(cryptoReport);
            await context.SaveChangesAsync();

            var handler = new GetWeeklySuggestionHistoryQueryHandler(context);
            var result = await handler.Handle(
                new GetWeeklySuggestionHistoryQuery
                {
                    AssetClass = SuggestionAssetClass.Crypto,
                    Page = 1,
                    PageSize = 1
                },
                CancellationToken.None);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(1);
            result.Total.Should().Be(1);
            result.Items.Should().ContainSingle().Which.ReportKey.Should().Be("crypto-report");
            result.Items.Single().PickedCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task Handle_WithOutOfRangePaging_AdjustsPageAndPageSizeBounds()
    {
        var options = CreateOptions();

        await using (var context = new TestApplicationDbContext(options))
        {
            for (var i = 1; i <= 2; i++)
            {
                var report = WeeklySuggestionReport.Create(
                    DateTime.UtcNow.AddDays(-i),
                    5,
                    50,
                    3,
                    SuggestionAssetClass.Stock,
                    SuggestionReportType.TopSuggestionsWeekly,
                    WeeklySuggestionReportStatus.Succeeded,
                    $"report-{i}");
                report.AddItem("Week1", 1, "MSFT", SuggestionAssetClass.Stock, 0.7m, "LONG", 0.8m, 300m, 290m, 310m, 320m);
                context.Add(report);
            }

            await context.SaveChangesAsync();
            var handler = new GetWeeklySuggestionHistoryQueryHandler(context);
            var result = await handler.Handle(
                new GetWeeklySuggestionHistoryQuery
                {
                    Page = 0,
                    PageSize = 200
                },
                CancellationToken.None);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(100);
            result.Total.Should().Be(2);
            result.Items.Should().HaveCount(2);
        }
    }
}
