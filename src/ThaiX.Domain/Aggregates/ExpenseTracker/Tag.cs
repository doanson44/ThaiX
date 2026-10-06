using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class Tag : BaseAuditableEntity
{
    private Tag()
    {
    }

    public string Name { get; private set; } = null!;
    public string? ColorHex { get; private set; }

    public static Tag Create(string name, string? colorHex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            ColorHex = colorHex?.Trim()
        };
    }

    public void Update(string name, string? colorHex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        ColorHex = colorHex?.Trim();
    }

    public void SoftDelete()
    {
        Delete();
    }
}
