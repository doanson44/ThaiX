namespace ThaiX.Infrastructure.Configuration;

public sealed class BotCommandSettings
{
    public const string SectionName = "BotCommands";

    public bool AllowGuidPassthroughUserMapping { get; set; }

    public bool EnableAiNarration { get; set; }

    public int AiNarrationMaxSourceChars { get; set; } = 4000;

    public string AiNarrationSystemPrompt { get; set; } =
        "You are ThaiX Slack response rewriter. Keep all factual values exact, keep symbols and numbers exact, keep risk wording accurate, and return concise plain text only.";

    /// <summary>
    /// When a mention's text doesn't confidently map to any known command, reply with a
    /// general AI chat answer instead of the parser's "Unknown command" error.
    /// </summary>
    public bool EnableConversationalFallback { get; set; } = true;

    public string ConversationalFallbackSystemPrompt { get; set; } =
        "You are ThaiX, a helpful assistant living inside a trading/market-data Telegram and Slack bot. " +
        "The user's message did not match any of the bot's price, alert, note, portfolio, or signal commands, " +
        "so reply conversationally and briefly instead — at most a few sentences. " +
        "Reply in the same language the user wrote in. If you don't know something, say so plainly.";

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
