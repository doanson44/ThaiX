using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateRecurringTransaction;

/// <summary>
/// Command to update a recurring transaction.
/// </summary>
[InvalidateCache(CacheGroups.RecurringTransactions)]
public sealed record UpdateRecurringTransactionCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required Guid WalletId { get; init; }
    public Guid? CategoryId { get; init; }
    public required ExpenseTransactionType TransactionType { get; init; }
    public required decimal Amount { get; init; }
    public required RecurringFrequency Frequency { get; init; }
    public required DateTime NextRun { get; init; }
    public DateTime? EndDate { get; init; }
    public required bool IsActive { get; init; }
    public string? Note { get; init; }
}
