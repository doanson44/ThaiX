using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Posts;

/// <summary>
/// Full post detail, used by the admin editor (any status) and the public post-view page
/// (published only, enforced by the query handler, not this DTO).
/// </summary>
public sealed record PostDto
{
    public required Guid Id { get; init; }
    public required Guid AuthorId { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public required string Summary { get; init; }
    public required string ContentHtml { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public required PostStatus Status { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public required int ReadTimeMinutes { get; init; }
    public IReadOnlyList<Guid> TagIds { get; set; } = [];
    public IReadOnlyList<string> TagNames { get; set; } = [];
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Lightweight row for the admin post list grid.
/// </summary>
public sealed record PostListItemDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public required PostStatus Status { get; init; }
    public string? CategoryName { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public required int ReadTimeMinutes { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Lightweight row for the public blog list page — published posts only.
/// </summary>
public sealed record PublishedPostListItemDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public required string Summary { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string? CategoryName { get; init; }
    public required int ReadTimeMinutes { get; init; }
    public DateTime? PublishedAt { get; init; }
}
