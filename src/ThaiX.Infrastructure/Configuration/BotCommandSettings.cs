using System.Resources;

namespace ThaiX.Infrastructure.Configuration;

public sealed class BotCommandSettings
{
    private static readonly ResourceManager PromptResources = new(
        "ThaiX.Infrastructure.Resources.BotCommandPrompts",
        typeof(BotCommandSettings).Assembly);

    private static string GetRequiredPrompt(string name) =>
        PromptResources.GetString(name)
        ?? throw new InvalidOperationException($"Missing bot command prompt resource '{name}'.");
    public const string SectionName = "BotCommands";

    public bool AllowGuidPassthroughUserMapping { get; set; }

    public bool EnableAiNarration { get; set; }

    public int AiNarrationMaxSourceChars { get; set; } = 4000;

    public string AiNarrationSystemPrompt { get; set; } = GetRequiredPrompt(nameof(AiNarrationSystemPrompt));

    /// <summary>
    /// When a mention's text doesn't confidently map to any known command, reply with a
    /// general AI chat answer instead of the parser's "Unknown command" error.
    /// </summary>
    public bool EnableConversationalFallback { get; set; } = true;

    public string ConversationalFallbackSystemPrompt { get; set; } = GetRequiredPrompt(nameof(ConversationalFallbackSystemPrompt));

    private Dictionary<string, Dictionary<string, string>> _userMappings =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Dictionary<string, string>> UserMappings
    {
        get => _userMappings;
        set
        {
            if (value is null)
            {
                _userMappings = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            var clone = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (platform, map) in value)
            {
                clone[platform] = map is null
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase);
            }

            _userMappings = clone;
        }
    }
}
