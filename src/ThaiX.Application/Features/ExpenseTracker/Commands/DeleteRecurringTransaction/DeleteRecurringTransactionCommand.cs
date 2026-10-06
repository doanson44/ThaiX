using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteRecurringTransaction;

/// <summary>
/// Command to soft-delete a recurring transaction.
/// </summary>
[InvalidateCache(CacheGroups.RecurringTransactions)]
public sealed record DeleteRecurringTransactionCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
