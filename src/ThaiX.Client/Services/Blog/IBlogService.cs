using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Blog;

namespace ThaiX.Client.Services.Blog;

/// <summary>
/// Client service for the blog API (posts, categories, tags, AI writing assistance).
/// </summary>
public interface IBlogService
{
    Task<PagedApiResponse<PostListItemDto>> GetPostsAsync(PostsListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<PublishedPostListItemDto>> GetPublishedPostsAsync(PublishedPostsListRequest request, CancellationToken cancellationToken = default);
    Task<PostDto?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PostDto?> GetPostBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Guid> CreatePostAsync(CreatePostRequest request, CancellationToken cancellationToken = default);
    Task UpdatePostAsync(Guid id, UpdatePostRequest request, CancellationToken cancellationToken = default);
    Task DeletePostAsync(Guid id, CancellationToken cancellationToken = default);
    Task RestorePostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> DuplicatePostAsync(Guid id, CancellationToken cancellationToken = default);
    Task PublishPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task SchedulePostAsync(Guid id, SchedulePostRequest request, CancellationToken cancellationToken = default);
    Task UnpublishPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task ArchivePostAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedApiResponse<TagDto>> SearchTagsAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Guid> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken = default);
    Task DeleteTagAsync(Guid id, CancellationToken cancellationToken = default);

    Task<string> GenerateTitleAsync(GenerateTitleRequest request, CancellationToken cancellationToken = default);
    Task<string> GenerateSummaryAsync(GenerateSummaryRequest request, CancellationToken cancellationToken = default);
    Task<string> RewriteTextAsync(RewriteTextRequest request, CancellationToken cancellationToken = default);
    Task<string> ImproveGrammarAsync(ImproveGrammarRequest request, CancellationToken cancellationToken = default);
    Task<string> GenerateTagsAsync(GenerateTagsRequest request, CancellationToken cancellationToken = default);
    Task<GenerateSeoResponseDto> GenerateSeoAsync(GenerateSeoRequest request, CancellationToken cancellationToken = default);
    Task<ReviewPostResponseDto> ReviewPostAsync(ReviewPostRequest request, CancellationToken cancellationToken = default);

    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
}
