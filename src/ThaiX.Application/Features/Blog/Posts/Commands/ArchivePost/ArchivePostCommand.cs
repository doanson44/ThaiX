using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Posts.Commands.ArchivePost;

[InvalidateCache(CacheGroups.BlogPosts)]
public sealed record ArchivePostCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
