using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

public sealed class FakeBackgroundJobClient : IBackgroundJobClient
{
    public string Create(Job job, IState state)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(state);

        return Guid.NewGuid().ToString("N");
    }

    public bool ChangeState(string jobId, IState state, string expectedState)
    {
        ArgumentNullException.ThrowIfNull(jobId);
        ArgumentNullException.ThrowIfNull(state);

        return true;
    }
}
