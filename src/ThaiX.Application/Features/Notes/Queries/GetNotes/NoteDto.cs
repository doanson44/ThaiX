using ThaiX.Domain.Aggregates.Notes;

namespace ThaiX.Application.Features.Notes.Queries.GetNotes;

public sealed record NoteDto
{
    public required Guid Id { get; init; }
    public required Guid OwnerId { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required NoteColor Color { get; init; }
    public required bool IsPinned { get; init; }
    public required bool IsArchived { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
