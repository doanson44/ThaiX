using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteWallet;

/// <summary>
/// Command to soft-delete a wallet.
/// </summary>
[InvalidateCache(CacheGroups.Wallets)]
public sealed record DeleteWalletCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
