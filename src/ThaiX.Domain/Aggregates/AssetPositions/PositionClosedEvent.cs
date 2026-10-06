using MediatR;
using ThaiX.Domain.Common.Events;

namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// Domain event raised when an asset position's quantity drops to zero (fully closed).
/// Consumed by the PriceAlert feature to auto-disable orphaned alerts.
/// </summary>
public sealed record PositionClosedEvent : IDomainEvent, INotification
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid PositionId { get; }
    public PositionAssetType AssetType { get; }
    public Guid PortfolioId { get; }
    public string Symbol { get; }

    public PositionClosedEvent(Guid positionId, PositionAssetType assetType, Guid portfolioId, string symbol)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        PositionId = positionId;
        AssetType = assetType;
        PortfolioId = portfolioId;
        Symbol = symbol;
    }
}
