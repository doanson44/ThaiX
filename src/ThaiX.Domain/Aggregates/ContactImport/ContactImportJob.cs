namespace ThaiX.Domain.Aggregates.ContactImport;

/// <summary>
/// Tracks an asynchronous contact import job (file path, status, counts).
/// Used by the bulk import endpoint and Hangfire worker.
/// </summary>
public sealed class ContactImportJob
{
    public Guid Id { get; private set; }
    public ContactImportJobStatus Status { get; private set; }
    /// <summary>Full path to the uploaded CSV file (temp or blob path).</summary>
    public string FilePath { get; private set; } = string.Empty;
    public int BatchSize { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public int TotalRows { get; private set; }
    public int InsertedCount { get; private set; }
    public int UpdatedCount { get; private set; }
    public int SkippedCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ContactImportJob() { }

    public static ContactImportJob Create(string filePath, int batchSize)
    {
        return new ContactImportJob
        {
            Id = Guid.NewGuid(),
            Status = ContactImportJobStatus.Pending,
            FilePath = filePath,
            BatchSize = batchSize,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkRunning()
    {
        Status = ContactImportJobStatus.Running;
    }

    public void MarkCompleted(int totalRows, int insertedCount, int updatedCount, int skippedCount)
    {
        Status = ContactImportJobStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        TotalRows = totalRows;
        InsertedCount = insertedCount;
        UpdatedCount = updatedCount;
        SkippedCount = skippedCount;
    }

    public void MarkFailed(string? errorMessage)
    {
        Status = ContactImportJobStatus.Failed;
        CompletedAtUtc = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}
