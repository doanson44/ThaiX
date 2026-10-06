using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.AssetPositions.Queries.GetCryptoPositions;

public sealed class GetCryptoPositionsQueryHandler
    : IRequestHandler<GetCryptoPositionsQuery, PagedResult<CryptoPositionListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCryptoPositionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CryptoPositionListItemDto>> Handle(
        GetCryptoPositionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.CryptoPositions
            .AsNoTracking()
            .Where(p =>
                p.PortfolioId == request.PortfolioId &&
                !p.IsDeleted &&
                p.Portfolio.OwnerId == _currentUser.UserId);

        if (request.IsClosed.HasValue)
        {
            query = query.Where(p => p.IsClosed == request.IsClosed.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new CryptoPositionListItemDto
            {
                Id = p.Id,
                PortfolioId = p.PortfolioId,
                Symbol = p.Symbol,
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
