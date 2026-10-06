namespace ThaiX.Application.Features.BotCommands.Common;

public interface IBotAsyncExecutionService
{
    Task<string> EnqueueTradeSuggestionAsync(TradeSuggestionAsyncRequest request, CancellationToken cancellationToken);

    Task<BotAsyncExecutionStatusDto?> GetStatusAsync(string executionId, CancellationToken cancellationToken);
}
