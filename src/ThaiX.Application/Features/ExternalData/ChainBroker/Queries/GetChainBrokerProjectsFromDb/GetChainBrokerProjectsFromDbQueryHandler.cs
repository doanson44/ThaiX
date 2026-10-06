using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerProjectsFromDb;

public sealed class GetChainBrokerProjectsFromDbQueryHandler
    : IRequestHandler<GetChainBrokerProjectsFromDbQuery, PagedResult<ChainBrokerProjectDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetChainBrokerProjectsFromDbQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ChainBrokerProjectDto>> Handle(
        GetChainBrokerProjectsFromDbQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.ChainBrokerProjects
            .AsNoTracking()
            .AsQueryable();

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.Search);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(p =>
                (p.Name != null && EF.Functions.Like(EF.Functions.Collate(p.Name, collation), pattern, "\\")) ||
                EF.Functions.Like(EF.Functions.Collate(p.Slug, collation), pattern, "\\") ||
                (p.Ticker != null && EF.Functions.Like(EF.Functions.Collate(p.Ticker, collation), pattern, "\\")));
        }

        return await query
            .OrderBy(p => p.Rank ?? int.MaxValue)
            .ThenBy(p => p.Name ?? p.Slug)
            .Select(p => new ChainBrokerProjectDto
            {
                Id = p.Id,
                Slug = p.Slug,
                Name = p.Name,
                Logo = p.Logo,
                Ticker = p.Ticker,
                CurrentPriceUsd = p.CurrentPriceUsd,
                PublicPriceUsd = p.PublicPriceUsd,
                PrivatePriceUsd = p.PrivatePriceUsd,
                AthPriceUsd = p.AthPriceUsd,
                PublicRoi = p.PublicRoi,
                PrivateRoi = p.PrivateRoi,
                PublicAthRoi = p.PublicAthRoi,
                PrivateAthRoi = p.PrivateAthRoi,
                BrokerScore = p.BrokerScore,
                SecurityScore = p.SecurityScore,
                TwitterScore = p.TwitterScore,
                Rank = p.Rank,
                PublicRaiseUsd = p.PublicRaiseUsd,
                PrivateRaiseUsd = p.PrivateRaiseUsd,
                TotalRaiseUsd = p.TotalRaiseUsd,
                PrivateAnnounceDate = p.PrivateAnnounceDate,
                ListingDate = p.ListingDate,
                IdoDate = p.IdoDate,
                NextUnlockDate = p.NextUnlockDate,
                MarketCapUsd = p.MarketCapUsd,
                FdmcUsd = p.FdmcUsd,
                Volume24hUsd = p.Volume24hUsd,
                CurrentCirculation = p.CurrentCirculation,
                TotalCirculation = p.TotalCirculation,
                PercentCirculating = p.PercentCirculating,
                PriceChange24h = p.PriceChange24h,
                PriceChange7d = p.PriceChange7d,
                PriceChange30d = p.PriceChange30d,
                PriceChange1y = p.PriceChange1y,
                Blockchains = p.Blockchains.Select(b => b.Name).ToList(),
                Tags = p.Tags.Select(t => new ChainBrokerTagDto { Name = t.Name, Slug = t.Slug }).ToList(),
                Funds = p.Funds.Select(f => f.FundId).ToList(),
                Launchpads = p.Launchpads.Select(l => new ChainBrokerRefDto { Slug = l.Slug, Name = l.Name }).ToList()
            })
            .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
