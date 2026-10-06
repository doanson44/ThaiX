using System.ComponentModel.DataAnnotations;

namespace ThaiX.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the Gmail SMTP email sender.
/// Bound from the "Email" section in appsettings.json.
/// </summary>
public sealed class EmailSettings
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Email";

    [Required]
    public string SmtpHost { get; set; } = "smtp.gmail.com";

    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;

    [Required]
    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "ThaiX";

    [Required]
    public string AppPassword { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;
}
