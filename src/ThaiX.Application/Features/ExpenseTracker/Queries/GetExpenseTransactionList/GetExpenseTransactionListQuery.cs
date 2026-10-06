using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTransactionList;

/// <summary>
/// Query to retrieve a paginated list of expense transactions.
/// </summary>
public sealed record GetExpenseTransactionListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseTransactionDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term (note).
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.ExpenseTransactions.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.ExpenseTransactions;
    public bool IsVersionedList => true;
}
