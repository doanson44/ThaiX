using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;

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
        var systemPrompt =
            "You are a command intent resolver for ThaiX bot. " +
            "Choose at most one command from the supplied catalog that best fulfills the user's current request. " +
            "Return ONLY one line with command text, starting with '/' and arguments if needed. " +
            "If the request is not supported or required arguments cannot be resolved confidently, return exactly: NONE.\n\n" +
            "The input may include recent conversation history and a Current message section. Treat the Current message as the request; use history only to resolve references or omitted arguments, and never repeat an earlier action unless the current message asks for it. " +
            "Use command syntax and descriptions as the source of truth. Do not rely on hardcoded ticker lists or assume an asset class from a symbol alone. Prefer a command with automatic asset detection when the asset type is unclear. " +
            "For market analysis requests, choose the relevant data-retrieval command available in the catalog; the response will be analyzed separately using the command's actual result.";

        var prompt = $"Command catalog:\n{catalog}\n\nUser message and available context:\n{mentionText.Trim()}\n\nReturn command:";

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

            builder.AppendLine($"/{metadata.Name} | syntax: {metadata.Syntax} | aliases: {aliases} | description: {metadata.Description}");
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