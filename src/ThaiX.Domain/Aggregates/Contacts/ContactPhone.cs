namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Represents a phone number belonging to a contact.
/// Owned by the Contact aggregate. Value = display; NormalizedValue = indexed for lookup/dedup.
/// </summary>
public sealed class ContactPhone
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    /// <summary>Normalized display value (e.g. 84912345678). Same as NormalizedValue.</summary>
    public string Value { get; private set; } = string.Empty;
    /// <summary>Digits-only normalized form for lookup (e.g. 84912345678).</summary>
    public string NormalizedValue { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }

    // Private constructor for EF Core
    private ContactPhone() { }

    public static ContactPhone Create(Guid contactId, string value, bool isPrimary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        var trimmed = value.Trim();
        var normalized = PhoneNormalizer.Normalize(trimmed)
            ?? throw new ArgumentException("Phone number has no valid digits.", nameof(value));

        return new ContactPhone
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            Value = normalized,
            NormalizedValue = normalized,
            IsPrimary = isPrimary
        };
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
