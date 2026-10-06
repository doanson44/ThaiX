using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Notes;

namespace ThaiX.Application.Features.Notes.Commands.UpdateNote;

[InvalidateCache(CacheGroups.Notes)]
public sealed record UpdateNoteCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public NoteColor Color { get; init; }
}
