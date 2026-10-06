using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Districts.Queries.GetDistricts;

/// <summary>
/// Query to retrieve a paginated list of districts.
/// </summary>
public sealed record GetDistrictsQuery : PagedRequest, IAppQuery<PagedResult<DistrictListItemDto>>
{
    /// <summary>
    /// Optional search term to filter by code or name.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Optional filter by city code.
    /// </summary>
    public string? CityCode { get; init; }
}

/// <summary>
/// DTO for district list item.
/// </summary>
public sealed record DistrictListItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CityCode { get; init; }
    public required string CityName { get; init; }
    /// <summary>Display text for dropdown: [Code] Name when Code is present, otherwise Name.</summary>
    public required string DisplayText { get; init; }
}
