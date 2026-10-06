using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateRecurringTransaction;

/// <summary>
/// Command to create a recurring transaction.
/// </summary>
[InvalidateCache(CacheGroups.RecurringTransactions)]
public sealed record CreateRecurringTransactionCommand : IAppCommand<Guid>
{
    public required Guid WalletId { get; init; }
    public Guid? CategoryId { get; init; }
    public required ExpenseTransactionType TransactionType { get; init; }
    public required decimal Amount { get; init; }
    public required RecurringFrequency Frequency { get; init; }
    public required DateTime NextRun { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Note { get; init; }
}
