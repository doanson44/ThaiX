using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.TradingSuggestions;

public sealed class WeeklySuggestionReportItem : BaseAuditableEntity
{
    private WeeklySuggestionReportItem() { }

    public Guid ReportId { get; private set; }
    public WeeklySuggestionReport Report { get; private set; } = null!;
    public string Timeframe { get; private set; } = string.Empty;
    public int Rank { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public SuggestionAssetClass MarketType { get; private set; }
    public decimal CompositeScore { get; private set; }
    public string Signal { get; private set; } = string.Empty;
    public decimal Confidence { get; private set; }
    public decimal EntryPrice { get; private set; }
    public decimal? StopLoss { get; private set; }
    public decimal? TakeProfit1 { get; private set; }
    public decimal? TakeProfit2 { get; private set; }

    public static WeeklySuggestionReportItem Create(
        Guid reportId,
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
        ArgumentException.ThrowIfNullOrWhiteSpace(timeframe);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(signal);

        return new WeeklySuggestionReportItem
        {
            Id = Guid.NewGuid(),
            ReportId = reportId,
            Timeframe = timeframe.Trim(),
            Rank = rank,
            Symbol = symbol.Trim().ToUpperInvariant(),
            MarketType = marketType,
            CompositeScore = compositeScore,
            Signal = signal.Trim().ToUpperInvariant(),
            Confidence = confidence,
            EntryPrice = entryPrice,
            StopLoss = stopLoss,
            TakeProfit1 = takeProfit1,
            TakeProfit2 = takeProfit2
        };
    }
}
