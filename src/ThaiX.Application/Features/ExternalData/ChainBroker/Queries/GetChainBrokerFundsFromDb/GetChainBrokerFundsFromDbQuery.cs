using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerFundsFromDb;

public sealed record GetChainBrokerFundsFromDbQuery : IAppQuery<PagedResult<ChainBrokerFundDto>>, ICacheableQuery
{
    public string? Search { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => CacheKeys.ChainBrokerData.FundsList(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromHours(6);
    public string CacheGroup => CacheGroups.ChainBrokerData;
    public bool IsVersionedList => false;
}

public sealed record ChainBrokerFundDto
{
    public required Guid Id { get; init; }
    public required string Slug { get; init; }
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? FundTypeName { get; init; }
    public string? FundTypeSlug { get; init; }
    public DateOnly? LastInvestmentDate { get; init; }
    public int? YearFounded { get; init; }
    public string? Status { get; init; }
    public decimal? AverageCurrentRoi { get; init; }
    public decimal? AverageMarketCapUsd { get; init; }
    public decimal? AverageInitialMarketCapUsd { get; init; }
    public decimal? AverageFdmcUsd { get; init; }
    public decimal? AverageInitialFdmcUsd { get; init; }
    public decimal? AveragePublicRaiseUsd { get; init; }
    public decimal? AveragePrivateRaiseUsd { get; init; }
    public decimal? AverageTotalRaiseUsd { get; init; }
    public decimal? AveragePriceChange24h { get; init; }
    public decimal? AveragePriceChange7d { get; init; }
    public decimal? AveragePriceChange30d { get; init; }
    public decimal? AveragePriceChange1y { get; init; }
    public int ProjectCount { get; init; }
    public int? GainersCount { get; init; }
    public decimal? GainersPercent { get; init; }
    public int? LosersCount { get; init; }
    public decimal? LosersPercent { get; init; }
}
