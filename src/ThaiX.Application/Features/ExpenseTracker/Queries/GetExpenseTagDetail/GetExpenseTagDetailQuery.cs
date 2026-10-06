using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTagDetail;

/// <summary>
/// Query to retrieve a single expense tag by id.
/// </summary>
public sealed record GetExpenseTagDetailQuery : IAppQuery<ExpenseTagDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.ExpenseTags.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.ExpenseTags;
    public bool IsVersionedList => false;
}
