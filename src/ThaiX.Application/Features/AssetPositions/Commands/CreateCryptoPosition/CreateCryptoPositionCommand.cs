using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.AssetPositions.Commands.CreateCryptoPosition;

[InvalidateCache(CacheGroups.CryptoPositions)]
public sealed record CreateCryptoPositionCommand : IAppCommand<Guid>
{
    public required Guid PortfolioId { get; init; }
    public required string Symbol { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public string? Note { get; init; }

    // First transaction (buy)
    public required decimal Quantity { get; init; }
    public required decimal Price { get; init; }
    public decimal Fee { get; init; }
    public required DateTime TransactedAt { get; init; }
    public string? TransactionNote { get; init; }
    public string? ExternalRef { get; init; }
}
