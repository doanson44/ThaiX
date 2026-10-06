using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Notes.Commands.DeleteNote;

[InvalidateCache(CacheGroups.Notes)]
public sealed record DeleteNoteCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
