using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.ChangeCredentialAccountPassword;

[InvalidateCache(CacheGroups.CredentialAccounts)]
public sealed record ChangeCredentialAccountPasswordCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }

    public required string Password { get; init; }
}
