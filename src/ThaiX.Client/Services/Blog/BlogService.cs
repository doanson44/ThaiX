using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Blog;

public sealed class BlogService : IBlogService
{
    private const string PostsBaseUrl = "api/blog/posts";
    private const string CategoriesBaseUrl = "api/blog/categories";
    private const string TagsBaseUrl = "api/blog/tags";
    private const string AiBaseUrl = "api/blog/ai";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public BlogService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<PostListItemDto>> GetPostsAsync(
        PostsListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = BuildPostsListQuery(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.BlogPost,
            query,
            ct => FetchPostsAsync(query, ct),
            cancellationToken: cancellationToken);
    }

    public Task<PagedApiResponse<PublishedPostListItemDto>> GetPublishedPostsAsync(
        PublishedPostsListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = BuildPublishedPostsListQuery(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.BlogPostPublic,
            query,
            ct => FetchPublishedPostsAsync(query, ct),
            cancellationToken: cancellationToken);
    }

    public Task<PostDto?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.BlogPost,
            id.ToString(),
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{PostsBaseUrl}/{id}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                return await ApiResponseReader.ReadSuccessDataAsync<PostDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public Task<PostDto?> GetPostBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.BlogPostPublic,
            slug,
            async ct =>
            {
                using var response = await _httpClient.GetAsync(
                    $"{PostsBaseUrl}/public/{Uri.EscapeDataString(slug)}",
                    ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                return await ApiResponseReader.ReadSuccessDataAsync<PostDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreatePostAsync(CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(PostsBaseUrl, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateBlogPostCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdatePostAsync(Guid id, UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{PostsBaseUrl}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogPostCachesAsync(cancellationToken);
    }

    public async Task DeletePostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{PostsBaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogPostCachesAsync(cancellationToken);
    }

    public async Task RestorePostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{PostsBaseUrl}/{id}/restore", content: null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogPostPublicCachesAsync(cancellationToken);
    }

    public async Task<Guid> DuplicatePostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{PostsBaseUrl}/{id}/duplicate", content: null, cancellationToken);
        var newId = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateBlogPostCachesAsync(cancellationToken);
        return newId;
    }

    public async Task PublishPostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{PostsBaseUrl}/{id}/publish", content: null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogPostAndPublicCachesAsync(cancellationToken);
    }

    public async Task SchedulePostAsync(Guid id, SchedulePostRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{PostsBaseUrl}/{id}/schedule", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogPostCachesAsync(cancellationToken);
    }

    public async Task UnpublishPostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{PostsBaseUrl}/{id}/unpublish", content: null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogPostAndPublicCachesAsync(cancellationToken);
    }

