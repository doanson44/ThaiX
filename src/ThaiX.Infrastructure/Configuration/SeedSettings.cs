using System.ComponentModel.DataAnnotations;

namespace ThaiX.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the initial admin user seeded on startup.
/// Bound from the "Seed" section in appsettings.json.
/// </summary>
public sealed class SeedSettings
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Seed";

    [Required]
    [EmailAddress]
    public string AdminEmail { get; set; } = "admin@app.example.invalid";

    [Required]
    public string AdminUsername { get; set; } = "admin";

    [Required]
    public string AdminPassword { get; set; } = "REPLACE_WITH_ADMIN_PASSWORD";
}
