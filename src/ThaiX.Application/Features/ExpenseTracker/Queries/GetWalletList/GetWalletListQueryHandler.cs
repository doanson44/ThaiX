using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetWalletList;

/// <summary>
/// Handler for GetWalletListQuery.
/// </summary>
public sealed class GetWalletListQueryHandler : IRequestHandler<GetWalletListQuery, PagedResult<ExpenseWalletDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetWalletListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseWalletDto>> Handle(GetWalletListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Wallets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(s) || x.Currency.ToLower().Contains(s));
        }

        query = request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        return await query
            .Select(x => new ExpenseWalletDto
            {
                Id = x.Id,
                Name = x.Name,
                WalletType = x.WalletType,
                Currency = x.Currency,
                CurrentBalance = x.CurrentBalance
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
