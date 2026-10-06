using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Infrastructure.Services.BotCommands;

public interface IBotAsyncExecutionStateStore
{
    void SetQueued(string executionId);
    void SetRunning(string executionId);
    void SetCompleted(string executionId, string plainText);
    void SetFailed(string executionId, string errorCode, string errorMessage);
    BotAsyncExecutionStatusDto? Get(string executionId);
}
