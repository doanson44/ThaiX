using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Cities.Queries.GetCities;

/// <summary>
/// Query to retrieve a paginated list of cities.
/// </summary>
public sealed record GetCitiesQuery : PagedRequest, IAppQuery<PagedResult<CityListItemDto>>
{
    /// <summary>
    /// Optional search term to filter by code or name.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Optional filter by country code.
    /// </summary>
    public string? CountryCode { get; init; }
}

/// <summary>
/// DTO for city list item.
/// </summary>
public sealed record CityListItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
    public required string CountryName { get; init; }
    /// <summary>Display text for dropdown: [Code] Name when Code is present, otherwise Name.</summary>
    public required string DisplayText { get; init; }
}
