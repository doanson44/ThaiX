namespace ThaiX.Client.Models.Blog;

public sealed class PostDto
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string ContentHtml { get; init; } = string.Empty;
    public string? FeaturedImageUrl { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public int ReadTimeMinutes { get; init; }
    public IReadOnlyList<Guid> TagIds { get; init; } = [];
    public IReadOnlyList<string> TagNames { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class PostListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? CategoryName { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int ReadTimeMinutes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class PublishedPostListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string? FeaturedImageUrl { get; init; }
    public string? CategoryName { get; init; }
    public int ReadTimeMinutes { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public sealed class CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}

public sealed class TagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}

public sealed class PostsListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public string? SearchTerm { get; set; }
}

public sealed class PublishedPostsListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
}

public sealed class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public IReadOnlyList<Guid> TagIds { get; set; } = [];
}

public sealed class UpdatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public IReadOnlyList<Guid> TagIds { get; set; } = [];
}

public sealed class SchedulePostRequest
{
    public DateTime PublishAt { get; set; }
}

public sealed class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public sealed class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public sealed class CreateTagRequest
{
    public string Name { get; set; } = string.Empty;
}

// --- AI ---

public sealed class GenerateTitleRequest
{
    public string ContentHtml { get; set; } = string.Empty;
}

public sealed class GenerateSummaryRequest
{
    public string Title { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
}

public sealed class RewriteTextRequest
{
    public string? Text { get; set; }
    public string Tone { get; set; } = "Professional";
}

public sealed class ImproveGrammarRequest
{
    public string? Text { get; set; }
}

public sealed class GenerateTagsRequest
{
    public string Title { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
}

public sealed class GenerateSeoRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public sealed class GenerateSeoResponseDto
{
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? Keywords { get; init; }
}

public sealed class ReviewPostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CategoryName { get; set; }
}

public sealed class ReviewPostResponseDto
{
    public string Verdict { get; init; } = string.Empty;
    public string Report { get; init; } = string.Empty;
}

public sealed class BlogAiTextResponseDto
{
    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// Raw response shape of POST /api/files/upload (not wrapped in ApiResponse envelope).
/// </summary>
public sealed class UploadedFileDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Url { get; init; } = string.Empty;
}
