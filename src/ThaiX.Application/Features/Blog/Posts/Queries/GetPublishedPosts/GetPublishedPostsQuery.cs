using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Blog.Posts.Queries.GetPublishedPosts;

/// <summary>
/// Public paged post list for the anonymous /blog page — published posts only, optional
/// category/tag filters.
/// </summary>
public sealed record GetPublishedPostsQuery : PagedRequest, IAppQuery<PagedResult<PublishedPostListItemDto>>, ICacheableQuery
{
    public Guid? CategoryId { get; init; }
    public Guid? TagId { get; init; }

    public string CacheKey => CacheKeys.Blog.PublishedPostsList(CategoryId, TagId, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.BlogPosts;
    public bool IsVersionedList => true;
}
