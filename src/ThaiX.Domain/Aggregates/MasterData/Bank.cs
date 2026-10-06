using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.MasterData;

/// <summary>
/// Represents a bank operating within a country.
/// </summary>
public sealed class Bank : BaseAuditableEntity
{
    /// <summary>
    /// Unique bank code (e.g., SWIFT/BIC code).
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable bank name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Code of the country this bank operates in.
    /// </summary>
    public string CountryCode { get; private set; } = string.Empty;

    /// <summary>
    /// Foreign key to the parent Country entity.
    /// </summary>
    public Guid CountryId { get; private set; }

    /// <summary>
    /// Navigation property to the parent Country.
    /// </summary>
    public Country Country { get; private set; } = null!;

    // Private constructor for EF Core
    private Bank() { }

    public static Bank Create(string code, string name, string countryCode, Guid countryId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Bank code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bank name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code is required.", nameof(countryCode));

        if (countryId == Guid.Empty)
            throw new ArgumentException("Country ID is required.", nameof(countryId));

        return new Bank
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            CountryId = countryId
        };
    }

    public void UpdateDetails(string code, string name, string countryCode, Guid countryId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Bank code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bank name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code is required.", nameof(countryCode));

        if (countryId == Guid.Empty)
            throw new ArgumentException("Country ID is required.", nameof(countryId));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        CountryCode = countryCode.Trim().ToUpperInvariant();
        CountryId = countryId;
    }

    public void SoftDelete()
    {
        Delete();
    }

    public void RestoreEntity()
    {
        Restore();
    }
}
