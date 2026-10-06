using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerProjectsFromDb;

public sealed record GetChainBrokerProjectsFromDbQuery : IAppQuery<PagedResult<ChainBrokerProjectDto>>, ICacheableQuery
{
    public string? Search { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => CacheKeys.ChainBrokerData.ProjectsList(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromHours(6);
    public string CacheGroup => CacheGroups.ChainBrokerData;
    public bool IsVersionedList => false;
}

public sealed record ChainBrokerProjectDto
{
    public required Guid Id { get; init; }
    public required string Slug { get; init; }
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? Ticker { get; init; }
    public decimal? CurrentPriceUsd { get; init; }
    public decimal? PublicPriceUsd { get; init; }
    public decimal? PrivatePriceUsd { get; init; }
    public decimal? AthPriceUsd { get; init; }
    public decimal? PublicRoi { get; init; }
    public decimal? PrivateRoi { get; init; }
    public decimal? PublicAthRoi { get; init; }
    public decimal? PrivateAthRoi { get; init; }
    public decimal? BrokerScore { get; init; }
    public decimal? SecurityScore { get; init; }
    public int? TwitterScore { get; init; }
    public int? Rank { get; init; }
    public decimal? PublicRaiseUsd { get; init; }
    public decimal? PrivateRaiseUsd { get; init; }
    public decimal? TotalRaiseUsd { get; init; }
    public DateOnly? PrivateAnnounceDate { get; init; }
    public DateOnly? ListingDate { get; init; }
    public DateOnly? IdoDate { get; init; }
    public DateOnly? NextUnlockDate { get; init; }
    public decimal? MarketCapUsd { get; init; }
    public decimal? FdmcUsd { get; init; }
    public decimal? Volume24hUsd { get; init; }
    public decimal? CurrentCirculation { get; init; }
    public decimal? TotalCirculation { get; init; }
    public decimal? PercentCirculating { get; init; }
    public decimal? PriceChange24h { get; init; }
    public decimal? PriceChange7d { get; init; }
    public decimal? PriceChange30d { get; init; }
    public decimal? PriceChange1y { get; init; }
    public IReadOnlyList<string> Blockchains { get; init; } = [];
    public IReadOnlyList<ChainBrokerTagDto> Tags { get; init; } = [];
    public IReadOnlyList<Guid> Funds { get; init; } = [];
    public IReadOnlyList<ChainBrokerRefDto> Launchpads { get; init; } = [];
}

public sealed record ChainBrokerTagDto
{
    public required string Name { get; init; }
    public string? Slug { get; init; }
}

public sealed record ChainBrokerRefDto
{
    public required string Slug { get; init; }
    public string? Name { get; init; }
}
