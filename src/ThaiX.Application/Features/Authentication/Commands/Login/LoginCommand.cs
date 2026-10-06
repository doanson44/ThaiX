using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Command for user login.
/// Supports both username and email for authentication.
/// Returns JWT token on successful authentication or 2FA challenge.
/// </summary>
public sealed record LoginCommand : IAppQuery<LoginResultDto>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public bool RememberMe { get; init; }
}

/// <summary>
/// DTO returned on successful login.
/// </summary>
public sealed record TokenDto
{
    public required string AccessToken { get; init; }
    public required string TokenType { get; init; }
    public required int ExpiresIn { get; init; }
    public required string Scope { get; init; }
}

/// <summary>
/// DTO returned from login that may require 2FA.
/// </summary>
public sealed record LoginResultDto
{
    public bool Succeeded { get; init; }
    public bool RequiresTwoFactor { get; init; }
    public TokenDto? Token { get; init; }
    public string? TwoFactorSessionToken { get; init; }
}
