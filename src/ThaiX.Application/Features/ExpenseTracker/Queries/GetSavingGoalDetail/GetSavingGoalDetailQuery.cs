using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetSavingGoalDetail;

/// <summary>
/// Query to retrieve a single saving goal by id.
/// </summary>
public sealed record GetSavingGoalDetailQuery : IAppQuery<ExpenseSavingGoalDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.SavingGoals.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.SavingGoals;
    public bool IsVersionedList => false;
}
