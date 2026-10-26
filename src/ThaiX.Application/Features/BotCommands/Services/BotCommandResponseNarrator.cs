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

        var sourceText = BuildAiSourceText(result, options.MaxSourceChars, options.UserMessage);
        var prompt =
            "Answer the user's actual request as a knowledgeable market analyst in a natural, conversational style. " +
            "Use only facts supported by the command result. Explain what the data suggests, its limitations, and relevant risks or scenarios when supported. " +
            "Keep prices, numbers, symbols, percentages, and source facts exact. Do not invent indicators, news, historical data, or certainty. " +
            "Clearly distinguish observed facts from interpretation, and respond in the same language as the user's request. " +
            "Do not merely reformat the command output; interpret it where the supplied evidence allows.\n\n" +
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

    private static string BuildAiSourceText(BotCommandExecutionDto result, int maxChars, string userMessage)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(userMessage))
        {
            builder.Append("User request:\n");
            builder.Append(userMessage.Trim());
            builder.Append("\n\n");
        }

        builder.Append($"Status: {result.Status}\n");
        builder.Append("Command result:\n");
        builder.Append(result.PlainText);

        if (result.Card is not null)
        {
            builder.Append("\n\nStructured result:\n");
            builder.Append($"Category: {result.Card.Category}\n");
            builder.Append($"Title: {result.Card.Title}\n");
            foreach (var metric in result.Card.Metrics)
            {
                builder.Append($"{metric.Label}: {metric.Value}\n");
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