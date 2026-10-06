using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransferDetail;

/// <summary>
/// Query to retrieve a single transfer by id.
/// </summary>
public sealed record GetTransferDetailQuery : IAppQuery<ExpenseTransferDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.Transfers.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.Transfers;
    public bool IsVersionedList => false;
}
