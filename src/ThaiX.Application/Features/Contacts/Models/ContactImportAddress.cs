namespace ThaiX.Application.Features.Contacts.Models;

/// <summary>
/// Address data for contact import. Not a domain entity.
/// </summary>
public sealed record ContactImportAddress
{
    public required string Street { get; init; }
    public required string CountryCode { get; init; }
    public string CityCode { get; init; } = string.Empty;
    public string DistrictCode { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}
