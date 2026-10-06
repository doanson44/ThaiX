using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccounts;

public sealed record GetCredentialAccountsQuery : PagedRequest, IAppQuery<PagedResult<CredentialAccountDto>>, ICacheableQuery
{
    public string? SearchTerm { get; init; }

    public bool? IsUsed { get; init; }

    public string CacheKey => CacheKeys.CredentialAccounts.List(SearchTerm, IsUsed, PageNumber, PageSize);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);

    public string CacheGroup => CacheGroups.CredentialAccounts;

    public bool IsVersionedList => true;
}

public sealed record CredentialAccountDto
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public string? Description { get; init; }

    public required bool IsUsed { get; init; }

    public DateTime? UsedAt { get; init; }

    public string? UsedBy { get; init; }

    public required int UsageCount { get; init; }

    public required string LastUsedAgo { get; init; }
}
