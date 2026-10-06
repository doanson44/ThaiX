using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;

/// <summary>
/// Handler for GetMarketScannerRulesQuery.
/// </summary>
public sealed class GetMarketScannerRulesQueryHandler
    : IRequestHandler<GetMarketScannerRulesQuery, PagedResult<MarketScannerRuleListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMarketScannerRulesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<MarketScannerRuleListItemDto>> Handle(
        GetMarketScannerRulesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.MarketScannerRules
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(r => EF.Functions.Like(EF.Functions.Collate(r.Name, collation), pattern, "\\"));
        }

        if (request.IsEnabled.HasValue)
            query = query.Where(r => r.IsEnabled == request.IsEnabled.Value);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "name" : request.SortBy.Trim().ToLowerInvariant();
        query = sortBy switch
        {
            "signaltype" => request.SortDescending ? query.OrderByDescending(r => r.SignalType) : query.OrderBy(r => r.SignalType),
            "window" => request.SortDescending ? query.OrderByDescending(r => r.Window) : query.OrderBy(r => r.Window),
            "threshold" => request.SortDescending ? query.OrderByDescending(r => r.Threshold) : query.OrderBy(r => r.Threshold),
            _ => request.SortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name)
        };

        return await query
            .Select(r => new MarketScannerRuleListItemDto
            {
                Id = r.Id,
                Name = r.Name,
                SignalType = r.SignalType,
                Window = r.Window,
                Threshold = r.Threshold,
                IsEnabled = r.IsEnabled,
                CreatedAt = r.CreatedAt,
                LastUpdated = r.UpdatedAt
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
