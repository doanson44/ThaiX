namespace ThaiX.Application.Features.BotCommands.Common;

/// <summary>
/// General AI chat fallback for mentions that don't map to any known bot command
/// (e.g. "@ThaiXGroupBot làm thơ"), so the bot answers instead of reporting "Unknown command".
/// </summary>
public interface IBotConversationalResponder
{
    Task<string?> RespondAsync(string message, string systemPrompt, CancellationToken cancellationToken);
}
