namespace ThaiX.Client.Models.MasterData;

/// <summary>
/// DTO for country list item (matches API response).
/// </summary>
public sealed record CountryListItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string DisplayText { get; init; }
}

/// <summary>
/// DTO for city list item (matches API response).
/// </summary>
public sealed record CityListItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
    public required string CountryName { get; init; }
    public required string DisplayText { get; init; }
}

/// <summary>
/// DTO for district list item (matches API response).
/// </summary>
public sealed record DistrictListItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CityCode { get; init; }
    public required string CityName { get; init; }
    public required string DisplayText { get; init; }
}

/// <summary>
/// DTO for bank list item (matches API response).
/// </summary>
public sealed record BankListItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
    public required string CountryName { get; init; }
    public required string DisplayText { get; init; }
}

/// <summary>
/// Query parameters for root-level master data list (countries).
/// </summary>
public record MasterDataListRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortBy { get; init; } = "name";
    public bool SortDescending { get; init; }
    public string? SearchTerm { get; init; }
}

/// <summary>
/// Query parameters for child master data list (cities, districts, banks) with optional parent filter.
/// </summary>
public sealed record MasterDataListWithParentRequest : MasterDataListRequest
{
    /// <summary>
    /// Parent code filter: CountryCode for cities/banks, CityCode for districts.
    /// </summary>
    public string? ParentCode { get; init; }
}

/// <summary>
/// Request to create or update a root-level master entity (code + name).
/// </summary>
public sealed record UpdateMasterDataRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

/// <summary>
/// Request to create or update a child master entity (code + name + parent code).
/// </summary>
public sealed record UpdateChildMasterDataRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string ParentCode { get; init; }
}

/// <summary>
/// Request to create a city (API expects CountryCode).
/// </summary>
public sealed record CreateCityRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
}

/// <summary>
/// Request to create a district (API expects CityCode).
/// </summary>
public sealed record CreateDistrictRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CityCode { get; init; }
}

/// <summary>
/// Request to create a bank (API expects CountryCode).
/// </summary>
public sealed record CreateBankRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
}

/// <summary>
/// Result of a CSV import operation (matches API response).
/// </summary>
public sealed record ImportResultDto
{
    public int TotalRows { get; init; }
    public int InsertedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int SkippedCount { get; init; }
    public required IReadOnlyList<ImportRowErrorDto> Errors { get; init; }
}

/// <summary>
/// Error for a single row during import.
/// </summary>
public sealed record ImportRowErrorDto
{
    public int RowNumber { get; init; }
    public string? RawData { get; init; }
    public required string Error { get; init; }
}

/// <summary>
/// DTO for a single symbol search result.
/// </summary>
public sealed record SymbolSearchItemDto
{
    public required string Symbol { get; init; }
    public required string DisplayText { get; init; }
}

/// <summary>
/// DTO for symbol search paginated result.
/// </summary>
public sealed record SymbolSearchResultDto
{
    public required IReadOnlyList<SymbolSearchItemDto> Items { get; init; }
    public int TotalCount { get; init; }
}
