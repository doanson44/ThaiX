using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.Features.AssetPositions.EventHandlers;

/// <summary>
/// Disables active price alerts for a symbol when the corresponding position is fully closed.
/// </summary>
public sealed class PositionClosedEventHandler : INotificationHandler<PositionClosedEvent>
{
    private readonly IApplicationDbContext _context;

    public PositionClosedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PositionClosedEvent notification, CancellationToken cancellationToken)
    {
        var alertAssetType = notification.AssetType == PositionAssetType.Crypto
            ? AssetType.CryptoSpot
            : AssetType.VnStock;

        var alerts = await _context.PriceAlerts
            .Where(a =>
                a.Symbol == notification.Symbol &&
                a.AssetType == alertAssetType &&
                a.IsEnabled &&
                !a.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var alert in alerts)
            alert.Disable();

        if (alerts.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);
    }
}
