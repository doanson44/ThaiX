using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotCommandResponseNarrator : IBotCommandResponseNarrator
{
    private readonly IAiTextGenerationService _aiTextGenerationService;

    public BotCommandResponseNarrator(IAiTextGenerationService aiTextGenerationService)
    {
        _aiTextGenerationService = aiTextGenerationService;
    }

    public async Task<BotCommandExecutionDto> NarrateAsync(
        BotCommandExecutionDto result,
        BotCommandNarrationOptions options,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled
            || result.Status != BotCommandExecutionStatus.Completed
            || string.IsNullOrWhiteSpace(result.PlainText))
        {
            return result;
        }

        var sourceText = BuildAiSourceText(result, options.MaxSourceChars);
        var prompt =
            "Rewrite the following bot output into one concise, user-friendly Slack reply. " +
            "Keep all numbers, symbols, asset codes, percentages, and risk statements exact. " +
            "Do not add new facts.\n\n" +
            sourceText;

        try
        {
            var aiResult = await _aiTextGenerationService.GenerateAsync(
                prompt,
                options.SystemPrompt,
                cancellationToken);

            var rewritten = aiResult.Text?.Trim();
            if (string.IsNullOrWhiteSpace(rewritten))
            {
                return result;
            }

            return result with
            {
                PlainText = rewritten
            };
        }
        catch
        {
            return result;
        }
    }

    private static string BuildAiSourceText(BotCommandExecutionDto result, int maxChars)
    {
        var builder = new StringBuilder();
        builder.Append($"Status: {result.Status}\n");
        builder.Append("PlainText:\n");
        builder.Append(result.PlainText);

        var composed = builder.ToString().Trim();
        if (composed.Length <= maxChars || maxChars <= 0)
        {
            return composed;
        }

        return composed[..maxChars];
    }
}