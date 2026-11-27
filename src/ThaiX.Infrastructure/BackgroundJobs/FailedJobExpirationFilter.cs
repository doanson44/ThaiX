using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace ThaiX.Infrastructure.BackgroundJobs;

public sealed class FailedJobExpirationFilter : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
{
    private readonly TimeSpan _retentionPeriod;

    public FailedJobExpirationFilter(TimeSpan retentionPeriod)
    {
        if (retentionPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(retentionPeriod), "Failed job retention must be positive.");
        }

        _retentionPeriod = retentionPeriod;
    }

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState.GetType() != typeof(FailedState))
        {
            return;
        }

        context.CandidateState = ExpiringFailedState.From((FailedState)context.CandidateState);
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is ExpiringFailedState)
        {
            context.JobExpirationTimeout = _retentionPeriod;
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }

    private sealed class ExpiringFailedState : FailedState, IState
    {
        private ExpiringFailedState(FailedState failedState)
            : base(failedState.Exception, failedState.ServerId)
        {
            Reason = failedState.Reason;
            MaxLinesInStackTrace = failedState.MaxLinesInStackTrace;
            IncludeFileInfo = failedState.IncludeFileInfo;
        }

        bool IState.IsFinal => true;

        public static ExpiringFailedState From(FailedState failedState)
        {
            return new ExpiringFailedState(failedState);
        }
    }
}