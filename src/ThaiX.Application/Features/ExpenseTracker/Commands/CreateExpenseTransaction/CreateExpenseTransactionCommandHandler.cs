using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Helpers;
using ExpenseTransaction = ThaiX.Domain.Aggregates.ExpenseTracker.Transaction;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseTransaction;

/// <summary>
/// Handler for CreateExpenseTransactionCommand.
/// </summary>
public sealed class CreateExpenseTransactionCommandHandler : IRequestHandler<CreateExpenseTransactionCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateExpenseTransactionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateExpenseTransactionCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == request.WalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{request.WalletId}' not found.");

        var entity = ExpenseTransaction.Create(
            request.WalletId,
            request.CategoryId,
            request.TransactionType,
            request.Amount,
            request.OccurredOn,
            request.Note);

        ExpenseTransactionWalletBalanceHelper.ApplyWallet(wallet, request.TransactionType, request.Amount, reverse: false);
        _dbContext.ExpenseTransactions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
