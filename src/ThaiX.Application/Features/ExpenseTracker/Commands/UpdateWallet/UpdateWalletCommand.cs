using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateWallet;

/// <summary>
/// Command to update an existing wallet.
/// </summary>
[InvalidateCache(CacheGroups.Wallets)]
public sealed record UpdateWalletCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required WalletType WalletType { get; init; }
    public required string Currency { get; init; }
}
