using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Queries.GetStockPositions;

public sealed record GetStockPositionsQuery : PagedRequest, IAppQuery<PagedResult<StockPositionListItemDto>>, ICacheableQuery
{
    public required Guid PortfolioId { get; init; }
    public bool? IsClosed { get; init; }

    public string CacheKey => CacheKeys.StockPositions.List(PortfolioId, IsClosed, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.StockPositions;
    public bool IsVersionedList => true;
}

public sealed record StockPositionListItemDto
{
    public required Guid Id { get; init; }
    public required Guid PortfolioId { get; init; }
    public required string Symbol { get; init; }
    public required StockExchange Exchange { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal AverageEntryPrice { get; init; }
    public required decimal TotalInvested { get; init; }
    public required decimal RealizedPnl { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public string? Note { get; init; }
    public required bool IsClosed { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
