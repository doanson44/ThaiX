using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;

/// <summary>
/// Query to retrieve a paginated list of market scanner rules.
/// </summary>
public sealed record GetMarketScannerRulesQuery : PagedRequest, IAppQuery<PagedResult<MarketScannerRuleListItemDto>>, ICacheableQuery
{
    public string? SearchTerm { get; init; }
    public bool? IsEnabled { get; init; }

    public string CacheKey => CacheKeys.MarketScanner.RulesList(SearchTerm, IsEnabled, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    public string CacheGroup => CacheGroups.MarketScanner;
    public bool IsVersionedList => true;
}

/// <summary>
/// DTO for market scanner rule list item.
/// </summary>
public sealed record MarketScannerRuleListItemDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required MarketSignalType SignalType { get; init; }
    public required MarketTimeWindow Window { get; init; }
    public decimal? Threshold { get; init; }
    public required bool IsEnabled { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastUpdated { get; init; }
}
