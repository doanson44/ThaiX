using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcBitcoinAnalysis;

public sealed record GetMexcBitcoinAnalysisQuery : IAppQuery<MexcBitcoinAnalysisResponse>;

public sealed record MexcBitcoinAnalysisResponse
{
    public string Symbol { get; init; } = "BTCUSDT";
    public decimal Price { get; init; }
    public decimal Open { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePct { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Volume { get; init; }
    public decimal QuoteVolume { get; init; }
    public decimal? PreviousClose { get; init; }
    public int HistoryCount { get; init; }
    public IReadOnlyList<MexcBitcoinCandle> History { get; init; } = Array.Empty<MexcBitcoinCandle>();
    public decimal? Ema9 { get; init; }
    public decimal? Ema21 { get; init; }
    public decimal? Ema50 { get; init; }
    public decimal? Rsi14 { get; init; }
    public decimal? Atr14 { get; init; }
    public decimal? AverageVolume20 { get; init; }
    public decimal? HighestHigh20 { get; init; }
    public decimal? LowestLow20 { get; init; }
    public string? Trend { get; init; }
    public string? Momentum { get; init; }
    public string? Setup { get; init; }
    public string? Signal { get; init; }
    public decimal? Confidence { get; init; }
    public decimal? Entry { get; init; }
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit1 { get; init; }
    public decimal? TakeProfit2 { get; init; }
    public string LastUpdated { get; init; } = string.Empty;
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record MexcBitcoinCandle
{
    public DateTimeOffset Time { get; init; }
    public string DateLabel { get; init; } = string.Empty;
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal Volume { get; init; }
}
