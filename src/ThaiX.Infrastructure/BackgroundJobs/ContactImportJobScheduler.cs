using Hangfire;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Enqueues contact import job processing via Hangfire.
/// </summary>
public sealed class ContactImportJobScheduler(IBackgroundJobClient backgroundJobClient) : IContactImportJobScheduler
{
    public void Enqueue(Guid jobId)
    {
        backgroundJobClient.Enqueue<ContactImportJobProcessor>(x => x.ProcessAsync(jobId, CancellationToken.None));
    }
}

