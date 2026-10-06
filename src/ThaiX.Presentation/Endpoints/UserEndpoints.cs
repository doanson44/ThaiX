using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Security.Claims;
using ThaiX.Application.Common.Emails;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Identity.Commands.LinkUserToContact;
using ThaiX.Application.Features.Identity.Commands.UnlinkContactFromUser;
using ThaiX.Application.Features.Identity.Dtos;
using ThaiX.Application.Features.Identity.Queries.GetCurrentUserProfile;
using ThaiX.Application.Features.Identity.Queries.GetLinkedContactForUser;
using ThaiX.Application.Features.Users.Commands.CreateUser;
using ThaiX.Application.Features.Users.Commands.ResetPassword;
using ThaiX.Application.Features.Users.Commands.SetUserLockout;
using ThaiX.Application.Features.Users.Commands.SetUserPermissions;
using ThaiX.Application.Features.Users.Commands.UpdateUser;
using ThaiX.Application.Features.Users.Queries.GetPermissions;
using ThaiX.Application.Features.Users.Queries.GetUserPermissions;
using ThaiX.Application.Features.Users.Queries.GetUsers;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// User management endpoints.
/// All endpoints require appropriate permissions.
/// </summary>
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var userGroup = endpoints.MapGroup("/api/users")
            .WithTags("Users");

        // GET /api/users/me/profile - Current user profile (FullName, AvatarUrl from linked Contact)
        userGroup.MapGet("me/profile", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                var err = ApiResponse<UserProfileDto?>.ErrorResult("USER_NOT_AUTHENTICATED", "User ID not found.");
                return Results.Json(err, statusCode: 401);
            }
            var profile = await mediator.Send(new GetCurrentUserProfileQuery(userId), cancellationToken);
            var response = ApiResponse<UserProfileDto?>.SuccessResult(profile);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("GetCurrentUserProfile")
        .WithDescription("Get current user profile (full name, avatar) from linked Contact");

        // POST /api/users/{userId}/link-contact - Link user to contact and create UserProfile projection
        userGroup.MapPost("{userId:guid}/link-contact", async (
            Guid userId,
            [FromBody] LinkUserToContactRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new LinkUserToContactCommand { UserId = userId, ContactId = request.ContactId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserRead)
        .WithName("LinkUserToContact")
        .WithDescription("Link an Identity user to a Contact and create UserProfile projection");

        // GET /api/users/{userId}/linked-contact - Get contact linked to user (if any)
        userGroup.MapGet("{userId:guid}/linked-contact", async (
            Guid userId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var linked = await mediator.Send(new GetLinkedContactForUserQuery(userId), cancellationToken);
            var response = ApiResponse<LinkedContactDto?>.SuccessResult(linked);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserRead)
        .WithName("GetLinkedContactForUser")
        .WithDescription("Get the contact linked to this user");

        // DELETE /api/users/{userId}/linked-contact - Unlink contact from user
        userGroup.MapDelete("{userId:guid}/linked-contact", async (
            Guid userId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new UnlinkContactFromUserCommand(userId), cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserWrite)
        .WithName("UnlinkContactFromUser")
        .WithDescription("Unlink the contact from this user");

        // GET /api/users - Paginated list of users
        userGroup.MapGet("/", async (
            [AsParameters] GetUsersQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUsersQuery
            {
                PageNumber = parameters.PageNumber ?? GetUsersQueryParameters.DefaultPageNumber,
                PageSize = parameters.PageSize ?? GetUsersQueryParameters.DefaultPageSize,
                SortBy = string.IsNullOrWhiteSpace(parameters.SortBy)
                    ? GetUsersQueryParameters.DefaultSortBy
                    : parameters.SortBy.Trim(),
                SortDescending = parameters.SortDescending ?? GetUsersQueryParameters.DefaultSortDescending,
                SearchTerm = parameters.SearchTerm,
                IsActive = parameters.IsActive
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(
                httpContext.GetCorrelationId());

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserRead)
        .WithName("GetUsers")
        .WithDescription("Get paginated list of users with optional filtering and sorting");

        // GET /api/users/permissions - List all available permission codes
        userGroup.MapGet("/permissions", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var permissions = await mediator.Send(new GetPermissionsQuery(), cancellationToken);
            var response = ApiResponse<IReadOnlyCollection<string>>.SuccessResult(permissions);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserRead)
        .WithName("GetPermissions")
        .WithDescription("Get all available permission codes for assignment UI");

        // GET /api/users/{userId}/permissions - Get permissions assigned to a user
        userGroup.MapGet("/{userId:guid}/permissions", async (
            Guid userId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var permissions = await mediator.Send(new GetUserPermissionsQuery(userId), cancellationToken);
            var response = ApiResponse<IReadOnlyCollection<string>>.SuccessResult(permissions);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserRead)
        .WithName("GetUserPermissions")
        .WithDescription("Get permissions currently assigned to the user");

        // POST /api/users - Create new user
        userGroup.MapPost("/", async (
            [FromBody] CreateUserCommand command,
            IMediator mediator,
            IIdentityUserService identityUserService,
            IEmailSender emailSender,
            IConfiguration configuration,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = await mediator.Send(command, cancellationToken);
            if (command.SendActivationEmail)
            {
                try
                {
                    var token = await identityUserService.GenerateEmailConfirmationTokenAsync(userId, cancellationToken);
                    var clientBaseUrl = configuration["ClientBaseUrl"] ?? "https://localhost:5202";
                    var encodedToken = Uri.EscapeDataString(token);
                    var confirmUrl = $"{clientBaseUrl}/auth/confirm-email?userId={userId}&token={encodedToken}";
                    await emailSender.SendEmailAsync(
                        command.Email,
                        "Confirm your email - ThaiX",
                        EmailTemplateBuilder.Build(
                            "Welcome to ThaiX!",
                            "<p>Your account has been created. Please confirm your email to get started.</p>",
                            new EmailTemplateBuilder.CallToAction("Confirm Email", confirmUrl),
                            "If you did not expect this email, please contact your administrator."),
                        cancellationToken);
                }
                catch (Exception)
                {
                    // Don't fail create if email sending fails; user can resend from account
                }
            }
            var response = ApiResponse<Guid>.SuccessResult(userId);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/users/{userId}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserWrite)
        .WithName("CreateUser")
        .WithDescription("Create a new user with optional initial permissions");

        // PUT /api/users/{userId} - Update user profile
        userGroup.MapPut("/{userId:guid}", async (
            Guid userId,
            [FromBody] UpdateUserRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateUserCommand
            {
                UserId = userId,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = request.EmailConfirmed,
                TwoFactorEnabled = request.TwoFactorEnabled
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserWrite)
        .WithName("UpdateUser")
        .WithDescription("Update user profile information");

        // PUT /api/users/{userId}/lockout - Set user lockout status
        userGroup.MapPut("/{userId:guid}/lockout", async (
            Guid userId,
            [FromBody] SetUserLockoutRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new SetUserLockoutCommand
            {
                UserId = userId,
                IsLocked = request.IsLocked,
                Reason = request.Reason,
                LockoutDurationMinutes = request.LockoutDurationMinutes
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserManageLockout)
        .WithName("SetUserLockout")
        .WithDescription("Block or unblock user login");

        // PUT /api/users/{userId}/permissions - Set user permissions
        userGroup.MapPut("/{userId:guid}/permissions", async (
            Guid userId,
            [FromBody] SetUserPermissionsRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new SetUserPermissionsCommand
            {
                UserId = userId,
                Permissions = request.Permissions
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserManagePermissions)
        .WithName("SetUserPermissions")
        .WithDescription("Set complete permissions list for user (replaces existing permissions)");

        // POST /api/users/{userId}/reset-password - Reset user password
        userGroup.MapPost("/{userId:guid}/reset-password", async (
            Guid userId,
            [FromBody] ResetPasswordRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new ResetPasswordCommand
            {
                UserId = userId,
                NewPassword = request.NewPassword,
                RequirePasswordChange = request.RequirePasswordChange
            };

            var resetPasswordResult = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<ResetPasswordResult>.SuccessResult(resetPasswordResult);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.UserResetPassword)
        .WithName("ResetUserPassword")
        .WithDescription("Reset user password (admin operation)");
    }
}

/// <summary>
/// Query parameters for GetUsers endpoint.
/// Uses [AsParameters] for automatic binding.
/// Pagination and sorting parameters are optional and use default values when omitted.
/// </summary>
public sealed record GetUsersQueryParameters
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
    public const string DefaultSortBy = "email";
    public const bool DefaultSortDescending = false;

    [FromQuery(Name = "pageNumber")]
    [DefaultValue(DefaultPageNumber)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(DefaultPageSize)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "sortBy")]
    [DefaultValue(DefaultSortBy)]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    [DefaultValue(DefaultSortDescending)]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "search")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "isActive")]
    public bool? IsActive { get; init; }
}

/// <summary>
/// Request body for UpdateUser endpoint.
/// </summary>
public sealed record UpdateUserRequest
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool? EmailConfirmed { get; init; }
    public bool? TwoFactorEnabled { get; init; }
}

/// <summary>
/// Request body for SetUserLockout endpoint.
/// </summary>
public sealed record SetUserLockoutRequest
{
    public required bool IsLocked { get; init; }
    public string? Reason { get; init; }
    public int? LockoutDurationMinutes { get; init; }
}

/// <summary>
/// Request body for SetUserPermissions endpoint.
/// </summary>
public sealed record SetUserPermissionsRequest
{
    public required IReadOnlyCollection<string> Permissions { get; init; }
}

/// <summary>
/// Request body for ResetPassword endpoint.
/// </summary>
public sealed record ResetPasswordRequest
{
    public string? NewPassword { get; init; }
    public bool RequirePasswordChange { get; init; } = true;
}

/// <summary>
/// Request body for LinkUserToContact endpoint.
/// </summary>
public sealed record LinkUserToContactRequest
{
    public required Guid ContactId { get; init; }
}
