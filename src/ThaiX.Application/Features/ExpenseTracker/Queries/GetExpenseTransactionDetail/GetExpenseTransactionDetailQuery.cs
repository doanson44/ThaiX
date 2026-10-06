using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTransactionDetail;

/// <summary>
/// Query to retrieve a single expense transaction by id.
/// </summary>
public sealed record GetExpenseTransactionDetailQuery : IAppQuery<ExpenseTransactionDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.ExpenseTransactions.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.ExpenseTransactions;
    public bool IsVersionedList => false;
}
