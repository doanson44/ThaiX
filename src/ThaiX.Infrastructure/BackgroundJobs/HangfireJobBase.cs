using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Helpers;

namespace ThaiX.Infrastructure.BackgroundJobs;

public abstract class HangfireJobBase
{
    private readonly IHangfireJobState _jobState;
    private readonly ILogger _logger;

    protected HangfireJobBase(
        IHangfireJobState jobState,
        ILogger logger)
    {
        _jobState = jobState;
        _logger = logger;
    }

    protected bool CanExecute(string jobId)
    {
        if (_jobState.IsEnabled(jobId))
            return true;

        _logger.LogInformation(
            "Hangfire job '{JobId}' is disabled. Skipping execution.",
            jobId);

        return false;
    }

    /// <summary>
    /// Serializes executions of the same job inside this process. Replaces Hangfire's
    /// DisableConcurrentExecution filter, whose distributed lock does not release reliably with MySqlStorage.
    /// </summary>
    protected static Task<IDisposable> AcquireJobLockAsync(string lockKey, CancellationToken cancellationToken)
        => AsyncLock.GetLockByKey(lockKey).LockAsync(cancellationToken);
}