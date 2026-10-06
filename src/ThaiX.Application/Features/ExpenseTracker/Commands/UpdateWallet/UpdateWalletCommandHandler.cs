using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateWallet;

/// <summary>
/// Handler for UpdateWalletCommand.
/// </summary>
public sealed class UpdateWalletCommandHandler : IRequestHandler<UpdateWalletCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateWalletCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Wallet '{request.Id}' not found.");

        wallet.Update(request.Name, request.WalletType, request.Currency);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
