using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.MasterData;

/// <summary>
/// Represents a district/ward within a city.
/// </summary>
public sealed class District : BaseAuditableEntity
{
    /// <summary>
    /// Unique district code.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable district name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Code of the city this district belongs to.
    /// </summary>
    public string CityCode { get; private set; } = string.Empty;

    /// <summary>
    /// Foreign key to the parent City entity.
    /// </summary>
    public Guid CityId { get; private set; }

    /// <summary>
    /// Navigation property to the parent City.
    /// </summary>
    public City City { get; private set; } = null!;

    // Private constructor for EF Core
    private District() { }

    public static District Create(string code, string name, string cityCode, Guid cityId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("District code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("District name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(cityCode))
            throw new ArgumentException("City code is required.", nameof(cityCode));

        if (cityId == Guid.Empty)
            throw new ArgumentException("City ID is required.", nameof(cityId));

        return new District
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CityCode = cityCode.Trim().ToUpperInvariant(),
            CityId = cityId
        };
    }

    public void UpdateDetails(string code, string name, string cityCode, Guid cityId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("District code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("District name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(cityCode))
            throw new ArgumentException("City code is required.", nameof(cityCode));

        if (cityId == Guid.Empty)
            throw new ArgumentException("City ID is required.", nameof(cityId));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        CityCode = cityCode.Trim().ToUpperInvariant();
        CityId = cityId;
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
