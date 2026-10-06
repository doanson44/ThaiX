namespace ThaiX.Client.Models.Auth;

public sealed record LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public bool RememberMe { get; init; }
}

public sealed record LoginResponse
{
    public string? Token { get; init; }
    public string? TokenType { get; init; }
    public int? ExpiresIn { get; init; }
    public bool RequiresTwoFactor { get; init; }
    public string? TwoFactorSessionToken { get; init; }
}

public sealed record RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
}

public sealed record RegisterResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required bool RequiresEmailConfirmation { get; init; }
}

public sealed record UserInfoResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required bool EmailConfirmed { get; init; }
    public required List<string> Permissions { get; init; }
}

public sealed record ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
    public required string ConfirmPassword { get; init; }
}

// ==================== Account Management Contracts ====================

public sealed record LoginWith2faRequest
{
    public required string TwoFactorSessionToken { get; init; }
    public required string TwoFactorCode { get; init; }
}

public sealed record LoginWithRecoveryCodeRequest
{
    public required string TwoFactorSessionToken { get; init; }
    public required string RecoveryCode { get; init; }
}

public sealed record ConfirmEmailRequest
{
    public required Guid UserId { get; init; }
    public required string Token { get; init; }
}

public sealed record ResendConfirmationRequest
{
    public required string Email { get; init; }
}

public sealed record ForgotPasswordRequest
{
    public required string Email { get; init; }
}

public sealed record AccountResetPasswordRequest
{
    public required Guid UserId { get; init; }
    public required string Token { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
}

public sealed record VerifyAuthenticatorRequest
{
    public required string Code { get; init; }
}

public sealed record RemoveExternalLoginRequest
{
    public required string LoginProvider { get; init; }
    public required string ProviderKey { get; init; }
}

public sealed record DeleteAccountRequest
{
    public required string Password { get; init; }
}

public sealed record TwoFactorStatusResponse
{
    public bool Is2faEnabled { get; init; }
    public int RecoveryCodesLeft { get; init; }
    public bool HasAuthenticator { get; init; }
}

public sealed record EnableAuthenticatorResponse
{
    public required string SharedKey { get; init; }
    public required string AuthenticatorUri { get; init; }
}

public sealed record VerifyAuthenticatorResponse
{
    public required string[] RecoveryCodes { get; init; }
}

public sealed record GenerateRecoveryCodesResponse
{
    public required string[] RecoveryCodes { get; init; }
}

public sealed record ExternalLoginsResponse
{
    public required List<ExternalLoginDto> ExternalLogins { get; init; }
    public bool HasPassword { get; init; }
}

public sealed record ExternalLoginDto
{
    public required string LoginProvider { get; init; }
    public required string ProviderKey { get; init; }
    public string? ProviderDisplayName { get; init; }
}
