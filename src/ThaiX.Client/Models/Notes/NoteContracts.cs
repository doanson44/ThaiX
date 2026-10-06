namespace ThaiX.Client.Models.Notes;

public sealed class NoteDto
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Color { get; init; } = "Default";
    public bool IsPinned { get; init; }
    public bool IsArchived { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class NotesListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsArchived { get; set; }
    public bool? IsDeleted { get; set; }
}

public sealed class CreateNoteRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Color { get; set; } = "Default";
}

public sealed class UpdateNoteRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Color { get; set; } = "Default";
}
