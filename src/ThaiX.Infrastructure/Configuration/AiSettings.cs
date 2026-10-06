using System.ComponentModel.DataAnnotations;

namespace ThaiX.Infrastructure.Configuration;

public sealed class AiSettings
{
    public const string SectionName = "Ai";

    [Required]
    public string PreferredProvider { get; set; } = "Gemini";

    [Range(5, 180)]
    public int RequestTimeoutSeconds { get; set; } = 45;

    public List<string> ProviderOrder { get; set; } = [];

    [Required]
    public Dictionary<string, AiProviderSettings> Providers { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed class AiProviderSettings
{
    public bool Enabled { get; set; }

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DailyQuota { get; set; } = 1;
}