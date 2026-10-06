using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateTransfer;

/// <summary>
/// Handler for UpdateTransferCommand.
/// </summary>
public sealed class UpdateTransferCommandHandler : IRequestHandler<UpdateTransferCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateTransferCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateTransferCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Transfers.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transfer '{request.Id}' not found.");

        var oldSource = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == entity.SourceWalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{entity.SourceWalletId}' not found.");
        var oldTarget = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == entity.TargetWalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{entity.TargetWalletId}' not found.");
        var newSource = entity.SourceWalletId == request.SourceWalletId
            ? oldSource
            : await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == request.SourceWalletId && !x.IsDeleted, cancellationToken)
                ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{request.SourceWalletId}' not found.");
        var newTarget = entity.TargetWalletId == request.TargetWalletId
            ? oldTarget
            : await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == request.TargetWalletId && !x.IsDeleted, cancellationToken)
                ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{request.TargetWalletId}' not found.");

        oldSource.ApplyIncome(entity.Amount);
        oldTarget.ApplyExpense(entity.Amount);

        newSource.ApplyExpense(request.Amount);
        newTarget.ApplyIncome(request.Amount);

        entity.Update(request.SourceWalletId, request.TargetWalletId, request.Amount, request.TransferredOn, request.Note);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
