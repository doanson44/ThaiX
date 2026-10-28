using System.Globalization;
using System.Resources;

namespace ThaiX.Presentation.Resources;

public static class AiPromptResources
{
    private static readonly ResourceManager ResourceManager = new(
        "ThaiX.Presentation.Resources.AiPrompts",
        typeof(AiPromptResources).Assembly);

    public static string Get(string key) =>
        ResourceManager.GetString(key, CultureInfo.InvariantCulture)
        ?? throw new MissingManifestResourceException($"AI prompt resource '{key}' was not found.");

    public static string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.InvariantCulture, Get(key), arguments);
}
