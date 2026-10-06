using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetRecurringTransactionList;

/// <summary>
/// Handler for GetRecurringTransactionListQuery.
/// </summary>
public sealed class GetRecurringTransactionListQueryHandler : IRequestHandler<GetRecurringTransactionListQuery, PagedResult<ExpenseRecurringTransactionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetRecurringTransactionListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseRecurringTransactionDto>> Handle(GetRecurringTransactionListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.RecurringTransactions.AsNoTracking().AsQueryable();
        query = request.SortDescending
            ? query.OrderByDescending(x => x.NextRun)
            : query.OrderBy(x => x.NextRun);

        return await query
            .Select(x => new ExpenseRecurringTransactionDto
            {
                Id = x.Id,
                WalletId = x.WalletId,
                CategoryId = x.CategoryId,
                TransactionType = x.TransactionType,
                Amount = x.Amount,
                Frequency = x.Frequency,
                NextRun = x.NextRun,
                EndDate = x.EndDate,
                IsActive = x.IsActive,
                Note = x.Note
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
