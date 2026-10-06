using MediatR;
using ThaiX.Application.Features.Files.Commands.DeleteFile;
using ThaiX.Application.Features.Files.Commands.UploadFile;
using ThaiX.Application.Features.Files.Queries.GetFile;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// File upload, metadata, and delete endpoints.
/// </summary>
public static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/files")
            .WithTags("Files");

        group.MapPost("/upload", async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var form = await httpContext.Request.ReadFormAsync(cancellationToken);
                var file = form.Files.GetFile("file");
                var storageKey = form["storageKey"].ToString();
                if (file is null || string.IsNullOrWhiteSpace(storageKey))
                    return Results.BadRequest("File and storageKey are required.");
                await using var stream = file.OpenReadStream();
                var command = new UploadFileCommand
                {
                    FileStream = stream,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    StorageKey = storageKey.Trim()
                };
                var result = await mediator.Send(command, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization(Domain.Common.Constants.Permissions.FileWrite)
            .WithName("UploadFile")
            .WithDescription("Upload a file; storageKey e.g. contacts/{contactId}/avatar/avatar.webp")
            .DisableAntiforgery();

        group.MapGet("/{id:guid}", async (
                Guid id,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetFileQuery { FileId = id };
                var result = await mediator.Send(query, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .RequireAuthorization(Domain.Common.Constants.Permissions.FileRead)
            .WithName("GetFile")
            .WithDescription("Get file metadata and public URL by ID");

        group.MapDelete("/{id:guid}", async (
                Guid id,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteFileCommand { FileId = id }, cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(Domain.Common.Constants.Permissions.FileDelete)
            .WithName("DeleteFile")
            .WithDescription("Delete file from storage and remove metadata");
    }
}
