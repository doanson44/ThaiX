using ThaiX.Client.Models.Auth;

namespace ThaiX.Client.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginWith2faAsync(LoginWith2faRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginWithRecoveryCodeAsync(LoginWithRecoveryCodeRequest request, CancellationToken cancellationToken = default);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<UserInfoResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

    // Email Confirmation
    Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
    Task ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default);

    // Password Reset
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(AccountResetPasswordRequest request, CancellationToken cancellationToken = default);

    // Two-Factor Authentication
    Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(CancellationToken cancellationToken = default);
    Task<EnableAuthenticatorResponse> EnableAuthenticatorAsync(CancellationToken cancellationToken = default);
    Task<VerifyAuthenticatorResponse> VerifyAuthenticatorAsync(VerifyAuthenticatorRequest request, CancellationToken cancellationToken = default);
    Task DisableTwoFactorAsync(CancellationToken cancellationToken = default);
    Task ResetAuthenticatorAsync(CancellationToken cancellationToken = default);
    Task<GenerateRecoveryCodesResponse> GenerateRecoveryCodesAsync(CancellationToken cancellationToken = default);

    // External Logins
    Task<ExternalLoginsResponse> GetExternalLoginsAsync(CancellationToken cancellationToken = default);
    Task RemoveExternalLoginAsync(RemoveExternalLoginRequest request, CancellationToken cancellationToken = default);

    // Personal Data (GDPR)
    Task<Dictionary<string, string>> DownloadPersonalDataAsync(CancellationToken cancellationToken = default);
    Task DeleteAccountAsync(DeleteAccountRequest request, CancellationToken cancellationToken = default);
}
