using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Tags.Commands.CreateTag;

public sealed record CreateTagCommand : IAppCommand<Guid>
{
    public required string Name { get; init; }
}
