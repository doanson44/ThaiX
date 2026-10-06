using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Posts.Commands.UnpublishPost;

[InvalidateCache(CacheGroups.BlogPosts)]
public sealed record UnpublishPostCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
