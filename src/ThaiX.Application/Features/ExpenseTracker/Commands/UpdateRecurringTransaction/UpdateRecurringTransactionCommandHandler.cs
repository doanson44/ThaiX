using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateRecurringTransaction;

/// <summary>
/// Handler for UpdateRecurringTransactionCommand.
/// </summary>
public sealed class UpdateRecurringTransactionCommandHandler : IRequestHandler<UpdateRecurringTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateRecurringTransactionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateRecurringTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.RecurringTransactions.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Recurring transaction '{request.Id}' not found.");

        entity.Update(
            request.WalletId,
            request.CategoryId,
            request.TransactionType,
            request.Amount,
            request.Frequency,
            request.NextRun,
            request.EndDate,
            request.IsActive,
            request.Note);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
