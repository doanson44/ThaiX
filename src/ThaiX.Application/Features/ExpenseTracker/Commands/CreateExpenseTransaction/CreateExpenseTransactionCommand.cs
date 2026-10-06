using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseTransaction;

/// <summary>
/// Command to create an expense or income transaction.
/// </summary>
[InvalidateCache(CacheGroups.ExpenseTransactions)]
[InvalidateCache(CacheGroups.Wallets)]
[InvalidateCache(CacheGroups.ExpenseDashboard)]
public sealed record CreateExpenseTransactionCommand : IAppCommand<Guid>
{
    public required Guid WalletId { get; init; }
    public Guid? CategoryId { get; init; }
    public required ExpenseTransactionType TransactionType { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime OccurredOn { get; init; }
    public string? Note { get; init; }
}
