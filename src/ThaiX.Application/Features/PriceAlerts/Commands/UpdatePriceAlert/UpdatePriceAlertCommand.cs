using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.Features.PriceAlerts.Commands.UpdatePriceAlert;

/// <summary>
/// Command to update an existing price alert.
/// Symbol and AssetType cannot be changed after creation.
/// </summary>
[InvalidateCache(CacheGroups.PriceAlerts)]
public sealed record UpdatePriceAlertCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required AlertCondition Condition { get; init; }
    public required decimal TargetPrice { get; init; }
    public string? Note { get; init; }
    public required bool IsOneTime { get; init; }
    public required bool IsEnabled { get; init; }
}
