using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Features.Notifications.Scheduling.Commands.CreateNotificationSchedule;
using ThaiX.Application.Features.Notifications.Scheduling.Commands.DeleteNotificationSchedule;
using ThaiX.Application.Features.Notifications.Scheduling.Commands.RunScheduleNow;
using ThaiX.Application.Features.Notifications.Scheduling.Commands.SetScheduleStatus;
using ThaiX.Application.Features.Notifications.Scheduling.Commands.UpdateNotificationSchedule;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;
using ThaiX.Application.Features.Notifications.Scheduling.Queries.GetNotificationSchedule;
using ThaiX.Application.Features.Notifications.Scheduling.Queries.GetNotificationSchedules;
using ThaiX.Application.Features.Notifications.Scheduling.Queries.GetScheduleExecutions;
using ThaiX.Application.Features.Notifications.Scheduling.Queries.GetUpcomingSchedules;
using ThaiX.Application.Features.Notifications.Scheduling.Queries.PreviewScheduleOccurrences;
using ThaiX.Domain.Aggregates.Notifications.Enums;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class NotificationScheduleEndpoints
{
    private const string BasePath = "/api/notification-schedules";

    public static void MapNotificationScheduleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(BasePath).RequireAuthorization();

        group.MapGet("/", async (
            [AsParameters] NotificationScheduleQueryParameters parameters,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetNotificationSchedulesQuery
            {
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20,
                Search = parameters.Search,
                Status = parameters.Status
            };
            var result = await mediator.Send(query, ct);
            return Results.Ok(result.ToPagedApiResponse());
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetNotificationScheduleQuery { Id = id }, ct);
            if (result is null) return Results.NotFound();
            return Results.Ok(ApiResponse<NotificationScheduleDto>.SuccessResult(result));
        });

        group.MapPost("/", async (
            [FromBody] CreateNotificationScheduleCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Created($"{BasePath}/{result.Id}",
                ApiResponse<NotificationScheduleDto>.SuccessResult(result));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateNotificationScheduleCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            if (id != command.Id)
                return Results.BadRequest(ApiResponse.ErrorResult("ID_MISMATCH", "Route ID does not match body ID."));

            await mediator.Send(command, ct);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new DeleteNotificationScheduleCommand { Id = id }, ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/activate", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new SetScheduleStatusCommand { Id = id, Status = ScheduleStatus.Active }, ct);
            return Results.Ok();
        });

        group.MapPost("/{id:guid}/pause", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new SetScheduleStatusCommand { Id = id, Status = ScheduleStatus.Paused }, ct);
            return Results.Ok();
        });

        group.MapPost("/{id:guid}/resume", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new SetScheduleStatusCommand { Id = id, Status = ScheduleStatus.Active }, ct);
            return Results.Ok();
        });

        group.MapPost("/{id:guid}/disable", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new SetScheduleStatusCommand { Id = id, Status = ScheduleStatus.Disabled }, ct);
            return Results.Ok();
        });

        group.MapPost("/{id:guid}/run-now", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RunScheduleNowCommand { Id = id }, ct);
            return Results.Ok(ApiResponse<ScheduleExecutionDto>.SuccessResult(result));
        });

        group.MapGet("/{id:guid}/executions", async (
            Guid id,
            [AsParameters] NotificationScheduleQueryParameters parameters,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetScheduleExecutionsQuery
            {
                ScheduleId = id,
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 50
            };
            var result = await mediator.Send(query, ct);
            return Results.Ok(result.ToPagedApiResponse());
        });

        group.MapGet("/upcoming", async (
            int count,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetUpcomingSchedulesQuery { Count = count == 0 ? 20 : count };
            return Results.Ok(ApiResponse<List<UpcomingScheduleDto>>.SuccessResult(
                await mediator.Send(query, ct)));
        });

        group.MapGet("/{id:guid}/preview", async (
            Guid id,
            int count,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new PreviewScheduleOccurrencesQuery { ScheduleId = id, Count = count == 0 ? 10 : count };
            var result = await mediator.Send(query, ct);
            if (result is null) return Results.NotFound();
            return Results.Ok(ApiResponse<List<DateTime>>.SuccessResult(result));
        });
    }
}

public sealed record NotificationScheduleQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }

    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    [FromQuery(Name = "status")]
    public string? Status { get; init; }
}
