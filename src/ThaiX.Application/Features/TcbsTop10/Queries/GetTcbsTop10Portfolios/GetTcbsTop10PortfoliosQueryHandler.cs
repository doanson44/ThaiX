using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.TcbsTop10;

namespace ThaiX.Application.Features.TcbsTop10.Queries.GetTcbsTop10Portfolios;

public sealed class GetTcbsTop10PortfoliosQueryHandler
    : IRequestHandler<GetTcbsTop10PortfoliosQuery, TcbsTop10PortfoliosResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTcbsTop10PortfoliosQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TcbsTop10PortfoliosResult> Handle(
        GetTcbsTop10PortfoliosQuery request,
        CancellationToken cancellationToken)
    {
        var portfolioEntities = await _dbContext.TcbsTop10Portfolios
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Tickers)
            .Include(p => p.Attachments)
            .OrderByDescending(p => p.PostedAt)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var portfolios = portfolioEntities
            .Select(p => new TcbsTop10PortfolioDto
            {
                Id = p.Id,
                SourceContentId = p.SourceContentId,
                PostedAt = p.PostedAt,
                EffectiveDate = p.EffectiveDate,
                AddedTickers = p.Tickers
                    .Where(t => t.ChangeType == TcbsPortfolioChangeType.Added)
                    .OrderBy(t => t.Ticker)
                    .Select(t => t.Ticker)
                    .ToList(),
                RemovedTickers = p.Tickers
                    .Where(t => t.ChangeType == TcbsPortfolioChangeType.Removed)
                    .OrderBy(t => t.Ticker)
                    .Select(t => t.Ticker)
                    .ToList(),
                Attachments = p.Attachments
                    .OrderBy(a => a.Kind)
                    .ThenBy(a => a.ImageType)
                    .Select(a => new TcbsTop10AttachmentDto
                    {
                        Kind = (int)a.Kind,
                        ImageType = a.ImageType.HasValue ? (int)a.ImageType.Value : null,
                        FileGuid = a.FileGuid,
                        Url = a.Url
                    })
                    .ToList()
            })
            .ToList();

        var currentHoldings = TcbsTop10Portfolio.DeriveCurrentHoldings(portfolioEntities);

        return new TcbsTop10PortfoliosResult
        {
            Portfolios = portfolios,
            CurrentHoldings = currentHoldings,
            AllTimeHoldings = [.. portfolioEntities
                .SelectMany(p => p.Tickers)
                .Where(t => t.ChangeType == TcbsPortfolioChangeType.Added)
                .Select(t => t.Ticker)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t, StringComparer.Ordinal)]
        };
    }

}