    public async Task ArchivePostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"{PostsBaseUrl}/{id}/archive", content: null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogPostPublicCachesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.BlogCategory,
            "list",
            async ct =>
            {
                using var response = await _httpClient.GetAsync(CategoriesBaseUrl, ct);
                return await ApiResponseReader.ReadSuccessDataAsync<IReadOnlyList<CategoryDto>>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(CategoriesBaseUrl, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateBlogCategoryCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{CategoriesBaseUrl}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogCategoryCachesAsync(cancellationToken);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{CategoriesBaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogCategoryCachesAsync(cancellationToken);
    }

    public Task<PagedApiResponse<TagDto>> SearchTagsAsync(
        string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = BuildTagsSearchQuery(search, pageNumber, pageSize);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.BlogTag,
            query,
            ct => FetchTagsAsync(query, ct),
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(TagsBaseUrl, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateBlogTagCachesAsync(cancellationToken);
        return id;
    }

    public async Task DeleteTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{TagsBaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateBlogTagCachesAsync(cancellationToken);
    }

    public async Task<string> GenerateTitleAsync(GenerateTitleRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{AiBaseUrl}/generate-title", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<BlogAiTextResponseDto>(response, cancellationToken);
        return result.Text;
    }

    public async Task<string> GenerateSummaryAsync(GenerateSummaryRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{AiBaseUrl}/generate-summary", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<BlogAiTextResponseDto>(response, cancellationToken);
        return result.Text;
    }

    public async Task<string> RewriteTextAsync(RewriteTextRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{AiBaseUrl}/rewrite", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<BlogAiTextResponseDto>(response, cancellationToken);
        return result.Text;
    }

    public async Task<string> ImproveGrammarAsync(ImproveGrammarRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{AiBaseUrl}/improve-grammar", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<BlogAiTextResponseDto>(response, cancellationToken);
        return result.Text;
    }

    public async Task<string> GenerateTagsAsync(GenerateTagsRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{AiBaseUrl}/generate-tags", request, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<BlogAiTextResponseDto>(response, cancellationToken);
        return result.Text;
    }

    public async Task<GenerateSeoResponseDto> GenerateSeoAsync(GenerateSeoRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{AiBaseUrl}/generate-seo", request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<GenerateSeoResponseDto>(response, cancellationToken);
    }

    public async Task<ReviewPostResponseDto> ReviewPostAsync(ReviewPostRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{AiBaseUrl}/review", request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<ReviewPostResponseDto>(response, cancellationToken);
    }

    public async Task<string> UploadImageAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var storageKey = $"blog/images/{Guid.NewGuid():N}-{fileName}";

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(storageKey), "storageKey");

        using var response = await _httpClient.PostAsync("api/files/upload", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException("EXTERNAL_API_ERROR", $"Image upload failed: {response.StatusCode}", response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<UploadedFileDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new ApiException("CLIENT_INVALID_RESPONSE", "Invalid response from upload API.", HttpStatusCode.InternalServerError);
        }

        return result.Url;
    }

    private async Task<PagedApiResponse<PostListItemDto>> FetchPostsAsync(string query, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"{PostsBaseUrl}{query}", cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<PostListItemDto>(response, cancellationToken);
    }

    private async Task<PagedApiResponse<PublishedPostListItemDto>> FetchPublishedPostsAsync(
        string query,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"{PostsBaseUrl}/public{query}", cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<PublishedPostListItemDto>(response, cancellationToken);
    }

    private async Task<PagedApiResponse<TagDto>> FetchTagsAsync(string query, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"{TagsBaseUrl}{query}", cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<TagDto>(response, cancellationToken);
    }

    private Task InvalidateBlogPostCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.BlogPost, cancellationToken);

    private Task InvalidateBlogPostPublicCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.BlogPostPublic, cancellationToken);

    private async Task InvalidateBlogPostAndPublicCachesAsync(CancellationToken cancellationToken)
    {
        await InvalidateBlogPostCachesAsync(cancellationToken);
        await InvalidateBlogPostPublicCachesAsync(cancellationToken);
    }

    private Task InvalidateBlogCategoryCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.BlogCategory, cancellationToken);

    private Task InvalidateBlogTagCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.BlogTag, cancellationToken);

    private static string BuildPostsListQuery(PostsListRequest request)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"?pageNumber={request.PageNumber}&pageSize={request.PageSize}");
        if (!string.IsNullOrWhiteSpace(request.SortBy))
            sb.Append(CultureInfo.InvariantCulture, $"&sortBy={Uri.EscapeDataString(request.SortBy)}");
        if (request.SortDescending)
            sb.Append("&sortDescending=true");
        if (!string.IsNullOrWhiteSpace(request.Status))
            sb.Append(CultureInfo.InvariantCulture, $"&status={Uri.EscapeDataString(request.Status)}");
        if (request.CategoryId.HasValue)
            sb.Append(CultureInfo.InvariantCulture, $"&categoryId={request.CategoryId.Value}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append(CultureInfo.InvariantCulture, $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        return sb.ToString();
    }

    private static string BuildPublishedPostsListQuery(PublishedPostsListRequest request)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"?pageNumber={request.PageNumber}&pageSize={request.PageSize}");
        if (request.CategoryId.HasValue)
            sb.Append(CultureInfo.InvariantCulture, $"&categoryId={request.CategoryId.Value}");
        if (request.TagId.HasValue)
            sb.Append(CultureInfo.InvariantCulture, $"&tagId={request.TagId.Value}");
        return sb.ToString();
    }

    private static string BuildTagsSearchQuery(string? search, int pageNumber, int pageSize)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"?pageNumber={pageNumber}&pageSize={pageSize}");
        if (!string.IsNullOrWhiteSpace(search))
            sb.Append(CultureInfo.InvariantCulture, $"&search={Uri.EscapeDataString(search)}");
        return sb.ToString();
    }
}
