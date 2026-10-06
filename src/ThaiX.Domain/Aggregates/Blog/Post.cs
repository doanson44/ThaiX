using System.Text.RegularExpressions;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Blog;

/// <summary>
/// A single blog post authored by an admin user. Content is stored as sanitized HTML
/// (authored via a WYSIWYG editor client-side, sanitized server-side in the Application
/// layer before reaching this aggregate) rather than a JSON-column document, since unlike
/// ResumeProfile this content is long-form free text, not structured sections.
/// Tag assignment (many-to-many with <see cref="Tag"/>) is managed via <see cref="PostTag"/>
/// join rows directly at the Application layer, not through this aggregate — mirrors how
/// ContactTag rows are queried/updated directly without loading the Contact aggregate.
/// </summary>
public sealed class Post : BaseAuditableEntity
{
    private const int WordsPerMinute = 200;
    private static readonly Regex HtmlTagPattern = new("<[^>]+>", RegexOptions.Compiled);

    private Post()
    {
    }

    public Guid AuthorId { get; private set; }

    public string Title { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string Summary { get; private set; } = null!;

    public string ContentHtml { get; private set; } = null!;

    public string? FeaturedImageUrl { get; private set; }

    public Guid? CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public ICollection<PostTag> PostTags { get; set; } = [];

    public PostStatus Status { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    public DateTime? ScheduledAt { get; private set; }

    public string? MetaTitle { get; private set; }

    public string? MetaDescription { get; private set; }

    public int ReadTimeMinutes { get; private set; }

    public static Post Create(
        Guid authorId,
        string title,
        string slug,
        string summary,
        string contentHtml,
        Guid? categoryId,
        string? metaTitle,
        string? metaDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHtml);

        return new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Title = title.Trim(),
            Slug = NormalizeSlug(slug),
            Summary = summary.Trim(),
            ContentHtml = contentHtml,
            CategoryId = categoryId,
            Status = PostStatus.Draft,
            MetaTitle = string.IsNullOrWhiteSpace(metaTitle) ? null : metaTitle.Trim(),
            MetaDescription = string.IsNullOrWhiteSpace(metaDescription) ? null : metaDescription.Trim(),
            ReadTimeMinutes = ComputeReadTime(contentHtml)
        };
    }

    public void UpdateContent(
        string title,
        string slug,
        string summary,
        string contentHtml,
        Guid? categoryId,
        string? metaTitle,
        string? metaDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHtml);

        Title = title.Trim();
        Slug = NormalizeSlug(slug);
        Summary = summary.Trim();
        ContentHtml = contentHtml;
        CategoryId = categoryId;
        MetaTitle = string.IsNullOrWhiteSpace(metaTitle) ? null : metaTitle.Trim();
        MetaDescription = string.IsNullOrWhiteSpace(metaDescription) ? null : metaDescription.Trim();
        ReadTimeMinutes = ComputeReadTime(contentHtml);
    }

    public void SetFeaturedImage(string? featuredImageUrl)
    {
        FeaturedImageUrl = string.IsNullOrWhiteSpace(featuredImageUrl) ? null : featuredImageUrl.Trim();
    }

    public void Publish()
    {
        if (Status is not (PostStatus.Draft or PostStatus.Scheduled))
        {
            throw new InvalidOperationException($"Cannot publish a post with status {Status}.");
        }

        Status = PostStatus.Published;
        PublishedAt = DateTime.UtcNow;
        ScheduledAt = null;
    }

    public void Schedule(DateTime publishAt)
    {
        if (publishAt <= DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(publishAt), "Scheduled publish time must be in the future.");
        }

        if (Status is not (PostStatus.Draft or PostStatus.Scheduled))
        {
            throw new InvalidOperationException($"Cannot schedule a post with status {Status}.");
        }

        Status = PostStatus.Scheduled;
        ScheduledAt = publishAt;
    }

    public void Unpublish()
    {
        Status = PostStatus.Draft;
        PublishedAt = null;
        ScheduledAt = null;
    }

    public void Archive()
    {
        Status = PostStatus.Archived;
    }

    public void SoftDelete()
    {
        Delete();
    }

    public void RestoreEntity()
    {
        Restore();
    }

    private static int ComputeReadTime(string contentHtml)
    {
        var plainText = HtmlTagPattern.Replace(contentHtml, " ");
        var wordCount = plainText.Split(
            [' ', '\t', '\n', '\r'],
            StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(wordCount / (double)WordsPerMinute));
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().Trim('/').ToLowerInvariant();
    }

    public Post Duplicate(Guid authorId, string slug)
    {
        var copy = Create(
            authorId,
            $"{Title} (Copy)",
            slug,
            Summary,
            ContentHtml,
            CategoryId,
            MetaTitle,
            MetaDescription);

        copy.SetFeaturedImage(FeaturedImageUrl);

        return copy;
    }
}
