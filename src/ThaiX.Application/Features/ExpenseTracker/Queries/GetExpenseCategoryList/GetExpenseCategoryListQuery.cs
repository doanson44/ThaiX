using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseCategoryList;

/// <summary>
/// Query to retrieve a paginated list of expense categories.
/// </summary>
public sealed record GetExpenseCategoryListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseCategoryDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term (name).
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.ExpenseCategories.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.ExpenseCategories;
    public bool IsVersionedList => true;
}
