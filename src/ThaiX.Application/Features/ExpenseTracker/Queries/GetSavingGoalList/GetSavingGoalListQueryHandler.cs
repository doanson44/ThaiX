using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetSavingGoalList;

/// <summary>
/// Handler for GetSavingGoalListQuery.
/// </summary>
public sealed class GetSavingGoalListQueryHandler : IRequestHandler<GetSavingGoalListQuery, PagedResult<ExpenseSavingGoalDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSavingGoalListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseSavingGoalDto>> Handle(GetSavingGoalListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.SavingGoals.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(s));
        }

        query = request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        return await query
            .Select(x => new ExpenseSavingGoalDto
            {
                Id = x.Id,
                WalletId = x.WalletId,
                Name = x.Name,
                TargetAmount = x.TargetAmount,
                CurrentAmount = x.CurrentAmount,
                TargetDate = x.TargetDate
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
