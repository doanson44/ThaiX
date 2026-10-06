using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotConversationalResponder : IBotConversationalResponder
{
    private readonly IAiTextGenerationService _aiTextGenerationService;

    public BotConversationalResponder(IAiTextGenerationService aiTextGenerationService)
    {
        _aiTextGenerationService = aiTextGenerationService;
    }

    public async Task<string?> RespondAsync(string message, string systemPrompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        try
        {
            var result = await _aiTextGenerationService.GenerateAsync(message.Trim(), systemPrompt, cancellationToken);
            return string.IsNullOrWhiteSpace(result.Text) ? null : result.Text.Trim();
        }
        catch
        {
            return null;
        }
    }
}
