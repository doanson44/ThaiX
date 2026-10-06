using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.Trading.Queries.EvaluateWeeklySuggestionPerformance;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Application.UnitTests.Features.Trading.Queries.EvaluateWeeklySuggestionPerformance;

public sealed class EvaluateWeeklySuggestionPerformanceQueryHandlerTests
{
    private static DbContextOptions<TestApplicationDbContext> CreateOptions()
        => new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Handle_WhenNoSymbols_ReturnsEmptyResult()
    {
        var options = CreateOptions();
        await using var context = new TestApplicationDbContext(options);
        var handler = new EvaluateWeeklySuggestionPerformanceQueryHandler(context);

        var result = await handler.Handle(new EvaluateWeeklySuggestionPerformanceQuery
        {
            Symbols = Array.Empty<EvaluateWeeklySuggestionSymbolInput>(),
            LookbackReports = 12
        }, CancellationToken.None);

        result.MatchedCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithReportsAndSymbols_EvaluatesMatchingItems()
    {
        var options = CreateOptions();
        await using var context = new TestApplicationDbContext(options);

        var report = WeeklySuggestionReport.Create(
            DateTime.UtcNow,
            5,
            100,
            3,
            SuggestionAssetClass.Crypto,
            SuggestionReportType.TopSuggestionsWeekly,
            WeeklySuggestionReportStatus.Succeeded,
            "crypto-history");
        report.AddItem("1h", 1, "BTCUSDT", SuggestionAssetClass.Crypto, 0.95m, "LONG", 0.9m, 100m, 95m, 110m, 120m);
        context.Add(report);
        await context.SaveChangesAsync();

        var handler = new EvaluateWeeklySuggestionPerformanceQueryHandler(context);
        var result = await handler.Handle(new EvaluateWeeklySuggestionPerformanceQuery
        {
            AssetClass = SuggestionAssetClass.Crypto,
            LookbackReports = 12,
            Symbols = new[]
            {
                new EvaluateWeeklySuggestionSymbolInput { Symbol = " btcusdt ", CurrentPrice = 110m }
            }
        }, CancellationToken.None);

        result.MatchedCount.Should().Be(1);
        result.Items.Should().ContainSingle(item =>
            item.Symbol == "BTCUSDT" &&
            item.CurrentPrice == 110m &&
            item.ChangePercentFromEntry == 10m &&
            item.ReportKey == "crypto-history" &&
            item.AssetClass == SuggestionAssetClass.Crypto);
    }
}
