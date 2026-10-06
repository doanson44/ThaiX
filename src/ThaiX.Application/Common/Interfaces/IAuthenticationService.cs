namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Service for authenticating users and issuing JWT tokens.
/// Implementation lives in Infrastructure layer.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with username/email and password.
    /// Returns authentication result (may require 2FA).
    /// </summary>
    /// <param name="rememberMe">When true, token has extended lifetime (e.g. 30 days).</param>
    Task<AuthenticationResult> AuthenticateUserAsync(
        string usernameOrEmail,
        string password,
        bool rememberMe = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with a two-factor code after initial login.
    /// </summary>
    Task<TokenResult> AuthenticateWithTwoFactorAsync(
        string twoFactorSessionToken,
        string twoFactorCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with a recovery code after initial login.
    /// </summary>
    Task<TokenResult> AuthenticateWithRecoveryCodeAsync(
        string twoFactorSessionToken,
        string recoveryCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates an API client with client credentials.
    /// Returns JWT token on success.
    /// </summary>
    Task<TokenResult> AuthenticateClientAsync(
        string clientId,
        string clientSecret,
        string? requestedScope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes external login callback and returns token or creates user.
    /// </summary>
    Task<AuthenticationResult> AuthenticateExternalUserAsync(
        string provider,
        string providerKey,
        string email,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of authentication operation.
/// </summary>
public sealed record TokenResult
{
    public required string AccessToken { get; init; }
    public required string TokenType { get; init; }
    public required int ExpiresIn { get; init; }
    public required string Scope { get; init; }
}

/// <summary>
/// Result of authentication that may require additional steps (e.g., 2FA).
/// </summary>
public sealed record AuthenticationResult
{
    public bool Succeeded { get; init; }
    public bool RequiresTwoFactor { get; init; }
    public TokenResult? Token { get; init; }

    /// <summary>
    /// Short-lived session token for 2FA verification.
    /// Only populated when RequiresTwoFactor is true.
    /// </summary>
    public string? TwoFactorSessionToken { get; init; }
}
