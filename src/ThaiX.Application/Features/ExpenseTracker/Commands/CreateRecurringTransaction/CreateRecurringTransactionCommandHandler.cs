using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateRecurringTransaction;

/// <summary>
/// Handler for CreateRecurringTransactionCommand.
/// </summary>
public sealed class CreateRecurringTransactionCommandHandler : IRequestHandler<CreateRecurringTransactionCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateRecurringTransactionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateRecurringTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = RecurringTransaction.Create(
            request.WalletId,
            request.CategoryId,
            request.TransactionType,
            request.Amount,
            request.Frequency,
            request.NextRun,
            request.EndDate,
            request.Note);
        _dbContext.RecurringTransactions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
