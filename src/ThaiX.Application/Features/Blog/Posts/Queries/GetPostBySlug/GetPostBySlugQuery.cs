using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Posts.Queries.GetPostBySlug;

/// <summary>
/// Public lookup used by the anonymous /blog/{slug} page. Returns null unless a published
/// post exists at that slug — mirrors GetResumeBySlugQuery exactly.
/// </summary>
public sealed record GetPostBySlugQuery : IAppQuery<PostDto?>, ICacheableQuery
{
    public required string Slug { get; init; }

    public string CacheKey => CacheKeys.Blog.BySlug(Slug);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.BlogPosts;
    public bool IsVersionedList => true;
}
