using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTransactionDetail;

/// <summary>
/// Handler for GetExpenseTransactionDetailQuery.
/// </summary>
public sealed class GetExpenseTransactionDetailQueryHandler : IRequestHandler<GetExpenseTransactionDetailQuery, ExpenseTransactionDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpenseTransactionDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseTransactionDto> Handle(GetExpenseTransactionDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.ExpenseTransactions
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transaction '{request.Id}' not found.");
    }
}
