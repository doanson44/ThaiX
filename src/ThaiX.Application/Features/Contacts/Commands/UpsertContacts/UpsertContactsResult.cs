using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Contacts.Commands.UpsertContacts;

/// <summary>
/// Result of processing one batch of contact import rows.
/// </summary>
public sealed record UpsertContactsResult
{
    public int InsertedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int SkippedCount { get; init; }
    public required IReadOnlyList<ImportRowError> Errors { get; init; }
}
