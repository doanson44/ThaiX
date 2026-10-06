using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetRecurringTransactionList;

/// <summary>
/// Query to retrieve a paginated list of recurring transactions.
/// </summary>
public sealed record GetRecurringTransactionListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseRecurringTransactionDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term.
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.RecurringTransactions.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.RecurringTransactions;
    public bool IsVersionedList => true;
}
