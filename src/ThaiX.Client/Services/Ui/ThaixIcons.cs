namespace ThaiX.Client.Services.Ui;

public static class ThaixIcons
{
    private static readonly IReadOnlyDictionary<string, string> BootstrapIcons =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["account_balance_wallet"] = "wallet2",
            ["add"] = "plus-lg",
            ["alert"] = "exclamation-circle",
            ["analytics"] = "graph-up",
            ["arrow_back"] = "arrow-left",
            ["bell"] = "bell",
            ["broken_image"] = "image-alt",
            ["calendar"] = "calendar3",
            ["camera_alt"] = "camera",
            ["cancel"] = "x-circle",
            ["cart"] = "cart3",
            ["check"] = "check-lg",
            ["check_circle"] = "check-circle",
            ["chevron_right"] = "chevron-right",
            ["close"] = "x-lg",
            ["code"] = "code-slash",
            ["content_copy"] = "copy",
            ["cube"] = "box",
            ["delete"] = "trash3",
            ["description"] = "file-earmark-text",
            ["document"] = "file-earmark-text",
            ["download"] = "download",
            ["edit"] = "pencil",
            ["gamepad"] = "controller",
            ["globe"] = "globe",
            ["home"] = "house",
            ["lightning"] = "lightning-charge",
            ["link"] = "link-45deg",
            ["lock"] = "lock",
            ["logout"] = "box-arrow-right",
            ["open_in_new"] = "box-arrow-up-right",
            ["person"] = "person",
            ["publish"] = "cloud-upload",
            ["search"] = "search",
            ["server"] = "hdd-network",
            ["settings"] = "gear",
            ["share"] = "share",
            ["shield-check"] = "shield-check",
            ["star"] = "star",
            ["team"] = "people",
            ["upload"] = "upload",
            ["upload_file"] = "file-earmark-arrow-up",
            ["user"] = "person",
            ["visibility"] = "eye",
            ["visibility_off"] = "eye-slash",
            ["wallet"] = "wallet2",
            ["west"] = "arrow-left"
        };

    private static readonly IReadOnlyDictionary<string, string> RadzenAliases =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["alert"] = "info",
            ["ai-chip"] = "memory",
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
            return "help";

        return RadzenAliases.TryGetValue(name, out var icon) ? icon : name;
    }

    public static string? ResolveBootstrapIcon(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return BootstrapIcons.TryGetValue(name, out var icon) ? icon : null;
    }

}
