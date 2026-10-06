using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Queries.GetPositionTransactions;

public sealed record GetPositionTransactionsQuery : PagedRequest, IAppQuery<PagedResult<PositionTransactionDto>>, ICacheableQuery
{
    public required Guid PositionId { get; init; }
    public required PositionAssetType AssetType { get; init; }

    public string CacheKey => AssetType == PositionAssetType.Crypto
        ? CacheKeys.CryptoPositions.Transactions(PositionId, PageNumber, PageSize)
        : CacheKeys.StockPositions.Transactions(PositionId, PageNumber, PageSize);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => AssetType == PositionAssetType.Crypto ? CacheGroups.CryptoPositions : CacheGroups.StockPositions;
    public bool IsVersionedList => true;
}

public sealed record PositionTransactionDto
{
    public required Guid Id { get; init; }
    public required TransactionType TransactionType { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal Price { get; init; }
    public decimal? Fee { get; init; }
    public required DateTime TransactedAt { get; init; }
    public string? Note { get; init; }
    public string? ExternalRef { get; init; }
}
