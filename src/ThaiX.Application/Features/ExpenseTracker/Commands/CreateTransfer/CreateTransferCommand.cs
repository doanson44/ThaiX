using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateTransfer;

/// <summary>
/// Command to create a transfer between wallets.
/// </summary>
[InvalidateCache(CacheGroups.Transfers)]
[InvalidateCache(CacheGroups.Wallets)]
[InvalidateCache(CacheGroups.ExpenseDashboard)]
public sealed record CreateTransferCommand : IAppCommand<Guid>
{
    public required Guid SourceWalletId { get; init; }
    public required Guid TargetWalletId { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime TransferredOn { get; init; }
    public string? Note { get; init; }
}
