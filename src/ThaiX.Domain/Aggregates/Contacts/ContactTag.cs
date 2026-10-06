namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Represents a tag/label applied to a contact.
/// Tag names are unique per contact.
/// Owned by the Contact aggregate.
/// </summary>
public sealed class ContactTag
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Private constructor for EF Core
    private ContactTag() { }

    public static ContactTag Create(Guid contactId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new ContactTag
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            Name = name.Trim()
        };
    }
}
