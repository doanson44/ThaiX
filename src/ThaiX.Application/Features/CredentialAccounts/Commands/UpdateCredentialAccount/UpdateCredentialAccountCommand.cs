using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.UpdateCredentialAccount;

[InvalidateCache(CacheGroups.CredentialAccounts)]
public sealed record UpdateCredentialAccountCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public string? Description { get; init; }
}
