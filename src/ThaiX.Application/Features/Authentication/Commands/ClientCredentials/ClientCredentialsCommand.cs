using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Authentication.Commands.Login;

namespace ThaiX.Application.Features.Authentication.Commands.ClientCredentials;

/// <summary>
/// Command for M2M client credentials authentication.
/// Returns JWT token for system actors.
/// </summary>
public sealed record ClientCredentialsCommand : IAppQuery<TokenDto>
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public string? Scope { get; init; }
}
