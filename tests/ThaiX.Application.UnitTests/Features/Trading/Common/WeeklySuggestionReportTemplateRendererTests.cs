using System.Text;
using ThaiX.Application.Features.Trading.Common;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Application.UnitTests.Features.Trading.Common;

public sealed class WeeklySuggestionReportTemplateRendererTests
{
    [Fact]
    public void BuildCsvStorageKey_ShouldReturnSanitizedPath()
    {
        var key = WeeklySuggestionReportTemplateStorage.BuildCsvStorageKey("My/Report:Name", SuggestionAssetClass.Crypto);

        key.Should().Be("reports/weekly/crypto/My_Report_Name.csv");
    }

    [Fact]
    public void BuildMarkdownFileName_ShouldReturnSanitizedFileName()
    {
        var name = WeeklySuggestionReportTemplateStorage.BuildMarkdownFileName("Weekly Report", SuggestionAssetClass.Stock);

        name.Should().Be("stock_Weekly Report.md");
    }

    [Fact]
    public void RenderCsv_WhenItemsExist_ShouldProduceHeaderAndRows()
    {
        var context = new WeeklySuggestionTemplateContext(
            "test-report",
            new DateTime(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc),
            SuggestionAssetClass.Crypto,
            WeeklySuggestionReportStatus.Succeeded,
            new[]
            {
                new WeeklySuggestionTemplateItem("1h", 1, "BTCUSDT", SuggestionAssetClass.Crypto, 88.5m, "LONG", 0.95m, 30000m, 29500m, 31000m, 32000m)
            });

        var csv = WeeklySuggestionReportTemplateRenderer.RenderCsv(context);
        var text = Encoding.UTF8.GetString(csv);

        text.Should().Contain("ReportKey,RunAtUtc,AssetClass,Status");
        text.Should().Contain("test-report");
        text.Should().Contain("BTCUSDT");
        text.Should().Contain("0.95");
    }

    [Fact]
    public void RenderMarkdown_WhenNoItems_ShouldContainNoPicksMessage()
    {
        var context = new WeeklySuggestionTemplateContext(
            "empty-report",
            DateTime.UtcNow,
            SuggestionAssetClass.Stock,
            WeeklySuggestionReportStatus.NoData,
            Array.Empty<WeeklySuggestionTemplateItem>());

        var markdown = WeeklySuggestionReportTemplateRenderer.RenderMarkdown(context);
        var text = Encoding.UTF8.GetString(markdown);

        text.Should().Contain("_No picks in this report._");
        text.Should().Contain("Report Key: `empty-report`");
    }
}
