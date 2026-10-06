using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Emails;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.Identity;
using ThaiX.Presentation.Localization;
using ThaiX.Presentation.Models;
using static ThaiX.Presentation.Resources.ResourceKeys;
using IAuthenticationService = ThaiX.Application.Common.Interfaces.IAuthenticationService;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Account management endpoints: email confirmation, password reset, 2FA, external login, personal data.
/// </summary>
public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/account")
            .WithTags("Account");

        // Email Confirmation
        group.MapPost("/confirm-email", ConfirmEmailAsync)
            .AllowAnonymous()
            .WithName("ConfirmEmail")
            .WithSummary("Confirm email address with token");

        group.MapPost("/resend-confirmation", ResendConfirmationEmailAsync)
            .AllowAnonymous()
            .WithName("ResendConfirmation")
            .WithSummary("Resend email confirmation");

        // Password Reset
        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .AllowAnonymous()
            .WithName("ForgotPassword")
            .WithSummary("Request password reset email");

        group.MapPost("/reset-password", ResetPasswordAsync)
            .AllowAnonymous()
            .WithName("ResetPassword")
            .WithSummary("Reset password with token");

        // Two-Factor Authentication
        group.MapPost("/login-2fa", LoginWith2faAsync)
            .AllowAnonymous()
            .WithName("LoginWith2fa")
            .WithSummary("Complete login with 2FA code");

        group.MapPost("/login-recovery", LoginWithRecoveryCodeAsync)
            .AllowAnonymous()
            .WithName("LoginWithRecoveryCode")
            .WithSummary("Complete login with recovery code");

        group.MapGet("/2fa-status", GetTwoFactorStatusAsync)
            .RequireAuthorization()
            .WithName("Get2faStatus")
            .WithSummary("Get 2FA status for current user");

        group.MapPost("/enable-authenticator", EnableAuthenticatorAsync)
            .RequireAuthorization()
            .WithName("EnableAuthenticator")
            .WithSummary("Enable authenticator app 2FA");

        group.MapPost("/verify-authenticator", VerifyAuthenticatorAsync)
            .RequireAuthorization()
            .WithName("VerifyAuthenticator")
            .WithSummary("Verify authenticator code and enable 2FA");

        group.MapPost("/disable-2fa", DisableTwoFactorAsync)
            .RequireAuthorization()
            .WithName("Disable2fa")
            .WithSummary("Disable two-factor authentication");

        group.MapPost("/reset-authenticator", ResetAuthenticatorAsync)
            .RequireAuthorization()
            .WithName("ResetAuthenticator")
            .WithSummary("Reset authenticator key");

        group.MapPost("/generate-recovery-codes", GenerateRecoveryCodesAsync)
            .RequireAuthorization()
            .WithName("GenerateRecoveryCodes")
            .WithSummary("Generate new recovery codes");

        // External Login
        group.MapGet("/external-login", ExternalLoginChallengeAsync)
            .AllowAnonymous()
            .WithName("ExternalLoginChallenge")
            .WithSummary("Initiate external login (Google)");

        group.MapGet("/external-login-callback", ExternalLoginCallbackAsync)
            .AllowAnonymous()
            .WithName("ExternalLoginCallback")
            .WithSummary("Handle external login callback");

        group.MapGet("/external-logins", GetExternalLoginsAsync)
            .RequireAuthorization()
            .WithName("GetExternalLogins")
            .WithSummary("Get linked external logins");

        group.MapDelete("/external-login", RemoveExternalLoginAsync)
            .RequireAuthorization()
            .WithName("RemoveExternalLogin")
            .WithSummary("Remove an external login");

        // Personal Data (GDPR)
        group.MapGet("/personal-data", DownloadPersonalDataAsync)
            .RequireAuthorization()
            .WithName("DownloadPersonalData")
            .WithSummary("Download personal data (GDPR)");

        group.MapDelete("/personal-data", DeleteAccountAsync)
            .RequireAuthorization()
            .WithName("DeleteAccount")
            .WithSummary("Delete account and personal data (GDPR)");
    }

    // ==================== Email Confirmation ====================

    private static async Task<IResult> ConfirmEmailAsync(
        [FromBody] ConfirmEmailRequest request,
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var success = await identityUserService.ConfirmEmailAsync(
            request.UserId, request.Token, cancellationToken);

        if (!success)
        {
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.EMAIL_CONFIRMATION_FAILED,
                    localizer[Error.EmailConfirmationFailed]),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.EmailConfirmed]));
    }

    private static async Task<IResult> ResendConfirmationEmailAsync(
        [FromBody] ResendConfirmationRequest request,
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IEmailSender emailSender,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var userId = await identityUserService.FindUserIdByEmailAsync(request.Email, cancellationToken);
        if (userId == null)
        {
            // Don't reveal whether user exists
            return Results.Ok(ApiResponse.SuccessResult(localizer[Success.ConfirmationEmailSent]));
        }

        var token = await identityUserService.GenerateEmailConfirmationTokenAsync(userId.Value, cancellationToken);
        var clientBaseUrl = configuration["ClientBaseUrl"] ?? "https://localhost:5202";
        var encodedToken = Uri.EscapeDataString(token);
        var confirmUrl = $"{clientBaseUrl}/auth/confirm-email?userId={userId.Value}&token={encodedToken}";

        await emailSender.SendEmailAsync(
            request.Email,
            "Confirm your email - ThaiX",
            EmailTemplateBuilder.Build(
                "Confirm your email",
                "<p>Please confirm your email address by clicking the button below.</p>",
                new EmailTemplateBuilder.CallToAction("Confirm Email", confirmUrl)),
            cancellationToken);

        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.ConfirmationEmailSent]));
    }

    // ==================== Password Reset ====================

    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IEmailSender emailSender,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var userId = await identityUserService.FindUserIdByEmailAsync(request.Email, cancellationToken);
        if (userId == null)
        {
            // Don't reveal whether user exists
            return Results.Ok(ApiResponse.SuccessResult(localizer[Success.PasswordResetEmailSent]));
        }

        var token = await identityUserService.GeneratePasswordResetTokenAsync(userId.Value, cancellationToken);
        var clientBaseUrl = configuration["ClientBaseUrl"] ?? "https://localhost:5202";
        var encodedToken = Uri.EscapeDataString(token);
        var resetUrl = $"{clientBaseUrl}/auth/reset-password?userId={userId.Value}&token={encodedToken}";

        await emailSender.SendEmailAsync(
            request.Email,
            "Reset your password - ThaiX",
            EmailTemplateBuilder.Build(
                "Reset your password",
                "<p>Click the button below to choose a new password.</p>",
                new EmailTemplateBuilder.CallToAction("Reset Password", resetUrl),
                "If you did not request this, you can safely ignore this email."),
            cancellationToken);

        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.PasswordResetEmailSent]));
    }

    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] AccountResetPasswordRequest request,
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.PASSWORD_MISMATCH, localizer[Error.PasswordMismatch]),
                statusCode: StatusCodes.Status400BadRequest);
        }

        var success = await identityUserService.ResetPasswordWithTokenAsync(
            request.UserId, request.Token, request.Password, cancellationToken);

        if (!success)
        {
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.PASSWORD_RESET_FAILED,
                    localizer[Error.PasswordResetFailed]),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.PasswordResetCompleted]));
    }

    // ==================== Two-Factor Authentication ====================

    private static async Task<IResult> LoginWith2faAsync(
        [FromBody] LoginWith2faRequest request,
        [FromServices] IAuthenticationService authService,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var tokenResult = await authService.AuthenticateWithTwoFactorAsync(
            request.TwoFactorSessionToken,
            request.TwoFactorCode,
            cancellationToken);

        var response = new LoginResponse
        {
            Token = tokenResult.AccessToken,
            TokenType = tokenResult.TokenType,
            ExpiresIn = tokenResult.ExpiresIn
        };

        return Results.Ok(ApiResponse<LoginResponse>.SuccessResult(
            response,
            localizer[Success.LoginSuccessful]));
    }

    private static async Task<IResult> LoginWithRecoveryCodeAsync(
        [FromBody] LoginWithRecoveryCodeRequest request,
        [FromServices] IAuthenticationService authService,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var tokenResult = await authService.AuthenticateWithRecoveryCodeAsync(
            request.TwoFactorSessionToken,
            request.RecoveryCode,
            cancellationToken);

        var response = new LoginResponse
        {
            Token = tokenResult.AccessToken,
            TokenType = tokenResult.TokenType,
            ExpiresIn = tokenResult.ExpiresIn
        };

        return Results.Ok(ApiResponse<LoginResponse>.SuccessResult(
            response,
            localizer[Success.LoginSuccessful]));
    }

    private static async Task<IResult> GetTwoFactorStatusAsync(
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        var is2faEnabled = await identityUserService.GetTwoFactorEnabledAsync(userId.Value, cancellationToken);
        var recoveryCodesLeft = await identityUserService.CountRecoveryCodesAsync(userId.Value, cancellationToken);
        var hasAuthenticator = !string.IsNullOrEmpty(
            await identityUserService.GetAuthenticatorKeyAsync(userId.Value, cancellationToken));

        return Results.Ok(ApiResponse<object>.SuccessResult(new
        {
            Is2faEnabled = is2faEnabled,
            RecoveryCodesLeft = recoveryCodesLeft,
            HasAuthenticator = hasAuthenticator
        }));
    }

    private static async Task<IResult> EnableAuthenticatorAsync(
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        var key = await identityUserService.GetAuthenticatorKeyAsync(userId.Value, cancellationToken);
        var email = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                    ?? httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
                    ?? "user";

        return Results.Ok(ApiResponse<object>.SuccessResult(new
        {
            SharedKey = key,
            AuthenticatorUri = $"otpauth://totp/ThaiX:{email}?secret={key}&issuer=ThaiX&digits=6"
        }));
    }

    private static async Task<IResult> VerifyAuthenticatorAsync(
        [FromBody] VerifyAuthenticatorRequest request,
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        var code = request.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isValid = await identityUserService.VerifyTwoFactorTokenAsync(userId.Value, code, cancellationToken);

        if (!isValid)
        {
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.TWO_FACTOR_INVALID,
                    localizer[Error.TwoFactorCodeInvalid]),
                statusCode: StatusCodes.Status400BadRequest);
        }

        await identityUserService.SetTwoFactorEnabledAsync(userId.Value, true, cancellationToken);
        var recoveryCodes = await identityUserService.GenerateNewTwoFactorRecoveryCodesAsync(
            userId.Value, 10, cancellationToken);

        return Results.Ok(ApiResponse<object>.SuccessResult(new
        {
            RecoveryCodes = recoveryCodes.ToArray()
        }));
    }

    private static async Task<IResult> DisableTwoFactorAsync(
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        await identityUserService.SetTwoFactorEnabledAsync(userId.Value, false, cancellationToken);
        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.TwoFactorDisabled]));
    }

    private static async Task<IResult> ResetAuthenticatorAsync(
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        await identityUserService.ResetAuthenticatorKeyAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.AuthenticatorReset]));
    }

    private static async Task<IResult> GenerateRecoveryCodesAsync(
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        var codes = await identityUserService.GenerateNewTwoFactorRecoveryCodesAsync(
            userId.Value, 10, cancellationToken);

        return Results.Ok(ApiResponse<object>.SuccessResult(new
        {
            RecoveryCodes = codes.ToArray()
        }));
    }

    // ==================== External Login ====================

    private static Task<IResult> ExternalLoginChallengeAsync(
        [FromQuery] string provider,
        [FromQuery] string returnUrl,
        HttpContext httpContext)
    {
        var callbackUrl = $"/api/account/external-login-callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items = { { "LoginProvider", provider } }
        };

        return Task.FromResult(Results.Challenge(properties, [provider]));
    }

    private static async Task<IResult> ExternalLoginCallbackAsync(
        [FromQuery] string? returnUrl,
        [FromServices] IAuthenticationService authService,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authenticateResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
        {
            var clientBaseUrl = configuration["ClientBaseUrl"] ?? "https://localhost:5202";
            return Results.Redirect($"{clientBaseUrl}/auth/login?error=external_login_failed");
        }

        var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
        var providerKey = authenticateResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var provider = authenticateResult.Properties?.Items["LoginProvider"] ?? "Google";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
        {
            var clientBaseUrl = configuration["ClientBaseUrl"] ?? "https://localhost:5202";
            return Results.Redirect($"{clientBaseUrl}/auth/login?error=missing_claims");
        }

        var result = await authService.AuthenticateExternalUserAsync(
            provider, providerKey, email, cancellationToken);

        if (!result.Succeeded || result.Token == null)
        {
            var clientBaseUrl = configuration["ClientBaseUrl"] ?? "https://localhost:5202";
            return Results.Redirect($"{clientBaseUrl}/auth/login?error=auth_failed");
        }

        // Redirect back to client with token
        var baseUrl = returnUrl ?? configuration["ClientBaseUrl"] ?? "https://localhost:5202";
        var redirectUrl = $"{baseUrl}/auth/external-login-callback?token={result.Token.AccessToken}" +
                          $"&tokenType={result.Token.TokenType}&expiresIn={result.Token.ExpiresIn}";

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect(redirectUrl);
    }

    private static async Task<IResult> GetExternalLoginsAsync(
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        var logins = await identityUserService.GetExternalLoginsAsync(userId.Value, cancellationToken);
        var hasPassword = await identityUserService.HasPasswordAsync(userId.Value, cancellationToken);

        return Results.Ok(ApiResponse<object>.SuccessResult(new
        {
            ExternalLogins = logins,
            HasPassword = hasPassword
        }));
    }

    private static async Task<IResult> RemoveExternalLoginAsync(
        [FromBody] RemoveExternalLoginRequest request,
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        await identityUserService.RemoveExternalLoginAsync(
            userId.Value, request.LoginProvider, request.ProviderKey, cancellationToken);

        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.ExternalLoginRemoved]));
    }

    // ==================== Personal Data (GDPR) ====================

    private static async Task<IResult> DownloadPersonalDataAsync(
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        var data = await identityUserService.GetPersonalDataAsync(userId.Value, cancellationToken);

        return Results.Ok(ApiResponse<Dictionary<string, string>>.SuccessResult(data));
    }

    private static async Task<IResult> DeleteAccountAsync(
        [FromBody] DeleteAccountRequest request,
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContextAccessor);
        if (userId == null)
            return Results.Json(ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, "Not authenticated"),
                statusCode: StatusCodes.Status401Unauthorized);

        // Verify password before deletion
        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.RESOURCE_NOT_FOUND, localizer[Error.UserNotFound]),
                statusCode: StatusCodes.Status404NotFound);
        }

        var hasPassword = await userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            var passwordCheck = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordCheck)
            {
                return Results.Json(
                    ApiResponse.ErrorResult(ErrorCodes.INVALID_CREDENTIALS, localizer[Error.InvalidCredentials]),
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        await identityUserService.DeleteUserAsync(userId.Value, cancellationToken);

        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.AccountDeleted]));
    }

    // ==================== Helpers ====================

    private static Guid? GetCurrentUserId(IHttpContextAccessor httpContextAccessor)
    {
        var userIdStr = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdStr, out var userId) ? userId : null;
    }
}

// ==================== Request/Response Models ====================

public record ConfirmEmailRequest
{
    public required Guid UserId { get; init; }
    public required string Token { get; init; }
}

public record ResendConfirmationRequest
{
    public required string Email { get; init; }
}

public record ForgotPasswordRequest
{
    public required string Email { get; init; }
}

public record AccountResetPasswordRequest
{
    public required Guid UserId { get; init; }
    public required string Token { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
}

public record LoginWith2faRequest
{
    public required string TwoFactorSessionToken { get; init; }
    public required string TwoFactorCode { get; init; }
}

public record LoginWithRecoveryCodeRequest
{
    public required string TwoFactorSessionToken { get; init; }
    public required string RecoveryCode { get; init; }
}

public record VerifyAuthenticatorRequest
{
    public required string Code { get; init; }
}

public record RemoveExternalLoginRequest
{
    public required string LoginProvider { get; init; }
    public required string ProviderKey { get; init; }
}

public record DeleteAccountRequest
{
    public required string Password { get; init; }
}
