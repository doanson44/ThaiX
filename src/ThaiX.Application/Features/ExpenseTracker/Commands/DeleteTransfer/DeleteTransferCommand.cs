using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteTransfer;

/// <summary>
/// Command to soft-delete a transfer and reverse wallet balances.
/// </summary>
[InvalidateCache(CacheGroups.Transfers)]
[InvalidateCache(CacheGroups.Wallets)]
[InvalidateCache(CacheGroups.ExpenseDashboard)]
public sealed record DeleteTransferCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
