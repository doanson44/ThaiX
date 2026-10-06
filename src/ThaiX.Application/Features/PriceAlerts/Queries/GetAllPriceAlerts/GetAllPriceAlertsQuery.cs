using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.Features.PriceAlerts.Queries.GetAllPriceAlerts;

/// <summary>
/// Query to retrieve all active (enabled, non-deleted) price alerts without pagination.
/// Intended for background job consumption (price alert checker).
/// Result is cached indefinitely and invalidated by Create/Update/Delete commands.
/// </summary>
public sealed record GetAllPriceAlertsQuery : IAppQuery<IReadOnlyList<PriceAlertDto>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.PriceAlerts.AllActive();
    public TimeSpan? Expiration => null;
    public string CacheGroup => CacheGroups.PriceAlerts;
}

/// <summary>
/// Minimal price alert data needed by the price alert checker job.
/// </summary>
public sealed record PriceAlertDto
{
    public required Guid Id { get; init; }
    public required string Symbol { get; init; }
    public required AssetType AssetType { get; init; }
    public required AlertCondition Condition { get; init; }
    public required decimal TargetPrice { get; init; }
    public string? Note { get; init; }
    public required bool IsOneTime { get; init; }
    public DateTime? LastTriggeredAt { get; init; }
}
