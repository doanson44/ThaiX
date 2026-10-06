using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteRecurringTransaction;

/// <summary>
/// Handler for DeleteRecurringTransactionCommand.
/// </summary>
public sealed class DeleteRecurringTransactionCommandHandler : IRequestHandler<DeleteRecurringTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteRecurringTransactionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteRecurringTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.RecurringTransactions.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Recurring transaction '{request.Id}' not found.");

        entity.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
