using System.Globalization;
using System.Text;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Application.Features.Trading.Common;

public sealed record WeeklySuggestionTemplateContext(
    string ReportKey,
    DateTime RunAtUtc,
    SuggestionAssetClass AssetClass,
    WeeklySuggestionReportStatus Status,
    IReadOnlyList<WeeklySuggestionTemplateItem> Items);

public sealed record WeeklySuggestionTemplateItem(
    string Timeframe,
    int Rank,
    string Symbol,
    SuggestionAssetClass MarketType,
    decimal CompositeScore,
    string Signal,
    decimal Confidence,
    decimal EntryPrice,
    decimal? StopLoss,
    decimal? TakeProfit1,
    decimal? TakeProfit2);

public static class WeeklySuggestionReportTemplateStorage
{
    public static string BuildCsvStorageKey(string reportKey, SuggestionAssetClass assetClass)
        => $"reports/weekly/{ToAssetClassSegment(assetClass)}/{SanitizeSegment(reportKey)}.csv";

    public static string BuildMarkdownStorageKey(string reportKey, SuggestionAssetClass assetClass)
        => $"reports/weekly/{ToAssetClassSegment(assetClass)}/{SanitizeSegment(reportKey)}.md";

    public static string BuildCsvFileName(string reportKey, SuggestionAssetClass assetClass)
        => $"{ToAssetClassSegment(assetClass)}_{SanitizeSegment(reportKey)}.csv";

    public static string BuildMarkdownFileName(string reportKey, SuggestionAssetClass assetClass)
        => $"{ToAssetClassSegment(assetClass)}_{SanitizeSegment(reportKey)}.md";

    private static string ToAssetClassSegment(SuggestionAssetClass assetClass)
        => assetClass.ToString().ToLowerInvariant();

    private static string SanitizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars())
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar,
            Path.VolumeSeparatorChar,
            ':'
        };

        var sb = new StringBuilder(value.Length);
        foreach (var c in value.Trim())
        {
            sb.Append(invalidChars.Contains(c) ? '_' : c);
        }

        return sb.ToString();
    }
}

public static class WeeklySuggestionReportTemplateRenderer
{
    public static byte[] RenderCsv(WeeklySuggestionTemplateContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ReportKey,RunAtUtc,AssetClass,Status,Timeframe,Rank,Symbol,MarketType,CompositeScore,Signal,Confidence,EntryPrice,StopLoss,TakeProfit1,TakeProfit2");

        foreach (var item in context.Items.OrderBy(x => x.Timeframe).ThenBy(x => x.Rank))
        {
            AppendCsvField(sb, context.ReportKey);
            sb.Append(',');
            AppendCsvField(sb, context.RunAtUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            sb.Append(',');
            AppendCsvField(sb, context.AssetClass.ToString());
            sb.Append(',');
            AppendCsvField(sb, context.Status.ToString());
            sb.Append(',');
            AppendCsvField(sb, item.Timeframe);
            sb.Append(',');
            AppendCsvField(sb, item.Rank.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
            AppendCsvField(sb, item.Symbol);
            sb.Append(',');
            AppendCsvField(sb, item.MarketType.ToString());
            sb.Append(',');
            AppendCsvField(sb, item.CompositeScore.ToString("0.####", CultureInfo.InvariantCulture));
            sb.Append(',');
            AppendCsvField(sb, item.Signal);
            sb.Append(',');
            AppendCsvField(sb, item.Confidence.ToString("0.####", CultureInfo.InvariantCulture));
            sb.Append(',');
            AppendCsvField(sb, item.EntryPrice.ToString("0.########", CultureInfo.InvariantCulture));
            sb.Append(',');
            AppendCsvField(sb, item.StopLoss?.ToString("0.########", CultureInfo.InvariantCulture));
            sb.Append(',');
            AppendCsvField(sb, item.TakeProfit1?.ToString("0.########", CultureInfo.InvariantCulture));
            sb.Append(',');
            AppendCsvField(sb, item.TakeProfit2?.ToString("0.########", CultureInfo.InvariantCulture));
            sb.AppendLine();
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public static byte[] RenderMarkdown(WeeklySuggestionTemplateContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Weekly Suggestion Report");
        sb.AppendLine();
        sb.AppendLine($"- Report Key: `{context.ReportKey}`");
        sb.AppendLine($"- Run At (UTC): `{context.RunAtUtc:yyyy-MM-dd HH:mm:ss}`");
        sb.AppendLine($"- Asset Class: `{context.AssetClass}`");
        sb.AppendLine($"- Status: `{context.Status}`");
        sb.AppendLine($"- Picks: `{context.Items.Count}`");
        sb.AppendLine();

        if (context.Items.Count == 0)
        {
            sb.AppendLine("_No picks in this report._");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        sb.AppendLine("| Timeframe | Rank | Symbol | MarketType | Score | Signal | Confidence | Entry | StopLoss | TakeProfit1 | TakeProfit2 |");
        sb.AppendLine("| --- | ---: | --- | --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |");

        foreach (var item in context.Items.OrderBy(x => x.Timeframe).ThenBy(x => x.Rank))
        {
            sb.Append("| ");
            sb.Append(item.Timeframe);
            sb.Append(" | ");
            sb.Append(item.Rank.ToString(CultureInfo.InvariantCulture));
            sb.Append(" | ");
            sb.Append(item.Symbol);
            sb.Append(" | ");
            sb.Append(item.MarketType);
            sb.Append(" | ");
            sb.Append(item.CompositeScore.ToString("0.####", CultureInfo.InvariantCulture));
            sb.Append(" | ");
            sb.Append(item.Signal);
            sb.Append(" | ");
            sb.Append(item.Confidence.ToString("0.####", CultureInfo.InvariantCulture));
            sb.Append(" | ");
            sb.Append(item.EntryPrice.ToString("0.########", CultureInfo.InvariantCulture));
            sb.Append(" | ");
            sb.Append(FormatNullableDecimal(item.StopLoss));
            sb.Append(" | ");
            sb.Append(FormatNullableDecimal(item.TakeProfit1));
            sb.Append(" | ");
            sb.Append(FormatNullableDecimal(item.TakeProfit2));
            sb.AppendLine(" |");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string FormatNullableDecimal(decimal? value)
        => value?.ToString("0.########", CultureInfo.InvariantCulture) ?? "-";

    private static void AppendCsvField(StringBuilder sb, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var needsQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!needsQuote)
        {
            sb.Append(value);
            return;
        }

        sb.Append('"');
        sb.Append(value.Replace("\"", "\"\"", StringComparison.Ordinal));
        sb.Append('"');
    }
}
