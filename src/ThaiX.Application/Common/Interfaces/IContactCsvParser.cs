namespace ThaiX.Application.Common.Interfaces;

using ThaiX.Application.Features.Contacts.Models;

/// <summary>
/// Parses a CSV stream into contact import rows. No database or domain logic.
/// Supports simple and Google/Apple contact export column names.
/// </summary>
public interface IContactCsvParser
{
    /// <summary>
    /// Parses the CSV stream and yields normalized rows with row numbers. Memory-safe streaming.
    /// </summary>
    IAsyncEnumerable<ParsedImportRow> ParseAsync(Stream csvStream, CancellationToken cancellationToken = default);
}
