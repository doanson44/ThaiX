using System.Text;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Resources;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotCommandResponseNarrator : IBotCommandResponseNarrator
{
    private readonly IAiTextGenerationService _aiTextGenerationService;
    private readonly ILogger<BotCommandResponseNarrator> _logger;

    public BotCommandResponseNarrator(
        IAiTextGenerationService aiTextGenerationService,
        ILogger<BotCommandResponseNarrator> logger)
    {
        _aiTextGenerationService = aiTextGenerationService;
        _logger = logger;
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
                _logger.LogWarning(
                    "AI narration returned an empty response. Falling back to structured command output.");
                return result with { PlainText = string.Empty };
            }

            return result with
            {
                PlainText = rewritten
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "AI narration failed with {ExceptionType}. Falling back to structured command output.",
                ex.GetType().Name);
            return result with { PlainText = string.Empty };
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
