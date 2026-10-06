using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.CreateCredentialAccount;

[InvalidateCache(CacheGroups.CredentialAccounts)]
public sealed record CreateCredentialAccountCommand : IAppCommand<Guid>
{
    public required string Username { get; init; }

    public required string Password { get; init; }

    public string? Description { get; init; }
}
