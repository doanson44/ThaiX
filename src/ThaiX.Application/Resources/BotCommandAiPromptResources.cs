using System.Globalization;
using System.Resources;

namespace ThaiX.Application.Resources;

public static class BotCommandAiPromptResources
{
    private static readonly ResourceManager ResourceManager = new(
        "ThaiX.Application.Resources.BotCommandAiPrompts",
        typeof(BotCommandAiPromptResources).Assembly);

    public static string Get(string key) =>
        ResourceManager.GetString(key, CultureInfo.InvariantCulture)
        ?? throw new MissingManifestResourceException($"AI prompt resource '{key}' was not found.");

    public static string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.InvariantCulture, Get(key), arguments);
}
