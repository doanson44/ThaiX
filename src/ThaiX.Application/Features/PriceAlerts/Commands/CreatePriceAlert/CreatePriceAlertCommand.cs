using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.Features.PriceAlerts.Commands.CreatePriceAlert;

/// <summary>
/// Command to create a new price alert.
/// </summary>
[InvalidateCache(CacheGroups.PriceAlerts)]
public sealed record CreatePriceAlertCommand : IAppCommand<Guid>
{
    public required string Symbol { get; init; }
    public required AssetType AssetType { get; init; }
    public required AlertCondition Condition { get; init; }
    public required decimal TargetPrice { get; init; }
    public string? Note { get; init; }
    public required bool IsOneTime { get; init; }
}
