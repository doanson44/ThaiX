namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Represents a physical address belonging to a contact.
/// References master data codes (Country, City, District) by value, not FK.
/// Owned by the Contact aggregate.
/// </summary>
public sealed class ContactAddress
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public string Street { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string CityCode { get; private set; } = string.Empty;
    public string DistrictCode { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }

    // Private constructor for EF Core
    private ContactAddress() { }

    public static ContactAddress Create(
        Guid contactId,
        string street,
        string countryCode,
        string cityCode,
        string districtCode,
        string postalCode,
        bool isPrimary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street, nameof(street));
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode, nameof(countryCode));

        return new ContactAddress
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            Street = street.Trim(),
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            CityCode = cityCode?.Trim().ToUpperInvariant() ?? string.Empty,
            DistrictCode = districtCode?.Trim().ToUpperInvariant() ?? string.Empty,
            PostalCode = postalCode?.Trim() ?? string.Empty,
            IsPrimary = isPrimary
        };
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
