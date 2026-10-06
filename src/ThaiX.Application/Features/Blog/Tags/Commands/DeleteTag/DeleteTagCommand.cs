using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Blog.Tags.Commands.DeleteTag;

public sealed record DeleteTagCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
