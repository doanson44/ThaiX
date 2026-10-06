using System.Net;
using System.Net.Http.Json;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly JwtAuthenticationStateProvider _authenticationStateProvider;
    private readonly IClientCacheService _cache;

    public AuthService(
        HttpClient httpClient,
        JwtAuthenticationStateProvider authenticationStateProvider,
        IClientCacheService cache)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _cache = cache;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        var loginResponse = await ApiResponseReader.ReadSuccessDataAsync<LoginResponse>(response, cancellationToken);

        if (!loginResponse.RequiresTwoFactor && !string.IsNullOrEmpty(loginResponse.Token))
        {
            await _authenticationStateProvider.MarkUserAsAuthenticatedAsync(loginResponse.Token);
        }

        return loginResponse;
    }

    public async Task<LoginResponse> LoginWith2faAsync(
        LoginWith2faRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/account/login-2fa", request, cancellationToken);
        var loginResponse = await ApiResponseReader.ReadSuccessDataAsync<LoginResponse>(response, cancellationToken);

        if (!string.IsNullOrEmpty(loginResponse.Token))
        {
            await _authenticationStateProvider.MarkUserAsAuthenticatedAsync(loginResponse.Token);
        }

        return loginResponse;
    }

    public async Task<LoginResponse> LoginWithRecoveryCodeAsync(
        LoginWithRecoveryCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/account/login-recovery", request, cancellationToken);
        var loginResponse = await ApiResponseReader.ReadSuccessDataAsync<LoginResponse>(response, cancellationToken);

        if (!string.IsNullOrEmpty(loginResponse.Token))
        {
            await _authenticationStateProvider.MarkUserAsAuthenticatedAsync(loginResponse.Token);
        }

        return loginResponse;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<RegisterResponse>(response, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsync("api/auth/logout", content: null, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
            }
        }
        finally
        {
            await _authenticationStateProvider.MarkUserAsLoggedOutAsync();
            await _cache.ClearAsync(cancellationToken);
        }
    }

    public async Task<UserInfoResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/auth/user", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<UserInfoResponse>(response, cancellationToken);
    }

    public async Task ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    // ==================== Email Confirmation ====================

    public async Task ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/account/confirm-email", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task ResendConfirmationAsync(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/account/resend-confirmation", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    // ==================== Password Reset ====================

    public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/account/forgot-password", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task ResetPasswordAsync(
        AccountResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/account/reset-password", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    // ==================== Two-Factor Authentication ====================

    public async Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/account/2fa-status", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<TwoFactorStatusResponse>(response, cancellationToken);
    }

    public async Task<EnableAuthenticatorResponse> EnableAuthenticatorAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("api/account/enable-authenticator", null, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<EnableAuthenticatorResponse>(response, cancellationToken);
    }

    public async Task<VerifyAuthenticatorResponse> VerifyAuthenticatorAsync(
        VerifyAuthenticatorRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/account/verify-authenticator", request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<VerifyAuthenticatorResponse>(response, cancellationToken);
    }

    public async Task DisableTwoFactorAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("api/account/disable-2fa", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task ResetAuthenticatorAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("api/account/reset-authenticator", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task<GenerateRecoveryCodesResponse> GenerateRecoveryCodesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("api/account/generate-recovery-codes", null, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<GenerateRecoveryCodesResponse>(response, cancellationToken);
    }

    // ==================== External Logins ====================

    public async Task<ExternalLoginsResponse> GetExternalLoginsAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/account/external-logins", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<ExternalLoginsResponse>(response, cancellationToken);
    }

    public async Task RemoveExternalLoginAsync(
        RemoveExternalLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Delete, "api/account/external-login")
        {
            Content = JsonContent.Create(request)
        };
        using var response = await _httpClient.SendAsync(msg, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    // ==================== Personal Data (GDPR) ====================

    public async Task<Dictionary<string, string>> DownloadPersonalDataAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/account/personal-data", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<Dictionary<string, string>>(response, cancellationToken);
    }

    public async Task DeleteAccountAsync(
        DeleteAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Delete, "api/account/personal-data")
        {
            Content = JsonContent.Create(request)
        };
        using var response = await _httpClient.SendAsync(msg, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }
}
