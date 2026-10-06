using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Posts.Queries.GetPostById;

/// <summary>
/// Admin lookup by Id — returns a post regardless of status. Not cached, since the editor
/// always wants the freshest draft state.
/// </summary>
public sealed record GetPostByIdQuery : IAppQuery<PostDto?>
{
    public required Guid Id { get; init; }
}
