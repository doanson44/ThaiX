namespace ThaiX.Application.Features.BotCommands.Common;

public interface IBotCommandResponseNarrator
{
    Task<BotCommandExecutionDto> NarrateAsync(
        BotCommandExecutionDto result,
        BotCommandNarrationOptions options,
        CancellationToken cancellationToken);
}

public sealed record BotCommandNarrationOptions
{
    public bool Enabled { get; init; }
    public int MaxSourceChars { get; init; } = 4000;
    public string SystemPrompt { get; init; } = string.Empty;
}