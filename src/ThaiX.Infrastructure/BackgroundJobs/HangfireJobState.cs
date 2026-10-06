using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.BackgroundJobs;

public sealed class HangfireJobState : IHangfireJobState
{
    private readonly HangfireConfiguration _options;

    public HangfireJobState(IOptions<HangfireConfiguration> options)
    {
        _options = options.Value;
    }

    public bool IsEnabled(string jobId)
    {
        return _options.RecurringJobs.TryGetValue(jobId, out var job)
            && job.Enabled;
    }
}
