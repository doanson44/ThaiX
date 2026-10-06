using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Posts.Commands.DuplicatePost;

[InvalidateCache(CacheGroups.BlogPosts)]
public sealed record DuplicatePostCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }
}
