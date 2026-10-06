using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;

namespace ThaiX.Application.Features.MarketScanner.Queries.GetAllMarketScannerRules;

/// <summary>
/// Handler for GetAllMarketScannerRulesQuery.
/// </summary>
public sealed class GetAllMarketScannerRulesQueryHandler
    : IRequestHandler<GetAllMarketScannerRulesQuery, IReadOnlyList<MarketScannerRuleListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllMarketScannerRulesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MarketScannerRuleListItemDto>> Handle(
        GetAllMarketScannerRulesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.MarketScannerRules
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        if (request.IsEnabled.HasValue)
            query = query.Where(r => r.IsEnabled == request.IsEnabled.Value);

        return await query
            .OrderBy(r => r.SignalType)
            .ThenBy(r => r.Window)
            .ThenBy(r => r.Name)
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
            .ToListAsync(cancellationToken);
    }
}
