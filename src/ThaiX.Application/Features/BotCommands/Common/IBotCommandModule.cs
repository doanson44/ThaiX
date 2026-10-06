namespace ThaiX.Application.Features.BotCommands.Common;

public interface IBotCommandModule
{
    BotCommandMetadata Metadata { get; }

    Task<BotCommandExecutionDto> ExecuteAsync(BotCommandContext context, CancellationToken cancellationToken);
}
