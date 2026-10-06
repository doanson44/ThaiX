using ThaiX.Application.Common.Models;
using ThaiX.Application.Trading;

namespace ThaiX.Application.Features.BotCommands.Common;

public enum BotCommandCategory
{
    MarketData = 1,
    TechnicalAnalysis = 2,
    TradingSignals = 3,
    Portfolio = 4,
    Alerts = 5,
    System = 6,
    Administration = 7
}

public enum BotCommandExecutionStatus
{
    Completed = 1,
    Queued = 2,
    Rejected = 3,
    Failed = 4
}

public sealed record BotCommandExecutionDto
{
    public required BotCommandExecutionStatus Status { get; init; }
    public required string PlainText { get; init; }
    public string? ExecutionId { get; init; }
    public string? ErrorCode { get; init; }
    public NotificationCard? Card { get; init; }
}

public sealed record BotCommandMetadata
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> Aliases { get; init; }
    public required string Syntax { get; init; }
    public required string Description { get; init; }
    public required BotCommandCategory Category { get; init; }
    public required IReadOnlyList<string> RequiredPermissions { get; init; }
    public bool SupportsAsync { get; init; }
}

public sealed record BotParsedCommand
{
    public required string Name { get; init; }
    public required string RawText { get; init; }
    public required IReadOnlyList<string> Arguments { get; init; }
    public required IReadOnlySet<string> Flags { get; init; }

    public bool HasFlag(string flag)
    {
        if (string.IsNullOrWhiteSpace(flag))
        {
            return false;
        }

        var normalized = flag.Trim().ToLowerInvariant();
        return Flags.Contains(normalized);
    }
}

public sealed record BotParseResult
{
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public BotParsedCommand? Command { get; init; }

    public static BotParseResult ParseFailed(string errorMessage)
    {
        return new BotParseResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Command = null
        };
    }

    public static BotParseResult ParseSucceeded(BotParsedCommand command)
    {
        return new BotParseResult
        {
            Success = true,
            ErrorMessage = null,
            Command = command
        };
    }
}

public sealed record BotCommandContext
{
    public required string Channel { get; init; }
    public required string ExternalChannelId { get; init; }
    public required BotParsedCommand ParsedCommand { get; init; }
}

public sealed record BotMappedUser
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyCollection<string> Permissions { get; init; }
}

public sealed record TradeSuggestionAsyncRequest
{
    public required string Symbol { get; init; }
    public required MarketType MarketType { get; init; }
    public required string Timeframe { get; init; }
    public bool UseLongTermTimeframe { get; init; }
    public MarketRegime MarketRegime { get; init; } = MarketRegime.RiskOn;
    public EventRisk EventRisk { get; init; } = EventRisk.Low;
}

public sealed record BotAsyncExecutionStatusDto
{
    public required string ExecutionId { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string? PlainText { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}
