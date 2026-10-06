using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetWalletList;

/// <summary>
/// Query to retrieve a paginated list of wallets.
/// </summary>
public sealed record GetWalletListQuery : PagedRequest, IAppQuery<PagedResult<ExpenseWalletDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term (name or currency).
    /// </summary>
    public string? Search { get; init; }

    public string CacheKey => CacheKeys.Wallets.List(Search, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.Wallets;
    public bool IsVersionedList => true;
}
