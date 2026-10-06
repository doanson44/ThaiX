using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Application.Features.Blog.Posts.Queries.GetPosts;

/// <summary>
/// Admin paged post list — any status, optional status/category/title-search filters.
/// </summary>
public sealed record GetPostsQuery : PagedRequest, IAppQuery<PagedResult<PostListItemDto>>, ICacheableQuery
{
    public PostStatus? Status { get; init; }
    public Guid? CategoryId { get; init; }
    public string? SearchTerm { get; init; }

    public string CacheKey => CacheKeys.Blog.PostsList(Status?.ToString(), CategoryId, SearchTerm, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.BlogPosts;
    public bool IsVersionedList => true;
}
