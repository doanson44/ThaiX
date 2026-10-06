using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Resumes;

/// <summary>
/// A single editable resume/portfolio document, published at a public slug for job hunting.
/// Structured sections (experience, projects, education, skills, links) are stored as a
/// single JSON document since they are always a handful of rows with no need for
/// pagination/filtering — mirrors the JSON-column pattern already used by Notification.DataJson.
/// </summary>
public sealed class ResumeProfile : BaseAuditableEntity
{
    private ResumeProfile()
    {
    }

    public Guid OwnerId { get; private set; }

    public string Slug { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string Headline { get; private set; } = null!;

    public string? MetaDescription { get; private set; }

    public bool IsPublished { get; private set; }

    public string ContentJson { get; private set; } = null!;

    public static ResumeProfile Create(
        Guid ownerId,
        string slug,
        string fullName,
        string headline,
        string? metaDescription,
        string contentJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(headline);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentJson);

        return new ResumeProfile
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Slug = NormalizeSlug(slug),
            FullName = fullName.Trim(),
            Headline = headline.Trim(),
            MetaDescription = string.IsNullOrWhiteSpace(metaDescription) ? null : metaDescription.Trim(),
            IsPublished = false,
            ContentJson = contentJson
        };
    }

    public void UpdateContent(
        string slug,
        string fullName,
        string headline,
        string? metaDescription,
        string contentJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(headline);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentJson);

        Slug = NormalizeSlug(slug);
        FullName = fullName.Trim();
        Headline = headline.Trim();
        MetaDescription = string.IsNullOrWhiteSpace(metaDescription) ? null : metaDescription.Trim();
        ContentJson = contentJson;
    }

    public void SetPublished(bool isPublished)
    {
        IsPublished = isPublished;
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().Trim('/').ToLowerInvariant();
    }
}
