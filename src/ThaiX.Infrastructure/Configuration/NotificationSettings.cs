using System.ComponentModel.DataAnnotations;

namespace ThaiX.Infrastructure.Configuration;

public sealed class NotificationSettings
{
    public const string SectionName = "Notifications";

    [Required]
    public Dictionary<string, string> Targets { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
