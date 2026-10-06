using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Commands.AddStockTransaction;

[InvalidateCache(CacheGroups.StockPositions)]
public sealed record AddStockTransactionCommand : IAppCommand<Unit>
{
    public required Guid PositionId { get; init; }
    public required TransactionType TransactionType { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal Price { get; init; }
    public decimal Fee { get; init; }
    public required DateTime TransactedAt { get; init; }
    public string? Note { get; init; }
    public string? ExternalRef { get; init; }
}
