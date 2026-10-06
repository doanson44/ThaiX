namespace ThaiX.Application.Features.BotCommands.Common;

public interface IBotCommandIntentResolver
{
    Task<string?> ResolveCommandTextAsync(string mentionText, CancellationToken cancellationToken);
}