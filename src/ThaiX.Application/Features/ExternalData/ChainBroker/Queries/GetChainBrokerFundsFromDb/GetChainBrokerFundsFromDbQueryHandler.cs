using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerFundsFromDb;

public sealed class GetChainBrokerFundsFromDbQueryHandler
    : IRequestHandler<GetChainBrokerFundsFromDbQuery, PagedResult<ChainBrokerFundDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetChainBrokerFundsFromDbQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ChainBrokerFundDto>> Handle(
        GetChainBrokerFundsFromDbQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.ChainBrokerFunds
            .AsNoTracking()
            .AsQueryable();

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.Search);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(f =>
                (f.Name != null && EF.Functions.Like(EF.Functions.Collate(f.Name, collation), pattern, "\\")) ||
                EF.Functions.Like(EF.Functions.Collate(f.Slug, collation), pattern, "\\"));
        }

        return await query
            .OrderBy(f => f.Name ?? f.Slug)
            .Select(f => new ChainBrokerFundDto
            {
                Id = f.Id,
                Slug = f.Slug,
                Name = f.Name,
                Logo = f.Logo,
                FundTypeName = f.FundTypeName,
                FundTypeSlug = f.FundTypeSlug,
                LastInvestmentDate = f.LastInvestmentDate,
                YearFounded = f.YearFounded,
                Status = f.Status,
                AverageCurrentRoi = f.AverageCurrentRoi,
                AverageMarketCapUsd = f.AverageMarketCapUsd,
                AverageInitialMarketCapUsd = f.AverageInitialMarketCapUsd,
                AverageFdmcUsd = f.AverageFdmcUsd,
                AverageInitialFdmcUsd = f.AverageInitialFdmcUsd,
                AveragePublicRaiseUsd = f.AveragePublicRaiseUsd,
                AveragePrivateRaiseUsd = f.AveragePrivateRaiseUsd,
                AverageTotalRaiseUsd = f.AverageTotalRaiseUsd,
                AveragePriceChange24h = f.AveragePriceChange24h,
                AveragePriceChange7d = f.AveragePriceChange7d,
                AveragePriceChange30d = f.AveragePriceChange30d,
                AveragePriceChange1y = f.AveragePriceChange1y,
                ProjectCount = f.ProjectCount,
                GainersCount = f.GainersCount,
                GainersPercent = f.GainersPercent,
                LosersCount = f.LosersCount,
                LosersPercent = f.LosersPercent
            })
            .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
