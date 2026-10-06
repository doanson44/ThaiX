using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.AssetPositions.Queries.GetSavingPositions;

public sealed class GetSavingPositionsQueryHandler
    : IRequestHandler<GetSavingPositionsQuery, PagedResult<SavingPositionListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSavingPositionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<SavingPositionListItemDto>> Handle(
    GetSavingPositionsQuery request,
    CancellationToken cancellationToken)
    {
        var query = _context.SavingPositions
            .AsNoTracking()
            .Where(p =>
                p.PortfolioId == request.PortfolioId &&
                p.Portfolio.OwnerId == _currentUser.UserId);

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        return await query
            .OrderByDescending(p => p.DepositDate)
            .Select(p => new SavingPositionListItemDto
            {
                Id = p.Id,
                PortfolioId = p.PortfolioId,
                BankName = p.BankName,
                AccountNumber = p.AccountNumber,
                PrincipalAmount = p.PrincipalAmount,
                InterestRate = p.InterestRate,
                InterestType = p.InterestType,
                DepositDate = p.DepositDate,
                MaturityDate = p.MaturityDate,
                Status = p.Status,
                Note = p.Note,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
