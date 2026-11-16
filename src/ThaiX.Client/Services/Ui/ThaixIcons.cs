namespace ThaiX.Client.Services.Ui;

public static class ThaixIcons
{
    public const string SpritePath = "/icons/thaix.svg";

    private static readonly IReadOnlyDictionary<string, string> RadzenAliases =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["alert"] = "info",
            ["ai-chip"] = "memory",
            ["bell"] = "notifications",
            ["calendar"] = "calendar_month",
            ["cube"] = "category",
            ["document"] = "description",
            ["gamepad"] = "sports_esports",
            ["globe"] = "public",
            ["lightning"] = "bolt",
            ["server"] = "dns",
            ["shield-check"] = "verified_user",
            ["team"] = "groups",
            ["user"] = "person",
            ["wallet"] = "account_balance_wallet"
        };

    public static string Resolve(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "info";

        return RadzenAliases.TryGetValue(name, out var icon) ? icon : name;
    }

    public static bool Exists(string? name) =>
        !string.IsNullOrWhiteSpace(name);
}
