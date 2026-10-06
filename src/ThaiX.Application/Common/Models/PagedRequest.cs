namespace ThaiX.Application.Common.Models;

/// <summary>
/// Base model for paginated requests.
/// Provides standardized paging parameters across all query handlers.
/// </summary>
public record PagedRequest
{
    /// <summary>
    /// Page number (1-based index).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Optional field name to sort by.
    /// Should match property names in the entity/DTO.
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Sort direction (true = descending, false = ascending).
    /// </summary>
    public bool SortDescending { get; init; }

    /// <summary>
    /// Minimum allowed page size.
    /// </summary>
    public const int MinPageSize = 1;

    /// <summary>
    /// Maximum allowed page size to prevent abuse.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Magic value for PageSize that fetches all records without pagination.
    /// Pass -1 as pageSize to retrieve the complete result set.
    /// </summary>
    public const int FetchAll = -1;

    /// <summary>
    /// Gets validated page number (ensures >= 1).
    /// </summary>
    public int ValidatedPageNumber => Math.Max(1, PageNumber);

    /// <summary>
    /// Gets validated page size (clamped between min and max).
    /// </summary>
    public int ValidatedPageSize => Math.Clamp(PageSize, MinPageSize, MaxPageSize);

    /// <summary>
    /// Calculates skip count for SQL query.
    /// </summary>
    public int CalculateSkip() => (ValidatedPageNumber - 1) * ValidatedPageSize;
}
