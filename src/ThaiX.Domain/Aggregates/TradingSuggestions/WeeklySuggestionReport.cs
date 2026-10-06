using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.TradingSuggestions;

public enum SuggestionAssetClass
{
    Stock = 1,
    Crypto = 2
}

public enum SuggestionReportType
{
    TopSuggestionsWeekly = 1
}

public enum WeeklySuggestionReportStatus
{
    Succeeded = 1,
    PartialSucceeded = 2,
    NoData = 3,
    Failed = 4
}

public sealed class WeeklySuggestionReport : BaseAuditableEntity
{
    private readonly List<WeeklySuggestionReportItem> _items = new();

    private WeeklySuggestionReport() { }

    public DateTime RunAtUtc { get; private set; }
    public int ElapsedSeconds { get; private set; }
    public int CandidateScanLimit { get; private set; }
    public int TopCount { get; private set; }
    public SuggestionAssetClass AssetClass { get; private set; }
    public SuggestionReportType ReportType { get; private set; }
    public WeeklySuggestionReportStatus Status { get; private set; }
    public string ReportKey { get; private set; } = string.Empty;
    public IReadOnlyCollection<WeeklySuggestionReportItem> Items => _items.AsReadOnly();

    public static WeeklySuggestionReport Create(
        DateTime runAtUtc,
        int elapsedSeconds,
        int candidateScanLimit,
        int topCount,
        SuggestionAssetClass assetClass,
        SuggestionReportType reportType,
        WeeklySuggestionReportStatus status,
        string reportKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportKey);

        return new WeeklySuggestionReport
        {
            Id = Guid.NewGuid(),
            RunAtUtc = runAtUtc,
            ElapsedSeconds = elapsedSeconds,
            CandidateScanLimit = candidateScanLimit,
            TopCount = topCount,
            AssetClass = assetClass,
            ReportType = reportType,
            Status = status,
            ReportKey = reportKey.Trim()
        };
    }

    public void AddItem(
        string timeframe,
        int rank,
        string symbol,
        SuggestionAssetClass marketType,
        decimal compositeScore,
        string signal,
        decimal confidence,
        decimal entryPrice,
        decimal? stopLoss,
        decimal? takeProfit1,
        decimal? takeProfit2)
    {
        _items.Add(WeeklySuggestionReportItem.Create(
            reportId: Id,
            timeframe: timeframe,
            rank: rank,
            symbol: symbol,
            marketType: marketType,
            compositeScore: compositeScore,
            signal: signal,
            confidence: confidence,
            entryPrice: entryPrice,
            stopLoss: stopLoss,
            takeProfit1: takeProfit1,
            takeProfit2: takeProfit2));
    }
}
