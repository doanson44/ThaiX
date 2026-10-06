using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.DeleteCredentialAccount;

[InvalidateCache(CacheGroups.CredentialAccounts)]
public sealed record DeleteCredentialAccountCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }
}
