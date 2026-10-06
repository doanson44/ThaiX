using RK = ThaiX.Client.Constants.ResourceKeys;
using ThaiX.Client.Models.JsonBins;

namespace ThaiX.Client.Services.Tools;

public static class ToolCatalog
{
    public const JsonBinCategories JsonBinCategory = JsonBinCategories.Temp;
    public const int StateEnvelopeVersion = 1;

    public static string BuildCloudCode(string userId, string toolSlug) =>
        $"tools.{userId}.{toolSlug}";

    public static string LocalDraftKey(string toolSlug) =>
        $"ThaiX.tools.draft.{toolSlug}";

    public static readonly IReadOnlyList<ToolDefinition> All =
    [
        // Developer — existing text tools
        Dev("json-formatter", RK.Tools.JsonFormatter.Title, RK.Tools.JsonFormatter.Description, "data_object", true),
        Dev("json-compare", RK.Tools.JsonCompare.Title, RK.Tools.JsonCompare.Description, "compare_arrows", true),
        Dev("yaml-validator", RK.Tools.YamlValidator.Title, RK.Tools.YamlValidator.Description, "fact_check", true),
        Dev("yaml-json", RK.Tools.YamlJson.Title, RK.Tools.YamlJson.Description, "swap_horiz", true),
        Dev("xml-formatter", RK.Tools.XmlFormatter.Title, RK.Tools.XmlFormatter.Description, "code", true),
        Dev("csv-json", RK.Tools.CsvJson.Title, RK.Tools.CsvJson.Description, "table_view", true),
        Dev("json-to-csharp", RK.Tools.JsonToCsharp.Title, RK.Tools.JsonToCsharp.Description, "terminal", true),
        Dev("json-to-typescript", RK.Tools.JsonToTypescript.Title, RK.Tools.JsonToTypescript.Description, "javascript", true),
        Dev("sql-formatter", RK.Tools.SqlFormatter.Title, RK.Tools.SqlFormatter.Description, "storage", true),
        Dev("html-formatter", RK.Tools.HtmlFormatter.Title, RK.Tools.HtmlFormatter.Description, "html", true),
        Dev("css-formatter", RK.Tools.CssFormatter.Title, RK.Tools.CssFormatter.Description, "css", true),
        Dev("js-formatter", RK.Tools.JsFormatter.Title, RK.Tools.JsFormatter.Description, "javascript", true),
        Dev("markdown-preview", RK.Tools.MarkdownPreview.Title, RK.Tools.MarkdownPreview.Description, "preview", true),
        Dev("html-markdown", RK.Tools.HtmlMarkdown.Title, RK.Tools.HtmlMarkdown.Description, "swap_horiz", true),
        Dev("base64", RK.Tools.Base64.Title, RK.Tools.Base64.Description, "lock", true),
        Dev("url-codec", RK.Tools.UrlCodec.Title, RK.Tools.UrlCodec.Description, "link", true),
        Dev("sha256", RK.Tools.Sha256.Title, RK.Tools.Sha256.Description, "fingerprint", true),
        Dev("jwt-decoder", RK.Tools.JwtDecoder.Title, RK.Tools.JwtDecoder.Description, "key", false),
        Dev("password-generator", RK.Tools.PasswordGenerator.Title, RK.Tools.PasswordGenerator.Description, "password", false),
        Dev("password-strength", RK.Tools.PasswordStrength.Title, RK.Tools.PasswordStrength.Description, "security", false),
        Dev("uuid", RK.Tools.Uuid.Title, RK.Tools.Uuid.Description, "tag", true),
        Dev("cron", RK.Tools.Cron.Title, RK.Tools.Cron.Description, "schedule", true),
        Dev("qr", RK.Tools.Qr.Title, RK.Tools.Qr.Description, "qr_code", true),
        Dev("curl", RK.Tools.Curl.Title, RK.Tools.Curl.Description, "http", true),
        Dev("gitignore", RK.Tools.Gitignore.Title, RK.Tools.Gitignore.Description, "folder_off", true),
        Dev("conventional-commit", RK.Tools.ConventionalCommit.Title, RK.Tools.ConventionalCommit.Description, "commit", true),
        Dev("regex", RK.Tools.Regex.Title, RK.Tools.Regex.Description, "regular_expression", true),
        Dev("unix-timestamp", RK.Tools.UnixTimestamp.Title, RK.Tools.UnixTimestamp.Description, "schedule", true),
        Dev("diff", RK.Tools.Diff.Title, RK.Tools.Diff.Description, "difference", true),

        // Trading
        Calc("position-size", ToolCategory.Trading, RK.Tools.PositionSize.Title, RK.Tools.PositionSize.Description, "straighten"),
        Calc("risk-reward", ToolCategory.Trading, RK.Tools.RiskReward.Title, RK.Tools.RiskReward.Description, "balance"),
        Calc("futures-pnl", ToolCategory.Trading, RK.Tools.FuturesPnl.Title, RK.Tools.FuturesPnl.Description, "trending_up"),
        Calc("liquidation", ToolCategory.Trading, RK.Tools.Liquidation.Title, RK.Tools.Liquidation.Description, "warning"),
        Calc("average-entry", ToolCategory.Trading, RK.Tools.AverageEntry.Title, RK.Tools.AverageEntry.Description, "functions"),
        Calc("dca", ToolCategory.Trading, RK.Tools.Dca.Title, RK.Tools.Dca.Description, "savings"),
        Calc("break-even", ToolCategory.Trading, RK.Tools.BreakEven.Title, RK.Tools.BreakEven.Description, "horizontal_rule"),
        Calc("trading-fee", ToolCategory.Trading, RK.Tools.TradingFee.Title, RK.Tools.TradingFee.Description, "receipt"),
        Calc("funding", ToolCategory.Trading, RK.Tools.Funding.Title, RK.Tools.Funding.Description, "payments"),
        Calc("leverage", ToolCategory.Trading, RK.Tools.Leverage.Title, RK.Tools.Leverage.Description, "speed"),
        Calc("stop-loss", ToolCategory.Trading, RK.Tools.StopLoss.Title, RK.Tools.StopLoss.Description, "stop_circle"),
        Calc("take-profit", ToolCategory.Trading, RK.Tools.TakeProfit.Title, RK.Tools.TakeProfit.Description, "flag"),

        // Risk
        Calc("risk-of-ruin", ToolCategory.Risk, RK.Tools.RiskOfRuin.Title, RK.Tools.RiskOfRuin.Description, "dangerous"),
        Calc("max-drawdown", ToolCategory.Risk, RK.Tools.MaxDrawdown.Title, RK.Tools.MaxDrawdown.Description, "show_chart"),
        Calc("loss-recovery", ToolCategory.Risk, RK.Tools.LossRecovery.Title, RK.Tools.LossRecovery.Description, "replay"),
        Calc("expectancy", ToolCategory.Risk, RK.Tools.Expectancy.Title, RK.Tools.Expectancy.Description, "insights"),
        Calc("kelly-criterion", ToolCategory.Risk, RK.Tools.KellyCriterion.Title, RK.Tools.KellyCriterion.Description, "pie_chart"),
        Calc("consecutive-loss", ToolCategory.Risk, RK.Tools.ConsecutiveLoss.Title, RK.Tools.ConsecutiveLoss.Description, "trending_down"),

        // Crypto
        Calc("market-cap", ToolCategory.Crypto, RK.Tools.MarketCap.Title, RK.Tools.MarketCap.Description, "public"),
        Calc("market-cap-compare", ToolCategory.Crypto, RK.Tools.MarketCapCompare.Title, RK.Tools.MarketCapCompare.Description, "compare"),
        Calc("ath-drawdown", ToolCategory.Crypto, RK.Tools.AthDrawdown.Title, RK.Tools.AthDrawdown.Description, "south"),
    ];

    public static ToolDefinition? Find(string? slug) =>
        string.IsNullOrWhiteSpace(slug)
            ? null
            : All.FirstOrDefault(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<IGrouping<ToolCategory, ToolDefinition>> ByCategory() =>
        All.GroupBy(t => t.Category).OrderBy(g => (int)g.Key);

    public static string CategoryKey(ToolCategory category) => category switch
    {
        ToolCategory.Developer => RK.Tools.Category.Developer,
        ToolCategory.Trading => RK.Tools.Category.Trading,
        ToolCategory.Risk => RK.Tools.Category.Risk,
        ToolCategory.Crypto => RK.Tools.Category.Crypto,
        _ => RK.Tools.HubTitle
    };

    private static ToolDefinition Dev(
        string slug, string titleKey, string descriptionKey, string icon, bool allowCloud) =>
        new(slug, ToolCategory.Developer, ToolKind.TextTransform, titleKey, descriptionKey, icon, allowCloud, allowCloud);

    private static ToolDefinition Calc(
        string slug, ToolCategory category, string titleKey, string descriptionKey, string icon) =>
        new(slug, category, ToolKind.Calculator, titleKey, descriptionKey, icon, false, false);
}
