using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.ResetCredentialAccountUsed;

[InvalidateCache(CacheGroups.CredentialAccounts)]
public sealed record ResetCredentialAccountUsedCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }
}
