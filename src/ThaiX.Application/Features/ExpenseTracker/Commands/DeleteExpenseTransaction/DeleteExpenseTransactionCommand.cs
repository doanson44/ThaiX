using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseTransaction;

/// <summary>
/// Command to soft-delete an expense or income transaction.
/// </summary>
[InvalidateCache(CacheGroups.ExpenseTransactions)]
[InvalidateCache(CacheGroups.Wallets)]
[InvalidateCache(CacheGroups.ExpenseDashboard)]
public sealed record DeleteExpenseTransactionCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
