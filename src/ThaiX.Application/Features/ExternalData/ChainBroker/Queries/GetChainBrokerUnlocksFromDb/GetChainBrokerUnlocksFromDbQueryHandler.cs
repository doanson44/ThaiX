using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerUnlocksFromDb;

public sealed class GetChainBrokerUnlocksFromDbQueryHandler
    : IRequestHandler<GetChainBrokerUnlocksFromDbQuery, PagedResult<ChainBrokerUnlockDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetChainBrokerUnlocksFromDbQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ChainBrokerUnlockDto>> Handle(
        GetChainBrokerUnlocksFromDbQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.ChainBrokerUnlocks
            .AsNoTracking()
            .AsQueryable();

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.Search);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(u =>
                (u.Name != null && EF.Functions.Like(EF.Functions.Collate(u.Name, collation), pattern, "\\")) ||
                EF.Functions.Like(EF.Functions.Collate(u.Slug, collation), pattern, "\\") ||
                (u.Ticker != null && EF.Functions.Like(EF.Functions.Collate(u.Ticker, collation), pattern, "\\")));
        }

        // Sort by next unlock date ascending (nearest unlock first), nulls last
        return await query
            .OrderBy(u => u.NextUnlockDate == null ? 1 : 0)
            .ThenBy(u => u.NextUnlockDate)
            .ThenByDescending(u => u.UnlockValueUsd)
            .Select(u => new ChainBrokerUnlockDto
            {
                Id = u.Id,
                Slug = u.Slug,
                Name = u.Name,
                Logo = u.Logo,
                Ticker = u.Ticker,
                NextUnlockDate = u.NextUnlockDate,
                UnlockAmount = u.UnlockAmount,
                UnlockValueUsd = u.UnlockValueUsd,
                RoundName = u.RoundName,
                CirculationPercent = u.CirculationPercent,
                UnlockPercent = u.UnlockPercent,
                Volume24hUsd = u.Volume24hUsd,
                PriceChange24h = u.PriceChange24h,
                PriceChange7d = u.PriceChange7d,
                PriceChange30d = u.PriceChange30d,
                PriceChange1y = u.PriceChange1y
            })
            .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
