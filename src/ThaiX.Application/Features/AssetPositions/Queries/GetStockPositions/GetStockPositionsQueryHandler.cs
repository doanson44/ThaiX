using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.AssetPositions.Queries.GetStockPositions;

public sealed class GetStockPositionsQueryHandler
    : IRequestHandler<GetStockPositionsQuery, PagedResult<StockPositionListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetStockPositionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<StockPositionListItemDto>> Handle(
    GetStockPositionsQuery request,
    CancellationToken cancellationToken)
    {
        var query = _context.StockPositions
            .AsNoTracking()
            .Where(p =>
                p.PortfolioId == request.PortfolioId &&
                p.Portfolio.OwnerId == _currentUser.UserId);

        if (request.IsClosed.HasValue)
        {
            query = query.Where(p => p.IsClosed == request.IsClosed.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new StockPositionListItemDto
            {
                Id = p.Id,
                PortfolioId = p.PortfolioId,
                Symbol = p.Symbol,
                Exchange = p.Exchange,
                Quantity = p.Quantity,
                AverageEntryPrice = p.AverageEntryPrice,
                TotalInvested = p.TotalInvested,
                RealizedPnl = p.RealizedPnl,
                TargetPrice = p.TargetPrice,
                StopLoss = p.StopLoss,
                Note = p.Note,
                IsClosed = p.IsClosed,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
