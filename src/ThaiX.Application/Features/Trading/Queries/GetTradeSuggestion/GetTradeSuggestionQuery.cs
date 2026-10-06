using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Trading;

namespace ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;

/// <summary>
/// Query to retrieve a trade suggestion based on kline analysis.
/// MarketRegime and EventRisk are caller-supplied context that directly affect signal generation.
/// </summary>
public sealed record GetTradeSuggestionQuery : IAppQuery<TradeSuggestionDto>, ICacheableQuery
{
    public required string Symbol { get; init; }
    public required MarketType MarketType { get; init; }
    public required string Timeframe { get; init; }
    public bool UseLongTermTimeframe { get; init; }

    /// <summary>Current market regime. Defaults to RiskOn. Affects whether long/short signals are generated.</summary>
    public MarketRegime MarketRegime { get; init; } = MarketRegime.RiskOn;

    /// <summary>Current event risk level. Defaults to Low. High risk suppresses all signals.</summary>
    public EventRisk EventRisk { get; init; } = EventRisk.Low;

    public string CacheKey =>
        $"{CacheKeys.Trading.Suggestion(Symbol, MarketType.ToString(), Timeframe, MarketRegime.ToString(), EventRisk.ToString())}:{(UseLongTermTimeframe ? "1" : "0")}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
    public string CacheGroup => CacheGroups.Trading;
}
