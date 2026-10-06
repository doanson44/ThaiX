using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.PriceAlerts.Queries.GetPriceAlerts;

public sealed class GetPriceAlertsQueryHandler
    : IRequestHandler<GetPriceAlertsQuery, PagedResult<PriceAlertListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPriceAlertsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PriceAlertListItemDto>> Handle(
        GetPriceAlertsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PriceAlerts
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(a => EF.Functions.Like(EF.Functions.Collate(a.Symbol, collation), pattern, "\\"));
        }

        if (request.IsEnabled.HasValue)
            query = query.Where(a => a.IsEnabled == request.IsEnabled.Value);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "symbol" : request.SortBy.Trim().ToLowerInvariant();
        query = sortBy switch
        {
            "assettype" => request.SortDescending ? query.OrderByDescending(a => a.AssetType) : query.OrderBy(a => a.AssetType),
            "condition" => request.SortDescending ? query.OrderByDescending(a => a.Condition) : query.OrderBy(a => a.Condition),
            "targetprice" => request.SortDescending ? query.OrderByDescending(a => a.TargetPrice) : query.OrderBy(a => a.TargetPrice),
            "triggercount" => request.SortDescending ? query.OrderByDescending(a => a.TriggerCount) : query.OrderBy(a => a.TriggerCount),
            _ => request.SortDescending ? query.OrderByDescending(a => a.Symbol) : query.OrderBy(a => a.Symbol)
        };

        return await query
            .Select(a => new PriceAlertListItemDto
            {
                Id = a.Id,
                Symbol = a.Symbol,
                AssetType = a.AssetType,
                Condition = a.Condition,
                TargetPrice = a.TargetPrice,
                Note = a.Note,
                IsEnabled = a.IsEnabled,
                IsOneTime = a.IsOneTime,
                LastTriggeredAt = a.LastTriggeredAt,
                TriggerCount = a.TriggerCount,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
