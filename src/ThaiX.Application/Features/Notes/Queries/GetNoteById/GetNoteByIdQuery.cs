using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Notes.Queries.GetNotes;

namespace ThaiX.Application.Features.Notes.Queries.GetNoteById;

public sealed record GetNoteByIdQuery : IAppQuery<NoteDto?>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.Notes.Detail(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.Notes;
}
