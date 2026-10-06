using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetSavingGoalList;

/// <summary>
/// Query to retrieve a paginated list of saving goals.
/// </summary>
public sealed record GetSavingGoalListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseSavingGoalDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term (name).
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.SavingGoals.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.SavingGoals;
    public bool IsVersionedList => true;
}
