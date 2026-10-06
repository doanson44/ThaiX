using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Blog;

/// <summary>
/// A flat (non-hierarchical) blog category. Parent/child nesting is deferred — MVP scope.
/// </summary>
public sealed class Category : BaseAuditableEntity
{
    private Category()
    {
    }

    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? Icon { get; private set; }

    public string? Color { get; private set; }

    public static Category Create(string name, string slug, string? description, string? icon, string? color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = NormalizeSlug(slug),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim()
        };
    }

    public void Update(string name, string slug, string? description, string? icon, string? color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name.Trim();
        Slug = NormalizeSlug(slug);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
    }

    public void SoftDelete()
    {
        Delete();
    }

    public void RestoreEntity()
    {
        Restore();
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().Trim('/').ToLowerInvariant();
    }
}
