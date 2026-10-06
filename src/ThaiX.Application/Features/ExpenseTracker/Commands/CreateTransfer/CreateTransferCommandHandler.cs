using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateTransfer;

/// <summary>
/// Handler for CreateTransferCommand.
/// </summary>
public sealed class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateTransferCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var source = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == request.SourceWalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{request.SourceWalletId}' not found.");
        var target = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == request.TargetWalletId && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{request.TargetWalletId}' not found.");

        var entity = Transfer.Create(request.SourceWalletId, request.TargetWalletId, request.Amount, request.TransferredOn, request.Note);

        source.ApplyExpense(request.Amount);
        target.ApplyIncome(request.Amount);

        _dbContext.Transfers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
