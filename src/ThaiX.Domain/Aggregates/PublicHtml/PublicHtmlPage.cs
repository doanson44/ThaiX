using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.PublicHtml;

public sealed class PublicHtmlPage : BaseAuditableEntity
{
    private PublicHtmlPage()
    {
    }

    public string Title { get; private set; } = null!;
    public string StorageKey { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public DateTime? LastViewedAtUtc { get; private set; }
    public string Slug { get; private set; } = null!;

    public static PublicHtmlPage Create(string title, string slug, string storageKey, long sizeBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeBytes);

        var page = new PublicHtmlPage
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            StorageKey = storageKey,
            SizeBytes = sizeBytes
        };
        page.Slug = NormalizeSlug(slug);
        return page;
    }

    public static string NormalizeSlug(string slug)
    {
        slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
        if (slug.Length is < 1 or > 80 ||
            slug.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '-')) ||
            slug.StartsWith('-') || slug.EndsWith('-') ||
            slug.Contains("--", StringComparison.Ordinal))
            throw new ArgumentException("Invalid slug.", nameof(slug));

        return slug;
    }

    public void RenameSlug(string slug)
    {
        slug = NormalizeSlug(slug);
        Slug = slug;
    }

    public void Publish(DateTime publishedAtUtc)
    {
        if (IsDeleted)
            throw new InvalidOperationException("A deleted HTML page cannot be published.");

        IsPublished = true;
        PublishedAtUtc = DateTime.SpecifyKind(publishedAtUtc, DateTimeKind.Utc);
    }

    public void Unpublish()
    {
        IsPublished = false;
        PublishedAtUtc = null;
    }

    public void RecordView(DateTime viewedAtUtc)
    {
        if (!IsPublished || IsDeleted)
            return;

        LastViewedAtUtc = DateTime.SpecifyKind(viewedAtUtc, DateTimeKind.Utc);
    }

    public void SoftDeletePage()
    {
        Delete();
        IsPublished = false;
        PublishedAtUtc = null;
    }
}
