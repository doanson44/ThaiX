using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.Ai;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Notifications;


namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private async Task<HttpResponseMessage> HandleBlogAsync(
        string method,
        string path,
        string query,
        int pageNumber,
        int pageSize,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (path.StartsWith("api/blog/categories", StringComparison.OrdinalIgnoreCase))
        {
            if (method == "GET")
                return _store.WithLock(() => DemoEnvelope.SuccessData(_store.BlogCategories.ToList()));
            if (method == "POST")
                return DemoEnvelope.SuccessData(Guid.NewGuid());
            return DemoEnvelope.Success();
        }

        if (path.StartsWith("api/blog/tags", StringComparison.OrdinalIgnoreCase))
        {
            if (method == "GET")
            {
                return _store.WithLock(() =>
                {
                    var search = GetSearchTerm(query);
                    IEnumerable<TagDto> rows = _store.BlogTags;
                    if (!string.IsNullOrWhiteSpace(search))
                        rows = rows.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
                    return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
                });
            }

            if (method == "POST")
                return DemoEnvelope.SuccessData(Guid.NewGuid());
            return DemoEnvelope.Success();
        }

        // api/blog/posts/public/{slug}
        if (segments.Length >= 5 &&
            segments[2].Equals("posts", StringComparison.OrdinalIgnoreCase) &&
            segments[3].Equals("public", StringComparison.OrdinalIgnoreCase) &&
            method == "GET")
        {
            var slug = Uri.UnescapeDataString(segments[4]);
            return _store.WithLock(() =>
            {
                var published = _store.PublishedPosts.FirstOrDefault(p =>
                                    p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase))
                                ?? _store.PublishedPosts[0];
                return DemoEnvelope.SuccessData(ToPostDto(published));
            });
        }

        // api/blog/posts/public
        if (path.Equals("api/blog/posts/public", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            return _store.WithLock(() => DemoEnvelope.Paged(_store.PublishedPosts.ToList(), pageNumber, pageSize));
        }

        // api/blog/posts/{id}
        if (segments.Length >= 4 &&
            segments[2].Equals("posts", StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(segments[3], out var postId))
        {
            if (method == "GET")
            {
                return _store.WithLock(() =>
                {
                    var item = _store.BlogPosts.FirstOrDefault(p => p.Id == postId) ?? _store.BlogPosts[0];
                    var published = _store.PublishedPosts.FirstOrDefault(p => p.Id == postId);
                    return DemoEnvelope.SuccessData(published is not null ? ToPostDto(published) : ToPostDto(item));
                });
            }

            if (method == "POST")
            {
                // lifecycle actions: publish/schedule/unpublish/archive/restore/duplicate
                if (segments.Length >= 5 && segments[4].Equals("duplicate", StringComparison.OrdinalIgnoreCase))
                    return DemoEnvelope.SuccessData(Guid.NewGuid());
                return DemoEnvelope.Success();
            }

            return DemoEnvelope.Success();
        }

        // api/blog/posts list / create
        if (path.StartsWith("api/blog/posts", StringComparison.OrdinalIgnoreCase))
        {
            if (method == "GET")
            {
                return _store.WithLock(() =>
                {
                    var search = GetSearchTerm(query);
                    var status = GetQueryValue(query, "status");
                    IEnumerable<PostListItemDto> rows = _store.BlogPosts;
                    if (!string.IsNullOrWhiteSpace(search))
                        rows = rows.Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrWhiteSpace(status))
                        rows = rows.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
                    return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
                });
            }

            if (method == "POST")
            {
                var body = await ReadJsonAsync<CreatePostRequest>(request, cancellationToken);
                return _store.WithLock(() =>
                {
                    var id = Guid.NewGuid();
                    _store.BlogPosts.Insert(0, new PostListItemDto
                    {
                        Id = id,
                        Title = body?.Title ?? "Demo Post",
                        Slug = body?.Slug ?? $"demo-post-{id:N}",
                        Status = "Draft",
                        CategoryName = "Demo",
                        ReadTimeMinutes = 3,
                        CreatedAt = DateTime.UtcNow
                    });
                    return DemoEnvelope.SuccessData(id);
                });
            }

            return DemoEnvelope.Success();
        }

        return method == "GET"
            ? _store.WithLock(() => DemoEnvelope.Paged(_store.BlogPosts.ToList(), pageNumber, pageSize))
            : DemoEnvelope.Success();
    }

    private static PostDto ToPostDto(PublishedPostListItemDto p) =>
        new()
        {
            Id = p.Id,
            AuthorId = Guid.Parse(DemoJwtFactory.DemoUserId),
            Title = p.Title,
            Slug = p.Slug,
            Summary = p.Summary,
            ContentHtml = $"<p>{p.Summary}</p><p>Demo blog content for <strong>{p.Title}</strong>.</p>",
            FeaturedImageUrl = p.FeaturedImageUrl,
            CategoryName = p.CategoryName,
            Status = "Published",
            PublishedAt = p.PublishedAt,
            ReadTimeMinutes = p.ReadTimeMinutes,
            TagNames = ["demo", "ThaiX"],
            CreatedAt = p.PublishedAt ?? DateTime.UtcNow,
            UpdatedAt = p.PublishedAt
        };

    private static PostDto ToPostDto(PostListItemDto p) =>
        new()
        {
            Id = p.Id,
            AuthorId = Guid.Parse(DemoJwtFactory.DemoUserId),
            Title = p.Title,
            Slug = p.Slug,
            Summary = p.Title,
            ContentHtml = $"<p>Demo draft content for <strong>{p.Title}</strong>.</p>",
            CategoryName = p.CategoryName,
            Status = p.Status,
            PublishedAt = p.PublishedAt,
            ScheduledAt = p.ScheduledAt,
            ReadTimeMinutes = p.ReadTimeMinutes,
            TagNames = ["demo"],
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };

    private static HttpResponseMessage HandleBlogAi(string method, string path)
    {
        if (method != "POST")
        {
            return DemoEnvelope.SuccessData(new { demo = true });
        }

        if (path.Contains("generate-seo", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new GenerateSeoResponseDto
            {
                MetaTitle = "Demo SEO Title",
                MetaDescription = "Demo SEO description for offline mode.",
                Keywords = "demo, blog, ThaiX"
            });
        }

        if (path.Contains("/review", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new ReviewPostResponseDto
            {
                Verdict = "Approve",
                Report = "Demo review: content looks ready to publish."
            });
        }

        return DemoEnvelope.SuccessData(new BlogAiTextResponseDto { Text = "Demo blog AI text." });
    }

}
