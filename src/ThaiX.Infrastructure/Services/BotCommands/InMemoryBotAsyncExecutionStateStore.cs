using System.Collections.Concurrent;
using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Infrastructure.Services.BotCommands;

public sealed class InMemoryBotAsyncExecutionStateStore : IBotAsyncExecutionStateStore
{
    private readonly ConcurrentDictionary<string, BotAsyncExecutionStatusDto> _store =
        new(StringComparer.OrdinalIgnoreCase);

    public void SetQueued(string executionId)
    {
        _store[executionId] = new BotAsyncExecutionStatusDto
        {
            ExecutionId = executionId,
            Status = "Queued",
            CreatedAtUtc = DateTime.UtcNow,
            CompletedAtUtc = null,
            PlainText = null,
            ErrorCode = null,
            ErrorMessage = null
        };
    }

    public void SetRunning(string executionId)
    {
        _store.AddOrUpdate(
            executionId,
            _ => new BotAsyncExecutionStatusDto
            {
                ExecutionId = executionId,
                Status = "Running",
                CreatedAtUtc = DateTime.UtcNow,
                CompletedAtUtc = null,
                PlainText = null,
                ErrorCode = null,
                ErrorMessage = null
            },
            (_, current) => current with
            {
                Status = "Running",
                ErrorCode = null,
                ErrorMessage = null
            });
    }

    public void SetCompleted(string executionId, string plainText)
    {
        _store.AddOrUpdate(
            executionId,
            _ => new BotAsyncExecutionStatusDto
            {
                ExecutionId = executionId,
                Status = "Completed",
                CreatedAtUtc = DateTime.UtcNow,
                CompletedAtUtc = DateTime.UtcNow,
                PlainText = plainText,
                ErrorCode = null,
                ErrorMessage = null
            },
            (_, current) => current with
            {
                Status = "Completed",
                CompletedAtUtc = DateTime.UtcNow,
                PlainText = plainText,
                ErrorCode = null,
                ErrorMessage = null
            });
    }

    public void SetFailed(string executionId, string errorCode, string errorMessage)
    {
        _store.AddOrUpdate(
            executionId,
            _ => new BotAsyncExecutionStatusDto
            {
                ExecutionId = executionId,
                Status = "Failed",
                CreatedAtUtc = DateTime.UtcNow,
                CompletedAtUtc = DateTime.UtcNow,
                PlainText = null,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            },
            (_, current) => current with
            {
                Status = "Failed",
                CompletedAtUtc = DateTime.UtcNow,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            });
    }

    public BotAsyncExecutionStatusDto? Get(string executionId)
    {
        return _store.TryGetValue(executionId, out var value) ? value : null;
    }
}
