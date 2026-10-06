using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.Features.PriceAlerts.Queries.GetPriceAlerts;

/// <summary>
/// Query to retrieve a paginated list of price alerts for the admin UI.
/// </summary>
public sealed record GetPriceAlertsQuery : PagedRequest, IAppQuery<PagedResult<PriceAlertListItemDto>>, ICacheableQuery
{
    public string? SearchTerm { get; init; }
    public bool? IsEnabled { get; init; }

    public string CacheKey => CacheKeys.PriceAlerts.PriceAlertsList(SearchTerm, IsEnabled, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
    public string CacheGroup => CacheGroups.PriceAlerts;
    public bool IsVersionedList => true;
}

/// <summary>
/// DTO for the price alert list item in the admin grid.
/// </summary>
public sealed record PriceAlertListItemDto
{
    public required Guid Id { get; init; }
    public required string Symbol { get; init; }
    public required AssetType AssetType { get; init; }
    public required AlertCondition Condition { get; init; }
    public required decimal TargetPrice { get; init; }
    public string? Note { get; init; }
    public required bool IsEnabled { get; init; }
    public required bool IsOneTime { get; init; }
    public DateTime? LastTriggeredAt { get; init; }
    public required int TriggerCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
