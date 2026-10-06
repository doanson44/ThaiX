using Microsoft.Extensions.Logging;

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
}