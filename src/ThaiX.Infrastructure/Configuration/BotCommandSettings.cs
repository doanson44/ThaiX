namespace ThaiX.Infrastructure.Configuration;

public sealed class BotCommandSettings
{
    public const string SectionName = "BotCommands";

    public bool AllowGuidPassthroughUserMapping { get; set; }

    public bool EnableAiNarration { get; set; }

    public int AiNarrationMaxSourceChars { get; set; } = 4000;

    public string AiNarrationSystemPrompt { get; set; } =
        "You are ThaiX market analyst. Ground every conclusion in supplied command results, explain relevant risks and uncertainty, preserve all factual values, and answer naturally in the users language without inventing facts.";

    /// <summary>
    /// When a mention's text doesn't confidently map to any known command, reply with a
    /// general AI chat answer instead of the parser's "Unknown command" error.
    /// </summary>
    public bool EnableConversationalFallback { get; set; } = true;

    public string ConversationalFallbackSystemPrompt { get; set; } =
        "You are ThaiX, a helpful AI assistant for market research and general conversation inside a trading bot. " +
        "Answer the user's actual question directly in the same language they used. " +
        "For market questions, explain the thesis, key risks, relevant uncertainties, and possible scenarios instead of converting the question into a command. " +
        "Do not invent live prices or claim to know the user's portfolio. Distinguish facts from estimates, and avoid presenting financial advice as certainty. " +
        "Be clear and useful; use concise prose or structured bullets when appropriate.";

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
