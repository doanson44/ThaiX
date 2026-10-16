using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Presentation.Endpoints;

public static class PublicHtmlEndpoints
{
    public static void MapPublicHtmlEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/public-html").WithTags("Public HTML Pages");

        group.MapGet("/", async (IPublicHtmlPageService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)))
            .RequireAuthorization(Permissions.FileRead);

        group.MapPost("/", async ([FromForm] IFormFile file, [FromForm] string slug, [FromForm] string? title, IPublicHtmlPageService service, CancellationToken ct) =>
        {
            if (file.Length <= 0 || file.Length > 5 * 1024 * 1024)
                return Results.BadRequest(new { code = "PUBLIC_HTML_INVALID_SIZE", message = "HTML file must be between 1 byte and 5 MB." });
            try
            {
                await using var stream = file.OpenReadStream();
                var page = await service.UploadAsync(title ?? string.Empty, slug, file.FileName, stream, file.Length, ct);
                return Results.Created($"/api/public-html/{page.Id}", page);
            }
            catch (ArgumentException ex) { return Results.BadRequest(new { code = "PUBLIC_HTML_INVALID_INPUT", message = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { code = "PUBLIC_HTML_SLUG_EXISTS", message = ex.Message }); }
        }).RequireAuthorization(Permissions.FileWrite);

        group.MapPatch("/{id:guid}/slug", async (Guid id, [FromBody] RenameRequest request, IPublicHtmlPageService service, CancellationToken ct) =>
        {
            try { return Results.Ok(await service.RenameAsync(id, request.Slug, ct)); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (ArgumentException ex) { return Results.BadRequest(new { code = "PUBLIC_HTML_INVALID_SLUG", message = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { code = "PUBLIC_HTML_SLUG_EXISTS", message = ex.Message }); }
        }).RequireAuthorization(Permissions.FileWrite);

        group.MapPut("/{id:guid}/publish", async (Guid id, [FromBody] PublishRequest request, IPublicHtmlPageService service, CancellationToken ct) =>
        {
            try { return Results.Ok(await service.SetPublishedAsync(id, request.IsPublished, ct)); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        }).RequireAuthorization(Permissions.FileWrite);

        group.MapDelete("/{id:guid}", async (Guid id, IPublicHtmlPageService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization(Permissions.FileDelete);

        endpoints.MapGet("/api/public-html/{slug}/content", async (string slug, IPublicHtmlPageService service, CancellationToken ct) =>
        {
            var stream = await service.OpenPublishedContentAsync(slug, ct);
            return stream is null ? Results.NotFound() : Results.Stream(stream, "text/html; charset=utf-8");
        }).AllowAnonymous().WithTags("Public HTML Pages");

        endpoints.MapGet("/html/{slug}", async (string slug, IPublicHtmlPageService service, CancellationToken ct) =>
        {
            var page = await service.GetPublishedBySlugAsync(slug, ct);
            if (page is null) return Results.NotFound();
            var contentUrl = $"/api/public-html/{Uri.EscapeDataString(slug)}/content";
            var title = System.Net.WebUtility.HtmlEncode(page.Title);
            var html = $"""<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>{title}</title><style>html,body{{margin:0;width:100%;height:100%;overflow:hidden}}iframe{{width:100%;height:100%;border:0}}</style></head><body><iframe title="{title}" src="{contentUrl}" sandbox="allow-scripts allow-forms allow-popups allow-modals" referrerpolicy="no-referrer"></iframe></body></html>""";
            return Results.Content(html, "text/html; charset=utf-8");
        }).AllowAnonymous().WithTags("Public HTML Pages");
    }

    public sealed record RenameRequest(string Slug);
    public sealed record PublishRequest(bool IsPublished);
}