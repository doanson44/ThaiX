using Hangfire;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Infrastructure.BackgroundJobs;

namespace ThaiX.Infrastructure.Services.BotCommands;

public sealed class HangfireBotAsyncExecutionService : IBotAsyncExecutionService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IBotAsyncExecutionStateStore _stateStore;

    public HangfireBotAsyncExecutionService(
        IBackgroundJobClient jobClient,
        IBotAsyncExecutionStateStore stateStore)
    {
        _jobClient = jobClient;
        _stateStore = stateStore;
    }

    public Task<string> EnqueueTradeSuggestionAsync(
        TradeSuggestionAsyncRequest request,
        CancellationToken cancellationToken)
    {
        var executionId = Guid.NewGuid().ToString("N");
        _stateStore.SetQueued(executionId);

        _jobClient.Enqueue<BotTradeSuggestionAsyncJob>(
            job => job.RunAsync(executionId, request, CancellationToken.None));

        return Task.FromResult(executionId);
    }

    public Task<BotAsyncExecutionStatusDto?> GetStatusAsync(string executionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_stateStore.Get(executionId));
    }
}
