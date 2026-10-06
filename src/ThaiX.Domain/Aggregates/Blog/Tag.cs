using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Blog;

/// <summary>
/// A blog tag. Cheap and disposable by design — no update, only create/delete (MVP scope).
/// </summary>
public sealed class Tag : BaseAuditableEntity
{
    private Tag()
    {
    }

    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public static Tag Create(string name, string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = NormalizeSlug(slug)
        };
    }

    public void SoftDelete()
    {
        Delete();
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().Trim('/').ToLowerInvariant();
    }
}
