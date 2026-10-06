using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;

namespace ThaiX.Application.Features.MarketScanner.Queries.GetAllMarketScannerRules;

/// <summary>
/// Query to retrieve all market scanner rules without pagination.
/// </summary>
public sealed record GetAllMarketScannerRulesQuery : IAppQuery<IReadOnlyList<MarketScannerRuleListItemDto>>, ICacheableQuery
{
    public bool? IsEnabled { get; init; }

    public string CacheKey => CacheKeys.MarketScanner.AllRules(IsEnabled);
    public TimeSpan? Expiration => TimeSpan.FromHours(1);
    public string CacheGroup => CacheGroups.MarketScanner;
}
