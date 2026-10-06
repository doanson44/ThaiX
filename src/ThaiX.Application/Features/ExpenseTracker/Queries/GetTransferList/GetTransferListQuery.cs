using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransferList;

/// <summary>
/// Query to retrieve a paginated list of transfers.
/// </summary>
public sealed record GetTransferListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseTransferDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term (note).
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.Transfers.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.Transfers;
    public bool IsVersionedList => true;
}
