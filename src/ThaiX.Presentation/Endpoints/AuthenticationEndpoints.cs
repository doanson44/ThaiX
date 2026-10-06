using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Emails;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Authentication.Commands.Login;
using ThaiX.Infrastructure.Identity;
using ThaiX.Presentation.Localization;
using ThaiX.Presentation.Models;
using static ThaiX.Presentation.Resources.ResourceKeys;

namespace ThaiX.Presentation.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("User login with username/email and password")
            .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        group.MapPost("/register", RegisterAsync)
            .AllowAnonymous()
            .WithName("Register")
            .WithSummary("Register a new user")
            .Produces<ApiResponse<RegisterResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Logout current user (JWT - client-side token removal)")
            .Produces<ApiResponse>(StatusCodes.Status200OK);

        group.MapGet("/user", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user information")
            .Produces<ApiResponse<UserInfoResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithSummary("Change user password")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] AuthLoginRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password,
            RememberMe = request.RememberMe
        };

        var loginResult = await mediator.Send(command, cancellationToken);

        if (loginResult.RequiresTwoFactor)
        {
            return Results.Ok(ApiResponse<LoginResponse>.SuccessResult(
                new LoginResponse
                {
                    RequiresTwoFactor = true,
                    TwoFactorSessionToken = loginResult.TwoFactorSessionToken
                },
                localizer[Success.TwoFactorRequired]));
        }

        var response = new LoginResponse
        {
            Token = loginResult.Token!.AccessToken,
            TokenType = loginResult.Token.TokenType,
            ExpiresIn = loginResult.Token.ExpiresIn
        };

        return Results.Ok(ApiResponse<LoginResponse>.SuccessResult(
            response,
            localizer[Success.LoginSuccessful]));
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IIdentityUserService identityUserService,
        [FromServices] IEmailSender emailSender,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return Results.Json(
                ApiResponse<object>.ErrorResult(ErrorCodes.PASSWORD_MISMATCH, localizer[Error.PasswordMismatch]),
                statusCode: StatusCodes.Status400BadRequest);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.Json(
                ApiResponse<object>.ErrorResult(ErrorCodes.REGISTRATION_FAILED, errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Send confirmation email
        try
        {
            var token = await identityUserService.GenerateEmailConfirmationTokenAsync(user.Id, cancellationToken);
            var clientBaseUrl = configuration["ClientBaseUrl"] ?? "https://localhost:5202";
            var encodedToken = Uri.EscapeDataString(token);
            var confirmUrl = $"{clientBaseUrl}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            await emailSender.SendEmailAsync(
                user.Email!,
                "Confirm your email - ThaiX",
                EmailTemplateBuilder.Build(
                    "Welcome to ThaiX!",
                    "<p>Please confirm your email address to activate your account.</p>",
                    new EmailTemplateBuilder.CallToAction("Confirm Email", confirmUrl),
                    "If you did not create an account, you can safely ignore this email."),
                cancellationToken);
        }
        catch (Exception)
        {
            // Don't fail registration if email sending fails
        }

        var response = new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            RequiresEmailConfirmation = true
        };

        return Results.Ok(ApiResponse<RegisterResponse>.SuccessResult(
            response,
            localizer[Success.RegistrationSuccessful]));
    }

    private static Task<IResult> LogoutAsync(
        [FromServices] IStringLocalizer<SharedResource> localizer)
    {
        return Task.FromResult(Results.Ok(ApiResponse.SuccessResult(localizer[Success.LogoutSuccessful])));
    }

    private static async Task<IResult> GetCurrentUserAsync(
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Json(
                ApiResponse<object>.ErrorResult(ErrorCodes.UNAUTHORIZED, localizer[Error.UserNotAuthenticated]),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Results.Json(
                ApiResponse<object>.ErrorResult(ErrorCodes.RESOURCE_NOT_FOUND, localizer[Error.UserNotFound]),
                statusCode: StatusCodes.Status404NotFound);
        }

        var permissions = (await userManager.GetClaimsAsync(user))
            .Where(c => c.Type == ClaimTypeConstants.Permission)
            .Select(c => c.Value)
            .ToList();

        var response = new UserInfoResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            Permissions = permissions
        };

        return Results.Ok(ApiResponse<UserInfoResponse>.SuccessResult(response));
    }

    private static async Task<IResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.PASSWORD_MISMATCH, localizer[Error.PasswordMismatch]),
                statusCode: StatusCodes.Status400BadRequest);
        }

        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.UNAUTHORIZED, localizer[Error.UserNotAuthenticated]),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.RESOURCE_NOT_FOUND, localizer[Error.UserNotFound]),
                statusCode: StatusCodes.Status404NotFound);
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.Json(
                ApiResponse.ErrorResult(ErrorCodes.CHANGE_PASSWORD_FAILED, errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(ApiResponse.SuccessResult(localizer[Success.PasswordChanged]));
    }
}

// Request/Response models
public record AuthLoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public bool RememberMe { get; init; }
}

public record LoginResponse
{
    public string? Token { get; init; }
    public string? TokenType { get; init; }
    public int? ExpiresIn { get; init; }
    public bool RequiresTwoFactor { get; init; }
    public string? TwoFactorSessionToken { get; init; }
}

public record RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
}

public record RegisterResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required bool RequiresEmailConfirmation { get; init; }
}

public record UserInfoResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required bool EmailConfirmed { get; init; }
    public required List<string> Permissions { get; init; }
}

public record ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
    public required string ConfirmPassword { get; init; }
}
