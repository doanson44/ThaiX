using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.MarkCredentialAccountUsed;

[InvalidateCache(CacheGroups.CredentialAccounts)]
public sealed record MarkCredentialAccountUsedCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }
}
