using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTagList;

/// <summary>
/// Query to retrieve a paginated list of expense tags.
/// </summary>
public sealed record GetExpenseTagListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseTagDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term (name).
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.ExpenseTags.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.ExpenseTags;
    public bool IsVersionedList => true;
}
