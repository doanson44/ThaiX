using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Features.Notifications.Commands.SendNotification;
using ThaiX.Application.Features.Notifications.Commands.UpsertUserNotificationPreference;
using ThaiX.Application.Features.Notifications.Queries.GetNotificationById;
using ThaiX.Application.Features.Notifications.Queries.GetUserNotificationPreferences;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications")
            .WithTags("Notifications");

        group.MapPost("/send", async (
            [FromBody] SendNotificationCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var notificationId = await mediator.Send(command, cancellationToken);

            var response = ApiResponse<Guid>.SuccessResult(notificationId);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.SystemAdmin)
        .WithName("SendNotification")
        .WithDescription("Send a notification via configured routing (Slack, Telegram, or Both).");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var notification = await mediator.Send(new GetNotificationByIdQuery(id), cancellationToken);
            if (notification is null)
            {
                var notFound = ApiResponse<NotificationDto>.ErrorResult(
                    ErrorCodes.RESOURCE_NOT_FOUND,
                    "Notification was not found.");
                notFound.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.NotFound(notFound);
            }

            var response = ApiResponse<NotificationDto>.SuccessResult(notification);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.SystemAdmin)
        .WithName("GetNotificationById")
        .WithDescription("Get notification lifecycle and delivery status by id.");

        group.MapGet("/preferences/{userId:guid}", async (
            Guid userId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var preferences = await mediator.Send(
                new GetUserNotificationPreferencesQuery(userId),
                cancellationToken);

            var response = ApiResponse<IReadOnlyList<UserNotificationPreferenceDto>>.SuccessResult(preferences);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.SystemAdmin)
        .WithName("GetUserNotificationPreferences")
        .WithDescription("Get notification preferences for a user.");

        group.MapPut("/preferences", async (
            [FromBody] UpsertUserNotificationPreferenceCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var preferenceId = await mediator.Send(command, cancellationToken);

            var response = ApiResponse<Guid>.SuccessResult(preferenceId);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.SystemAdmin)
        .WithName("UpsertUserNotificationPreference")
        .WithDescription("Create or update a user notification preference.");
    }
}
