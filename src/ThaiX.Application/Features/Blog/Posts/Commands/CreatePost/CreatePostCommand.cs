using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Posts.Commands.CreatePost;

[InvalidateCache(CacheGroups.BlogPosts)]
public sealed record CreatePostCommand : IAppCommand<Guid>
{
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public required string Summary { get; init; }
    public required string ContentHtml { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public Guid? CategoryId { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public IReadOnlyList<Guid> TagIds { get; init; } = [];
}
