namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Schedules contact import jobs to run in the background (e.g. Hangfire).
/// Implemented in Infrastructure.
/// </summary>
public interface IContactImportJobScheduler
{
    /// <summary>
    /// Enqueues processing of the contact import job with the given id.
    /// </summary>
    void Enqueue(Guid jobId);
}
