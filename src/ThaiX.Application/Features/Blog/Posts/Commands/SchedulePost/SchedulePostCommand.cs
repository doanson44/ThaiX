using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Posts.Commands.SchedulePost;

[InvalidateCache(CacheGroups.BlogPosts)]
public sealed record SchedulePostCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required DateTime PublishAt { get; init; }
}
