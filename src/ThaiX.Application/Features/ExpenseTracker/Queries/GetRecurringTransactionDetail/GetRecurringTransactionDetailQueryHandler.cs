using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetRecurringTransactionDetail;

/// <summary>
/// Handler for GetRecurringTransactionDetailQuery.
/// </summary>
public sealed class GetRecurringTransactionDetailQueryHandler : IRequestHandler<GetRecurringTransactionDetailQuery, ExpenseRecurringTransactionDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetRecurringTransactionDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseRecurringTransactionDto> Handle(GetRecurringTransactionDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.RecurringTransactions
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Recurring transaction '{request.Id}' not found.");
    }
}
