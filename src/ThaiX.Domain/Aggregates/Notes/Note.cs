using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Notes;

public sealed class Note : BaseAuditableEntity
{
    private Note()
    {
    }

    public Guid OwnerId { get; private set; }

    public string Title { get; private set; } = null!;

    public string Content { get; private set; } = null!;

    public NoteColor Color { get; private set; }

    public bool IsPinned { get; private set; }

    public bool IsArchived { get; private set; }

    public static Note Create(
        Guid ownerId,
        string title,
        string content,
        NoteColor color = NoteColor.Default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new Note
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = title.Trim(),
            Content = content.Trim(),
            Color = color,
            IsPinned = false,
            IsArchived = false
        };
    }

    public void Update(string title, string content, NoteColor color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Title = title.Trim();
        Content = content.Trim();
        Color = color;
    }

    public void TogglePin()
    {
        IsPinned = !IsPinned;
    }

    public void ToggleArchive()
    {
        IsArchived = !IsArchived;
    }

    public void ChangeColor(NoteColor color)
    {
        Color = color;
    }

    public void SoftDelete()
    {
        Delete();
    }

    public void RestoreNote()
    {
        Restore();
    }
}
