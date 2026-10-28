using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Resources;

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

        var sourceText = BuildAiSourceText(result, options.MaxSourceChars, options.UserMessage);
        var prompt = BotCommandAiPromptResources.Format("Narrator.Prompt", sourceText);

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

    private static string BuildAiSourceText(BotCommandExecutionDto result, int maxChars, string userMessage)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(userMessage))
        {
            builder.Append(BotCommandAiPromptResources.Format("Narrator.UserRequest", userMessage.Trim()));
        }

        builder.Append(BotCommandAiPromptResources.Format("Narrator.Status", result.Status));
        builder.Append(BotCommandAiPromptResources.Get("Narrator.CommandResult"));
        builder.Append(result.PlainText);

        if (result.Card is not null)
        {
            builder.Append(BotCommandAiPromptResources.Get("Narrator.StructuredResult"));
            builder.Append(BotCommandAiPromptResources.Format("Narrator.Category", result.Card.Category));
            builder.Append(BotCommandAiPromptResources.Format("Narrator.Title", result.Card.Title));
            foreach (var metric in result.Card.Metrics)
            {
                builder.Append(BotCommandAiPromptResources.Format("Narrator.Metric", metric.Label, metric.Value));
            }
        }

        var composed = builder.ToString().Trim();
        if (composed.Length <= maxChars || maxChars <= 0)
        {
            return composed;
        }

        return composed[..maxChars];
    }
}
