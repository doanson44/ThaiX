using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PriceAlerts.Queries.GetAllPriceAlerts;

/// <summary>
/// Handler for GetAllPriceAlertsQuery.
/// Returns all enabled, non-deleted price alerts ordered by asset type and symbol.
/// Uses AsNoTracking for read-only access.
/// </summary>
public sealed class GetAllPriceAlertsQueryHandler
    : IRequestHandler<GetAllPriceAlertsQuery, IReadOnlyList<PriceAlertDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPriceAlertsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PriceAlertDto>> Handle(
        GetAllPriceAlertsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PriceAlerts
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.IsEnabled)
            .OrderBy(a => a.AssetType)
            .ThenBy(a => a.Symbol)
            .Select(a => new PriceAlertDto
            {
                Id = a.Id,
                Symbol = a.Symbol,
                AssetType = a.AssetType,
                Condition = a.Condition,
                TargetPrice = a.TargetPrice,
                Note = a.Note,
                IsOneTime = a.IsOneTime,
                LastTriggeredAt = a.LastTriggeredAt
            })
            .ToListAsync(cancellationToken);
    }
}
