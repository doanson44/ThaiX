using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetBudgetDetail;

/// <summary>
/// Query to retrieve a single budget by id.
/// </summary>
public sealed record GetBudgetDetailQuery : IAppQuery<ExpenseBudgetDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.Budgets.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.Budgets;
    public bool IsVersionedList => false;
}
