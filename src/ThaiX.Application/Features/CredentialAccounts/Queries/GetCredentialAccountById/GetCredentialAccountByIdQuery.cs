using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountById;

public sealed record GetCredentialAccountByIdQuery : IAppQuery<CredentialAccountDetailDto?>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.CredentialAccounts.Detail(Id);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);

    public string CacheGroup => CacheGroups.CredentialAccounts;
}

public sealed record CredentialAccountDetailDto
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public string? Description { get; init; }

    public required bool IsUsed { get; init; }

    public DateTime? UsedAt { get; init; }

    public string? UsedBy { get; init; }

    public required int UsageCount { get; init; }

    public required string LastUsedAgo { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
