using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetBudgetList;

/// <summary>
/// Handler for GetBudgetListQuery.
/// </summary>
public sealed class GetBudgetListQueryHandler : IRequestHandler<GetBudgetListQuery, PagedResult<ExpenseBudgetDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBudgetListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseBudgetDto>> Handle(GetBudgetListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Budgets.AsNoTracking().AsQueryable();
        query = request.SortDescending
            ? query.OrderByDescending(x => x.StartDate)
            : query.OrderBy(x => x.StartDate);

        return await query
            .Select(x => new ExpenseBudgetDto
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                Period = x.Period,
                Amount = x.Amount,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
