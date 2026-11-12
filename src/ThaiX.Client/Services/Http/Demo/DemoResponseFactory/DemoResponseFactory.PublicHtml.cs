using System.Net;
using ThaiX.Client.Models.PublicHtml;

namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private async Task<HttpResponseMessage> HandlePublicHtmlAsync(
        string method,
        string path,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (method == "GET" && path.Equals("api/public-html", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.Json(_store.PublicHtmlPages.ToList()));
        }

        if (method == "POST" && path.Equals("api/public-html", StringComparison.OrdinalIgnoreCase))
        {
            var id = Guid.NewGuid();
            _store.WithLock(() => _store.PublicHtmlPages.Insert(0, new PublicHtmlPageModel
            {
                Id = id,
                Title = "Uploaded demo page",
                Slug = $"demo-page-{id:N}"[..18],
                SizeBytes = request.Content?.Headers.ContentLength ?? 0,
                IsPublished = false,
                CreatedAtUtc = DateTime.UtcNow
            }));
            return DemoEnvelope.Json(new { id }, HttpStatusCode.Created);
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3 || !Guid.TryParse(segments[2], out var pageId))
        {
            return DemoEnvelope.Failure("DEMO_PUBLIC_HTML_NOT_FOUND", "The demo HTML page was not found.", HttpStatusCode.NotFound);
        }

        if (method == "PATCH" && segments.Length == 4 && segments[3].Equals("slug", StringComparison.OrdinalIgnoreCase))
        {
            var body = await ReadJsonAsync<RenamePublicHtmlPageRequest>(request, cancellationToken);
            if (body is null)
            {
                return DemoEnvelope.Failure("DEMO_PUBLIC_HTML_INVALID_SLUG", "A slug is required.");
            }

            return _store.WithLock(() =>
            {
                var page = _store.PublicHtmlPages.FirstOrDefault(item => item.Id == pageId);
                if (page is null)
                {
                    return DemoEnvelope.Failure("DEMO_PUBLIC_HTML_NOT_FOUND", "The demo HTML page was not found.", HttpStatusCode.NotFound);
                }

                page.Slug = body.Slug;
                return DemoEnvelope.Success();
            });
        }

        if (method == "PUT" && segments.Length == 4 && segments[3].Equals("publish", StringComparison.OrdinalIgnoreCase))
        {
            var body = await ReadJsonAsync<SetPublicHtmlPagePublishedRequest>(request, cancellationToken);
            if (body is null)
            {
                return DemoEnvelope.Failure("DEMO_PUBLIC_HTML_INVALID_STATUS", "A publish status is required.");
            }

            return _store.WithLock(() =>
            {
                var page = _store.PublicHtmlPages.FirstOrDefault(item => item.Id == pageId);
                if (page is null)
                {
                    return DemoEnvelope.Failure("DEMO_PUBLIC_HTML_NOT_FOUND", "The demo HTML page was not found.", HttpStatusCode.NotFound);
                }

                page.IsPublished = body.IsPublished;
                page.PublishedAtUtc = body.IsPublished ? DateTime.UtcNow : null;
                return DemoEnvelope.Success();
            });
        }

        if (method == "DELETE" && segments.Length == 3)
        {
            return _store.WithLock(() =>
            {
                _store.PublicHtmlPages.RemoveAll(item => item.Id == pageId);
                return DemoEnvelope.Success();
            });
        }

        return DemoEnvelope.Failure("DEMO_PUBLIC_HTML_ENDPOINT_NOT_FOUND", "The demo HTML endpoint was not found.", HttpStatusCode.NotFound);
    }

    private sealed record RenamePublicHtmlPageRequest(string Slug);
    private sealed record SetPublicHtmlPagePublishedRequest(bool IsPublished);
}
