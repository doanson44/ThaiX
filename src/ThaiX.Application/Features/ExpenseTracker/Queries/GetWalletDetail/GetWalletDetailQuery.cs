using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetWalletDetail;

/// <summary>
/// Query to retrieve a single wallet by id.
/// </summary>
public sealed record GetWalletDetailQuery : IAppQuery<ExpenseWalletDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.Wallets.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.Wallets;
    public bool IsVersionedList => false;
}
