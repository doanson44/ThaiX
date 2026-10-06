using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Countries.Queries.GetCountries;

/// <summary>
/// Query to retrieve a paginated list of countries.
/// </summary>
public sealed record GetCountriesQuery : PagedRequest, IAppQuery<PagedResult<CountryListItemDto>>
{
    /// <summary>
    /// Optional search term to filter by code or name.
    /// </summary>
    public string? SearchTerm { get; init; }
}

/// <summary>
/// DTO for country list item.
/// </summary>
public sealed record CountryListItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    /// <summary>Display text for dropdown: [Code] Name when Code is present, otherwise Name.</summary>
    public required string DisplayText { get; init; }
}
