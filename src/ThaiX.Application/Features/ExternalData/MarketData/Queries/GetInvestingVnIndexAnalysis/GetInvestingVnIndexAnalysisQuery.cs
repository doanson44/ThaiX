using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetInvestingVnIndexAnalysis;

/// <summary>
/// Retrieves daily VNINDEX candles and analyzes them with ThaiX's existing kline engine.
/// </summary>
public sealed record GetInvestingVnIndexAnalysisQuery : IAppQuery<InvestingVnIndexAnalysisResponse>;

public sealed record InvestingVnIndexAnalysisResponse
{
    public string Symbol { get; init; } = "VNINDEX";
    public bool Success { get; init; }
    public string? Message { get; init; }
    public int HistoryCount { get; init; }
    public DateTimeOffset? LatestTime { get; init; }
    public decimal? Open { get; init; }
    public decimal? High { get; init; }
    public decimal? Low { get; init; }
    public decimal? Close { get; init; }
    public decimal? Volume { get; init; }
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
}
