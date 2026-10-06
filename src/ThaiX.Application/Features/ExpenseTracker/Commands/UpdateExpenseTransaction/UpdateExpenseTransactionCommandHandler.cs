using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Helpers;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseTransaction;

/// <summary>
/// Handler for UpdateExpenseTransactionCommand.
/// </summary>
public sealed class UpdateExpenseTransactionCommandHandler : IRequestHandler<UpdateExpenseTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateExpenseTransactionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateExpenseTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ExpenseTransactions.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transaction '{request.Id}' not found.");

        var oldWallet = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == entity.WalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{entity.WalletId}' not found.");
        var newWallet = entity.WalletId == request.WalletId
            ? oldWallet
            : await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == request.WalletId && !x.IsDeleted, cancellationToken)
                ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{request.WalletId}' not found.");

        ExpenseTransactionWalletBalanceHelper.ApplyWallet(oldWallet, entity.TransactionType, entity.Amount, reverse: true);
        ExpenseTransactionWalletBalanceHelper.ApplyWallet(newWallet, request.TransactionType, request.Amount, reverse: false);

        entity.Update(request.WalletId, request.CategoryId, request.TransactionType, request.Amount, request.OccurredOn, request.Note);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
