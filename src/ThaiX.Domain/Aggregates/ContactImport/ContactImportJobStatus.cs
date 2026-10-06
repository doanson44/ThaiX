namespace ThaiX.Domain.Aggregates.ContactImport;

/// <summary>
/// Status of an asynchronous contact import job.
/// </summary>
public enum ContactImportJobStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}
