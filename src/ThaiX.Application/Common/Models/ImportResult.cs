namespace ThaiX.Application.Common.Models;

/// <summary>
/// Result of a CSV import operation.
/// </summary>
public sealed record ImportResult
{
    /// <summary>
    /// Total number of rows parsed from the CSV file.
    /// </summary>
    public int TotalRows { get; init; }

    /// <summary>
    /// Number of rows successfully imported (inserted).
    /// </summary>
    public int InsertedCount { get; init; }

    /// <summary>
    /// Number of rows updated (existing records matched by code).
    /// </summary>
    public int UpdatedCount { get; init; }

    /// <summary>
    /// Number of rows skipped due to errors.
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// Detailed error information for each skipped row.
    /// </summary>
    public required IReadOnlyList<ImportRowError> Errors { get; init; }
}

/// <summary>
/// Describes an error for a specific row during import.
/// </summary>
public sealed record ImportRowError
{
    /// <summary>
    /// 1-based row number in the CSV file (excluding header).
    /// </summary>
    public int RowNumber { get; init; }

    /// <summary>
    /// The raw field values from the row, for context.
    /// </summary>
    public string? RawData { get; init; }

    /// <summary>
    /// Error message explaining why this row was skipped.
    /// </summary>
    public required string Error { get; init; }
}
