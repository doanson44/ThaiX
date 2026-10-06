using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Notes.Commands.CreateNote;
using ThaiX.Application.Features.Notes.Commands.DeleteNote;
using ThaiX.Application.Features.Notes.Commands.ToggleArchiveNote;
using ThaiX.Application.Features.Notes.Commands.TogglePinNote;
using ThaiX.Application.Features.Notes.Commands.UpdateNote;
using ThaiX.Application.Features.Notes.Queries.GetNoteById;
using ThaiX.Application.Features.Notes.Queries.GetNotes;
using ThaiX.Domain.Aggregates.Notes;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class NoteEndpoints
{
    public static void MapNoteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notes")
            .WithTags("Notes");

        group.MapGet("/", async (
            [AsParameters] NoteQueryParameters parameters,
            IMediator mediator,
            ICurrentUserService currentUser,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetNotesQuery
            {
                OwnerId = currentUser.UserId,
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 50,
                SearchTerm = parameters.SearchTerm,
                IsPinned = parameters.IsPinned,
                IsArchived = parameters.IsArchived,
                IsDeleted = parameters.IsDeleted,
                SortBy = parameters.SortBy,
                SortDescending = parameters.SortDescending ?? false
            }, ct);
            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.NoteRead);

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetNoteByIdQuery { Id = id }, ct);
            if (result is null) return Results.NotFound();
            var response = ApiResponse<NoteDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.NoteRead);

        group.MapPost("/", async (
            CreateNoteCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/notes/{id}", response);
        })
        .RequireAuthorization(Permissions.NoteWrite);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateNoteRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new UpdateNoteCommand
            {
                Id = id,
                Title = request.Title,
                Content = request.Content,
                Color = request.Color
            }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.NoteWrite);

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new DeleteNoteCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.NoteDelete);

        group.MapPost("/{id:guid}/pin", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new TogglePinNoteCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.NoteWrite);

        group.MapPost("/{id:guid}/archive", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new ToggleArchiveNoteCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.NoteWrite);
    }
}

public sealed record UpdateNoteRequest(string Title, string Content, NoteColor Color);

public sealed record NoteQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    [DefaultValue(1)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(50)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "isPinned")]
    public bool? IsPinned { get; init; }

    [FromQuery(Name = "isArchived")]
    public bool? IsArchived { get; init; }

    [FromQuery(Name = "isDeleted")]
    public bool? IsDeleted { get; init; }
}
