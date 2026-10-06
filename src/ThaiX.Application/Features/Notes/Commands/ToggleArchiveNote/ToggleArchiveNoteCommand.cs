using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Notes.Commands.ToggleArchiveNote;

[InvalidateCache(CacheGroups.Notes)]
public sealed record ToggleArchiveNoteCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
