namespace ThaiX.Infrastructure.Configuration;

/// <summary>
/// File storage configuration.
/// </summary>
public sealed class StorageSettings
{
    public const string SectionName = "Storage";

    public string RootPath { get; init; } = "storage";

    public string BaseUrl { get; init; } = "/files";

    public int MaxFileSizeMB { get; init; } = 10;

    public IReadOnlyList<string> AllowedMimeTypes { get; init; } = [];
}