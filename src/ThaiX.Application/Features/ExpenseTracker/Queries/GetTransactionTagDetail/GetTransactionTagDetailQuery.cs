using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransactionTagDetail;

/// <summary>
/// Query to retrieve a single transaction-tag link by id.
/// </summary>
public sealed record GetTransactionTagDetailQuery : IAppQuery<ExpenseTransactionTagDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.TransactionTags.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.TransactionTags;
    public bool IsVersionedList => false;
}
