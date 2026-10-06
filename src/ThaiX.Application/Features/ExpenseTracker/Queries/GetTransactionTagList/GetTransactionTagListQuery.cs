using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransactionTagList;

/// <summary>
/// Query to retrieve a paginated list of transaction-tag links.
/// </summary>
public sealed record GetTransactionTagListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseTransactionTagDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term.
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.TransactionTags.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.TransactionTags;
    public bool IsVersionedList => true;
}
