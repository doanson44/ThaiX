using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateWallet;

/// <summary>
/// Handler for CreateWalletCommand.
/// </summary>
public sealed class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateWalletCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = Wallet.Create(request.Name, request.WalletType, request.Currency, request.InitialBalance);
        _dbContext.Wallets.Add(wallet);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return wallet.Id;
    }
}
