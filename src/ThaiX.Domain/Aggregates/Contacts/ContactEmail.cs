namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Represents an email address belonging to a contact.
/// Owned by the Contact aggregate.
/// </summary>
public sealed class ContactEmail
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }

    // Private constructor for EF Core
    private ContactEmail() { }

    public static ContactEmail Create(Guid contactId, string value, bool isPrimary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        return new ContactEmail
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            Value = value.Trim().ToLowerInvariant(),
            IsPrimary = isPrimary
        };
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
