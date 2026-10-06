using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerUnlocksFromDb;

public sealed record GetChainBrokerUnlocksFromDbQuery : IAppQuery<PagedResult<ChainBrokerUnlockDto>>, ICacheableQuery
{
    public string? Search { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => CacheKeys.ChainBrokerData.UnlocksList(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromHours(6);
    public string CacheGroup => CacheGroups.ChainBrokerData;
    public bool IsVersionedList => false;
}

public sealed record ChainBrokerUnlockDto
{
    public required Guid Id { get; init; }
    public required string Slug { get; init; }
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? Ticker { get; init; }
    public DateOnly? NextUnlockDate { get; init; }
    public string? UnlockAmount { get; init; }
    public decimal? UnlockValueUsd { get; init; }
    public string? RoundName { get; init; }
    public decimal? CirculationPercent { get; init; }
    public decimal? UnlockPercent { get; init; }
    public decimal? Volume24hUsd { get; init; }
    public decimal? PriceChange24h { get; init; }
    public decimal? PriceChange7d { get; init; }
    public decimal? PriceChange30d { get; init; }
    public decimal? PriceChange1y { get; init; }
}
