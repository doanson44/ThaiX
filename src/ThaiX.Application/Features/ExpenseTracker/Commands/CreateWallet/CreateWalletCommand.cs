using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateWallet;

/// <summary>
/// Command to create a new wallet.
/// </summary>
[InvalidateCache(CacheGroups.Wallets)]
public sealed record CreateWalletCommand : IAppCommand<Guid>
{
    public required string Name { get; init; }
    public required WalletType WalletType { get; init; }
    public required string Currency { get; init; }
    public decimal InitialBalance { get; init; }
}
