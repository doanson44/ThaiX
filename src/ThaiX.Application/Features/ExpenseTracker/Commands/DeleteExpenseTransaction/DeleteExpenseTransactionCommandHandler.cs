using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Helpers;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseTransaction;

/// <summary>
/// Handler for DeleteExpenseTransactionCommand.
/// </summary>
public sealed class DeleteExpenseTransactionCommandHandler : IRequestHandler<DeleteExpenseTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteExpenseTransactionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteExpenseTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ExpenseTransactions.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transaction '{request.Id}' not found.");

        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == entity.WalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{entity.WalletId}' not found.");

        ExpenseTransactionWalletBalanceHelper.ApplyWallet(wallet, entity.TransactionType, entity.Amount, reverse: true);
        entity.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
