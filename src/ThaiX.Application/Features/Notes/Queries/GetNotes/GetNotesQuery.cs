using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Notes.Queries.GetNotes;

public sealed record GetNotesQuery : PagedRequest, IAppQuery<PagedResult<NoteDto>>, ICacheableQuery
{
    public Guid OwnerId { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsPinned { get; init; }
    public bool? IsArchived { get; init; }
    public bool? IsDeleted { get; init; }

    public string CacheKey => CacheKeys.Notes.List(
        OwnerId,
        SearchTerm,
        IsPinned,
        IsArchived,
        IsDeleted,
        PageNumber,
        PageSize);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.Notes;
    public bool IsVersionedList => true;
}
