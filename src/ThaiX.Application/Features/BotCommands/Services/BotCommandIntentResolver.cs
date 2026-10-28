using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Resources;

namespace ThaiX.Application.Features.BotCommands.Services;

public sealed class BotCommandIntentResolver : IBotCommandIntentResolver
{
    private readonly IAiTextGenerationService _aiTextGenerationService;
    private readonly IBotCommandRegistry _registry;
    private readonly IBotCommandParser _parser;

    public BotCommandIntentResolver(
        IAiTextGenerationService aiTextGenerationService,
        IBotCommandRegistry registry,
        IBotCommandParser parser)
    {
        _aiTextGenerationService = aiTextGenerationService;
        _registry = registry;
        _parser = parser;
    }

    public async Task<string?> ResolveCommandTextAsync(string mentionText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mentionText))
        {
            return null;
        }

        var catalog = BuildCommandCatalog();
        var systemPrompt = BotCommandAiPromptResources.Get("Intent.System");
        var prompt = BotCommandAiPromptResources.Format("Intent.User", catalog, mentionText.Trim());

        var aiResult = await _aiTextGenerationService.GenerateAsync(prompt, systemPrompt, cancellationToken);
        var candidate = NormalizeCandidate(aiResult.Text);

        if (string.IsNullOrWhiteSpace(candidate)
            || string.Equals(candidate, "NONE", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var validated = ValidateCommandCandidate(candidate);
        if (!string.IsNullOrWhiteSpace(validated))
        {
            return validated;
        }

        if (!candidate.StartsWith("/", StringComparison.Ordinal))
        {
            validated = ValidateCommandCandidate('/' + candidate);
            if (!string.IsNullOrWhiteSpace(validated))
            {
                return validated;
            }
        }

        return null;
    }

    private string BuildCommandCatalog()
    {
        var modules = _registry.GetAll()
            .Select(x => x.Metadata)
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var builder = new StringBuilder();
        foreach (var metadata in modules)
        {
            var aliases = metadata.Aliases.Count == 0
                ? "-"
                : string.Join(", ", metadata.Aliases);

            builder.AppendLine(BotCommandAiPromptResources.Format("Intent.CatalogItem", metadata.Name, metadata.Syntax, aliases, metadata.Description));
        }

        return builder.ToString().Trim();
    }

    private string? ValidateCommandCandidate(string candidate)
    {
        var parse = _parser.Parse(candidate.Trim());
        if (!parse.Success || parse.Command is null)
        {
            return null;
        }

        return _registry.TryResolve(parse.Command.Name, out _)
            ? candidate.Trim()
            : null;
    }

    private static string NormalizeCandidate(string aiText)
    {
        if (string.IsNullOrWhiteSpace(aiText))
        {
            return string.Empty;
        }

        var line = aiText
            .Replace("```", string.Empty, StringComparison.Ordinal)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        return line?.Trim() ?? string.Empty;
    }
}
