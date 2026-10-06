using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;

/// <summary>
/// Query to retrieve Yahoo Finance VIX (Volatility Index) data.
/// </summary>
public sealed record GetYahooFinanceVixQuery : IAppQuery<YahooFinanceVixResponse>;

/// <summary>
/// Response containing VIX (Volatility Index) data with feature engineering and regime analysis.
/// </summary>
public sealed record YahooFinanceVixResponse
{
    public required string Symbol { get; init; }
    public decimal? CurrentPrice { get; init; }
    public decimal? PreviousClose { get; init; }
    public decimal? DayHigh { get; init; }
    public decimal? DayLow { get; init; }
    public decimal? FiftyTwoWeekHigh { get; init; }
    public decimal? FiftyTwoWeekLow { get; init; }
    public DateTime? LastUpdateTime { get; init; }
    // Feature engineering
    public decimal? CurrentValue { get; init; }
    public decimal? Ema10 { get; init; }
    public decimal? Ema20 { get; init; }
    public decimal? Momentum3D { get; init; }
    public decimal? Momentum5D { get; init; }
    public double Percentile { get; init; }
    public bool IsSpike { get; init; }
    // Regime (stable code: RiskOn | EarlyRisk | Panic | Recovery)
    // Liquidity, CapitalFlow, Action are derived client-side from Regime for i18n support.
    public string? Regime { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
}
