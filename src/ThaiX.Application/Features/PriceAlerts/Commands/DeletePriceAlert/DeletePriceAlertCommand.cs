using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.PriceAlerts.Commands.DeletePriceAlert;

/// <summary>
/// Command to soft-delete a price alert.
/// </summary>
[InvalidateCache(CacheGroups.PriceAlerts)]
public sealed record DeletePriceAlertCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
