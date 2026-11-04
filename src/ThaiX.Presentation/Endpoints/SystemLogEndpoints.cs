using MediatR;
using ThaiX.Application.Features.SystemLogs.Queries.DownloadSystemLogFile;
using ThaiX.Application.Features.SystemLogs.Queries.GetSystemLogFilePreview;
using ThaiX.Application.Features.SystemLogs.Queries.GetSystemLogFiles;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Presentation.Endpoints;

public static class SystemLogEndpoints
{
    public static void MapSystemLogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/system-logs")
            .WithTags("System Logs")
            .RequireAuthorization(Permissions.SystemViewLogs);

        group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetSystemLogFilesQuery(), cancellationToken)));

        group.MapGet("/{fileName}", async (
            string fileName,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var preview = await sender.Send(
                new GetSystemLogFilePreviewQuery(fileName),
                cancellationToken);

            return preview is null ? Results.NotFound() : Results.Ok(preview);
        });

        group.MapGet("/{fileName}/download", async (
            string fileName,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var download = await sender.Send(
                new DownloadSystemLogFileQuery(fileName),
                cancellationToken);

            return download is null
                ? Results.NotFound()
                : Results.File(
                    download.Content,
                    "text/plain; charset=utf-8",
                    download.FileName,
                    enableRangeProcessing: true);
        });
    }
}
