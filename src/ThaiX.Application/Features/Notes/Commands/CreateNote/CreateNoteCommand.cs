using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Notes;

namespace ThaiX.Application.Features.Notes.Commands.CreateNote;

[InvalidateCache(CacheGroups.Notes)]
public sealed record CreateNoteCommand : IAppCommand<Guid>
{
    public required string Title { get; init; }
    public required string Content { get; init; }
    public NoteColor Color { get; init; }
}
