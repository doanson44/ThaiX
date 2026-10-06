using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTransactionList;

/// <summary>
/// Handler for GetExpenseTransactionListQuery.
/// </summary>
public sealed class GetExpenseTransactionListQueryHandler : IRequestHandler<GetExpenseTransactionListQuery, PagedResult<ExpenseTransactionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpenseTransactionListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseTransactionDto>> Handle(GetExpenseTransactionListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ExpenseTransactions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => (x.Note ?? string.Empty).ToLower().Contains(s));
        }

        query = request.SortDescending
            ? query.OrderByDescending(x => x.OccurredOn)
            : query.OrderBy(x => x.OccurredOn);

        return await query
            .Select(x => new ExpenseTransactionDto
            {
                Id = x.Id,
                WalletId = x.WalletId,
                CategoryId = x.CategoryId,
                TransactionType = x.TransactionType,
                Amount = x.Amount,
                OccurredOn = x.OccurredOn,
                Note = x.Note
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
