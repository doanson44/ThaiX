namespace ThaiX.Client.Models.Slack;

public sealed class ExecuteSlackBotCommandRequest
{
    public string? RawText { get; set; }
    public string? Command { get; set; }
    public string? Text { get; set; }
    public string ExternalUserId { get; set; } = string.Empty;
    public string ExternalChannelId { get; set; } = string.Empty;
}

public sealed class BotCommandExecutionDto
{
    public string Status { get; init; } = string.Empty;
    public string PlainText { get; init; } = string.Empty;
    public string? ExecutionId { get; init; }
    public string? ErrorCode { get; init; }
}

public sealed class BotAsyncExecutionStatusDto
{
    public string ExecutionId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string? PlainText { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}