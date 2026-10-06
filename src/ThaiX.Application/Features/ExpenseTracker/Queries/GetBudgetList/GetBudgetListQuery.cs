using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetBudgetList;

/// <summary>
/// Query to retrieve a paginated list of budgets.
/// </summary>
public sealed record GetBudgetListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseBudgetDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term.
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.Budgets.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.Budgets;
    public bool IsVersionedList => true;
}
