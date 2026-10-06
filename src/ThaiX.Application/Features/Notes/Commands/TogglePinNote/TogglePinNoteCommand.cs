using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Notes.Commands.TogglePinNote;

[InvalidateCache(CacheGroups.Notes)]
public sealed record TogglePinNoteCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
