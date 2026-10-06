using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseCategoryDetail;

/// <summary>
/// Query to retrieve a single expense category by id.
/// </summary>
public sealed record GetExpenseCategoryDetailQuery : IAppQuery<ExpenseCategoryDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.ExpenseCategories.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.ExpenseCategories;
    public bool IsVersionedList => false;
}
