using ThaiX.Domain.Aggregates.ContactImport;

namespace ThaiX.Application.Features.Contacts.Queries.GetContactImportJob;

/// <summary>
/// DTO for contact import job status.
/// </summary>
public sealed record ContactImportJobDto
{
    public Guid Id { get; init; }
    public ContactImportJobStatus Status { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public int TotalRows { get; init; }
    public int InsertedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int SkippedCount { get; init; }
    public string? ErrorMessage { get; init; }
}
