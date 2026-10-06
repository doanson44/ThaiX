using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.MasterData;

/// <summary>
/// Represents a country in the master data catalog.
/// </summary>
public sealed class Country : BaseAuditableEntity
{
    private readonly List<City> _cities = new();
    private readonly List<Bank> _banks = new();

    /// <summary>
    /// Unique country code (e.g., "VN", "TH", "US").
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable country name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Cities belonging to this country.
    /// </summary>
    public IReadOnlyCollection<City> Cities => _cities.AsReadOnly();

    /// <summary>
    /// Banks operating in this country.
    /// </summary>
    public IReadOnlyCollection<Bank> Banks => _banks.AsReadOnly();

    // Private constructor for EF Core
    private Country() { }

    public static Country Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Country code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name is required.", nameof(name));

        return new Country
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim()
        };
    }

    public void UpdateDetails(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Country code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name is required.", nameof(name));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
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
