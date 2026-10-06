namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Represents a social media link belonging to a contact.
/// Owned by the Contact aggregate.
/// </summary>
public sealed class ContactSocialLink
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public string Platform { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;

    // Private constructor for EF Core
    private ContactSocialLink() { }

    public static ContactSocialLink Create(Guid contactId, string platform, string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(platform, nameof(platform));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        return new ContactSocialLink
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            Platform = platform.Trim(),
            Url = url.Trim()
        };
    }
}
