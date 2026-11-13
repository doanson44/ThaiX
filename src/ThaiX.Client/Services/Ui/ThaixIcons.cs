namespace ThaiX.Client.Services.Ui;

public static class ThaixIcons
{
    public const string SpritePath = "/icons/thaix.svg";

    public static string Resolve(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "alert";

        return name;
    }

    public static bool Exists(string? name) =>
        !string.IsNullOrWhiteSpace(name);
}
