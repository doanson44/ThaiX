using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetRecurringTransactionDetail;

/// <summary>
/// Query to retrieve a single recurring transaction by id.
/// </summary>
public sealed record GetRecurringTransactionDetailQuery : IAppQuery<ExpenseRecurringTransactionDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.RecurringTransactions.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.RecurringTransactions;
    public bool IsVersionedList => false;
}
