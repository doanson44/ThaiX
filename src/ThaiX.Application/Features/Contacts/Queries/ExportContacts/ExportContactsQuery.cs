using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Queries.ExportContacts;

/// <summary>
/// Query to export contacts to CSV format.
/// Returns a byte array containing the CSV file content.
/// CSV columns: FirstName,LastName,Email,Phone,Company,JobTitle,Birthday,Notes,Tags
/// </summary>
public sealed record ExportContactsQuery : IAppQuery<ExportContactsResult>
{
    /// <summary>
    /// Optional search term to filter exported contacts.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Filter by archived status. null = all, true = archived only, false = active only.
    /// </summary>
    public bool? IsArchived { get; init; }

    /// <summary>
    /// Filter by tag name.
    /// </summary>
    public string? Tag { get; init; }
}

/// <summary>
/// Result containing the CSV file content.
/// </summary>
public sealed record ExportContactsResult
{
    /// <summary>
    /// CSV file content as bytes (UTF-8 with BOM).
    /// </summary>
    public required byte[] FileContent { get; init; }

    /// <summary>
    /// Suggested file name.
    /// </summary>
    public required string FileName { get; init; }
}
