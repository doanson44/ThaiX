using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteTransfer;

/// <summary>
/// Handler for DeleteTransferCommand.
/// </summary>
public sealed class DeleteTransferCommandHandler : IRequestHandler<DeleteTransferCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteTransferCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteTransferCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Transfers.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transfer '{request.Id}' not found.");

        var source = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == entity.SourceWalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{entity.SourceWalletId}' not found.");
        var target = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == entity.TargetWalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{entity.TargetWalletId}' not found.");

        source.ApplyIncome(entity.Amount);
        target.ApplyExpense(entity.Amount);
        entity.SoftDelete();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
