using System.ComponentModel.DataAnnotations;

namespace ThaiX.Infrastructure.Configuration;

public sealed class SlackSettings
{
    public const string SectionName = "Slack";

    [Required]
    public string BotToken { get; set; } = string.Empty;

    [Required]
    public Dictionary<string, string> Routing { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, Dictionary<string, string>> _channels = new(StringComparer.OrdinalIgnoreCase);

    [Required]
    public Dictionary<string, Dictionary<string, string>> Channels
    {
        get => _channels;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var channels = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in value)
            {
                channels[kv.Key] = kv.Value is null
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(kv.Value, StringComparer.OrdinalIgnoreCase);
            }

            _channels = channels;
        }
    }
}
