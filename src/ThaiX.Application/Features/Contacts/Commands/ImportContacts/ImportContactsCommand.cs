using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Contacts.Commands.ImportContacts;

/// <summary>
/// Command to import contacts from a CSV file.
/// Only the standard template format is accepted (see ContactImportCsvSpec). Users must convert their CSV
/// to the canonical columns (FirstName, LastName, Email1, Phone1, etc.) before importing. Use GET /api/contacts/import/template to download the template.
/// Upsert by normalized phone (primary), then email. Batch size 200-5000 (default 500).
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record ImportContactsCommand : IAppCommand<ImportResult>
{
    /// <summary>
    /// CSV file stream.
    /// </summary>
    public required Stream CsvStream { get; init; }

    /// <summary>
    /// Batch size for processing (default 500, max 5000). Optional.
    /// </summary>
    public int? BatchSize { get; init; }
}
