using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Features.PublicHtml.Commands.DeletePublicHtmlPage;
using ThaiX.Application.Features.PublicHtml.Commands.RenamePublicHtmlPage;
using ThaiX.Application.Features.PublicHtml.Commands.RecordPublicHtmlPageView;
using ThaiX.Application.Features.PublicHtml.Commands.SetPublicHtmlPagePublished;
using ThaiX.Application.Features.PublicHtml.Commands.UploadPublicHtmlPage;
using ThaiX.Application.Features.PublicHtml.Queries.GetPublicHtmlPages;
using ThaiX.Application.Features.PublicHtml.Queries.GetPublishedPublicHtmlPage;
using ThaiX.Application.Features.PublicHtml.Queries.OpenPublishedPublicHtmlContent;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Presentation.Endpoints;

public static class PublicHtmlEndpoints
{
    public static void MapPublicHtmlEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/public-html").WithTags("Public HTML Pages");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPublicHtmlPagesQuery(), ct)))
            .RequireAuthorization(Permissions.FileRead);

        group.MapPost("/", async (
            [FromForm] IFormFile file,
            [FromForm] string slug,
            [FromForm] string? title,
            ISender sender,
            CancellationToken ct) =>
        {
            if (file.Length <= 0 || file.Length > 5 * 1024 * 1024)
                return Results.BadRequest(new { code = "PUBLIC_HTML_INVALID_SIZE", message = "HTML file must be between 1 byte and 5 MB." });

            try
            {
                await using var stream = file.OpenReadStream();
                var id = await sender.Send(new UploadPublicHtmlPageCommand(
                    title ?? string.Empty,
                    slug,
                    file.FileName,
                    stream,
                    file.Length), ct);
                return Results.Created($"/api/public-html/{id}", new { id });
            }
            catch (ArgumentException ex) { return Results.BadRequest(new { code = "PUBLIC_HTML_INVALID_INPUT", message = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { code = "PUBLIC_HTML_SLUG_EXISTS", message = ex.Message }); }
        }).RequireAuthorization(Permissions.FileWrite).DisableAntiforgery();

        group.MapPatch("/{id:guid}/slug", async (Guid id, [FromBody] RenameRequest request, ISender sender, CancellationToken ct) =>
        {
            try
            {
                var pageId = await sender.Send(new RenamePublicHtmlPageCommand(id, request.Slug), ct);
                return Results.Ok(new { id = pageId });
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (ArgumentException ex) { return Results.BadRequest(new { code = "PUBLIC_HTML_INVALID_SLUG", message = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { code = "PUBLIC_HTML_SLUG_EXISTS", message = ex.Message }); }
        }).RequireAuthorization(Permissions.FileWrite);

        group.MapPut("/{id:guid}/publish", async (Guid id, [FromBody] PublishRequest request, ISender sender, CancellationToken ct) =>
        {
            try
            {
                var pageId = await sender.Send(new SetPublicHtmlPagePublishedCommand(id, request.IsPublished), ct);
                return Results.Ok(new { id = pageId, isPublished = request.IsPublished });
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        }).RequireAuthorization(Permissions.FileWrite);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeletePublicHtmlPageCommand(id), ct);
            return Results.NoContent();
        }).RequireAuthorization(Permissions.FileDelete);

        endpoints.MapGet("/api/public-html/{slug}/content", async (string slug, ISender sender, CancellationToken ct) =>
        {
            var stream = await sender.Send(new OpenPublishedPublicHtmlContentQuery(slug), ct);
            return stream is null ? Results.NotFound() : Results.Stream(stream, "text/html; charset=utf-8");
        }).AllowAnonymous().WithTags("Public HTML Pages");

        endpoints.MapGet("/html/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
        {
            var page = await sender.Send(new GetPublishedPublicHtmlPageQuery(slug), ct);
            if (page is null) return Results.NotFound();
            await sender.Send(new RecordPublicHtmlPageViewCommand(page.Id), ct);
            var contentUrl = $"/api/public-html/{Uri.EscapeDataString(slug)}/content";
            var title = System.Net.WebUtility.HtmlEncode(page.Title);
            var html = $"<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>{title}</title><style>html,body{{margin:0;width:100%;height:100%;overflow:hidden}}iframe{{width:100%;height:100%;border:0}}</style></head><body><iframe title=\"{title}\" src=\"{contentUrl}\" sandbox=\"allow-scripts allow-forms allow-popups allow-modals\" referrerpolicy=\"no-referrer\"></iframe></body></html>";
            return Results.Content(html, "text/html; charset=utf-8");
        }).AllowAnonymous().WithTags("Public HTML Pages");
    }

    public sealed record RenameRequest(string Slug);
    public sealed record PublishRequest(bool IsPublished);
}
