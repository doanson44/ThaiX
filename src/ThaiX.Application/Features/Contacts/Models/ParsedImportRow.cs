namespace ThaiX.Application.Features.Contacts.Models;

/// <summary>
/// A parsed CSV row with its 1-based row number for error reporting.
/// </summary>
public sealed record ParsedImportRow
{
    public int RowNumber { get; init; }
    public required ContactImportRow Data { get; init; }
}
