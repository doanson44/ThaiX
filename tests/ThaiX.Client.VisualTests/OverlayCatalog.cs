namespace ThaiX.Client.VisualTests;

/// <summary>
/// Catalog of Radzen dialogs, confirm prompts, and toast/notifications exercised by Playwright.
/// Component names must stay aligned with dialog hosts under <c>src/ThaiX.Client</c>
/// (see <see cref="OverlayCatalogCoverageTests"/>).
/// </summary>
public static class OverlayCatalog
{
    public const int DesktopWidth = 1280;
    public const int DesktopHeight = 800;
    public const int MobileWidth = 375;
    public const int MobileHeight = 812;

    public enum OverlayKind
    {
        FormDialog,
        ConfirmDialog,
        Toast
    }

    public enum OverlayStepKind
    {
        /// <summary>Primary toolbar button whose icon text is <c>add</c>.</summary>
        ClickAddIcon,

        /// <summary>First visible button whose Radzen icon text matches <see cref="OverlayStep.Arg"/>.</summary>
        ClickIcon,

        /// <summary>CSS / Playwright selector click (first match).</summary>
        ClickCss,

        /// <summary>Fill input matched by selector; value in <see cref="OverlayStep.Value"/>.</summary>
        FillCss,

        /// <summary>Wait until selector is visible.</summary>
        WaitCss,

        /// <summary>Open first portfolio detail from <c>/portfolio/list</c> (visibility icon).</summary>
        OpenFirstPortfolioDetail,

        /// <summary>Toggle first Radzen switch on, then click settings icon.</summary>
        EnableSwitchThenSettings
    }

    public sealed record OverlayStep(OverlayStepKind Kind, string? Arg = null, string? Value = null, bool Force = false);

    public sealed record OverlayCase(
        string ComponentName,
        string Id,
        OverlayKind Kind,
        string HostRoute,
        bool RequiresAuth,
        OverlayStep[] Steps,
        string AssertSelector,
        int Width = DesktopWidth,
        int Height = DesktopHeight);

    public static readonly OverlayCase[] Cases =
    [
        // --- Create / form dialogs (add toolbar) ---
        Form("CountryDialog", "master-data-country-create", "/master-data/countries", ClickAdd()),
        Form("CityDialog", "master-data-city-create", "/master-data/cities", ClickAdd()),
        Form("DistrictDialog", "master-data-district-create", "/master-data/districts", ClickAdd()),
        Form("BankDialog", "master-data-bank-create", "/master-data/banks", ClickAdd()),
        Form("ContactEditorDialog", "crm-contact-create", "/crm/contacts", ClickAdd()),
        Form("NoteEditDialog", "crm-note-edit", "/crm/notes",
            new OverlayStep(OverlayStepKind.ClickIcon, "edit")),
        Form("UserEditorDialog", "admin-user-create", "/admin/users", ClickAdd()),
        Form("BlogCategoryDialog", "blog-category-create", "/admin/blog/categories", ClickAdd()),
        Form("BlogTagDialog", "blog-tag-create", "/admin/blog/tags", ClickAdd()),
        Form("CreatePortfolioDialog", "portfolio-create", "/portfolio/list", ClickAdd()),
        Form("PriceAlertDialog", "price-alert-create", "/portfolio/price-alerts", ClickAdd()),
        Form("ExpenseTrackerDialog", "expense-tracker-create", "/portfolio/expense-tracker", ClickAdd()),
        Form("MarketScannerRuleDialog", "market-scanner-create", "/portfolio/market-scanner-rules", ClickAdd()),
        Form("ScheduleFormDialog", "notification-schedule-create", "/notifications/schedules", ClickAdd()),
        Form("ApiClientDialog", "api-client-create", "/security/api-clients", ClickAdd()),
        Form("CredentialAccountEditor", "credential-account-create", "/security/credential-accounts", ClickAdd()),

        // --- Edit / secondary dialogs ---
        Form("EditPortfolioDialog", "portfolio-edit", "/portfolio/list",
            new OverlayStep(OverlayStepKind.ClickIcon, "edit")),
        Form("ApiClientScopesDialog", "api-client-scopes", "/security/api-clients",
            new OverlayStep(OverlayStepKind.ClickIcon, "shield")),
        Form("ApiClientSecretDialog", "api-client-secret", "/security/api-clients",
            new OverlayStep(OverlayStepKind.ClickIcon, "vpn_key"),
            new OverlayStep(OverlayStepKind.WaitCss, ".rz-dialog-wrapper"),
            new OverlayStep(OverlayStepKind.ClickCss, ".rz-dialog-wrapper button.rz-primary, .rz-dialog-wrapper .rz-button-primary"),
            new OverlayStep(OverlayStepKind.WaitCss, ".rz-dialog-wrapper")),
        Form("CredentialAccountPasswordDialog", "credential-password", "/security/credential-accounts",
            new OverlayStep(OverlayStepKind.ClickIcon, "visibility")),
        Form("CredentialAccountAuditLogsDialog", "credential-audit", "/security/credential-accounts",
            new OverlayStep(OverlayStepKind.ClickIcon, "history")),

        // --- Portfolio detail nested dialogs ---
        Form("CreateCryptoPositionDialog", "crypto-position-create", "/portfolio/list",
            new OverlayStep(OverlayStepKind.OpenFirstPortfolioDetail),
            new OverlayStep(OverlayStepKind.ClickAddIcon)),
        Form("CreateStockPositionDialog", "stock-position-create", "/portfolio/list",
            new OverlayStep(OverlayStepKind.OpenFirstPortfolioDetail),
            new OverlayStep(OverlayStepKind.ClickCss, ".rz-tabview-nav li >> nth=1"),
            new OverlayStep(OverlayStepKind.ClickAddIcon)),
        Form("CreateSavingPositionDialog", "saving-position-create", "/portfolio/list",
            new OverlayStep(OverlayStepKind.OpenFirstPortfolioDetail),
            new OverlayStep(OverlayStepKind.ClickCss, ".rz-tabview-nav li >> nth=2"),
            new OverlayStep(OverlayStepKind.ClickAddIcon)),
        Form("AddCryptoTransactionDialog", "crypto-tx-add", "/portfolio/list",
            new OverlayStep(OverlayStepKind.OpenFirstPortfolioDetail),
            new OverlayStep(OverlayStepKind.ClickCss, ".rz-data-grid button.rz-info:has(.rzi), .rz-data-grid button:has(.rzi:text-is('add')) >> nth=0")),
        Form("AddStockTransactionDialog", "stock-tx-add", "/portfolio/list",
            new OverlayStep(OverlayStepKind.OpenFirstPortfolioDetail),
            new OverlayStep(OverlayStepKind.ClickCss, ".rz-tabview-nav li >> nth=1"),
            new OverlayStep(OverlayStepKind.ClickCss, ".rz-data-grid button.rz-info:has(.rzi), .rz-data-grid button:has(.rzi:text-is('add')) >> nth=0")),

        // --- Market research / misc ---
        Form("TradeSuggestionDialog", "trade-suggestion", "/market-research/mexc/spot-ticker",
            new OverlayStep(OverlayStepKind.ClickIcon, "insights")),
        // Dialog is mobile-only (desktop uses an inline details pane).
        Form("CoinGeckoCoinDetailDialog", "coingecko-detail", "/market-research/coingecko",
            MobileWidth, MobileHeight,
            new OverlayStep(OverlayStepKind.WaitCss, "button:has(.rzi), button:has(.rzi-chevron-right)"),
            new OverlayStep(OverlayStepKind.ClickIcon, "chevron_right")),
        Form("WeeklySuggestionPicksDialog", "weekly-picks", "/market-research/mexc/spot-ticker",
            new OverlayStep(OverlayStepKind.ClickCss, "button:has(.rzi:text-is('visibility')) >> nth=0")),
        Form("MexcSocketPriceMoveAlertDialog", "mexc-socket-alert", "/market-data/mexc/spot-ticker-socket",
            new OverlayStep(OverlayStepKind.WaitCss, "button.tx-mexc-price-alert-settings"),
            new OverlayStep(OverlayStepKind.ClickCss, "button.tx-mexc-price-alert-settings")),
        Form("DonateModal", "donate-modal", "/",
            new OverlayStep(OverlayStepKind.WaitCss, "button.tx-donate-btn, .tx-donate-btn"),
            new OverlayStep(OverlayStepKind.ClickCss, "button.tx-donate-btn, .tx-donate-btn")),
        Form("OptimizeForJobDialog", "resume-optimize", "/admin/resume",
            new OverlayStep(OverlayStepKind.ClickIcon, "work")),
        Form("AiReportDialog", "resume-ai-report", "/admin/resume",
            new OverlayStep(OverlayStepKind.ClickIcon, "fact_check")),
        Form("ResumeView", "resume-preview", "/admin/resume",
            new OverlayStep(OverlayStepKind.ClickCss, "button:has(.rzi:text-is('visibility')) >> nth=0")),

        // --- Confirm dialogs ---
        Confirm("ConfirmDelete-Portfolio", "confirm-delete-portfolio", "/portfolio/list",
            new OverlayStep(OverlayStepKind.ClickIcon, "delete")),
        Confirm("ConfirmDelete-ApiClient", "confirm-delete-api-client", "/security/api-clients",
            new OverlayStep(OverlayStepKind.ClickIcon, "delete")),
        Confirm("ConfirmDelete-Contact", "confirm-delete-contact", "/crm/contacts",
            new OverlayStep(OverlayStepKind.ClickIcon, "delete")),
        Confirm("ConfirmRegenerate-ApiSecret", "confirm-regenerate-secret", "/security/api-clients",
            new OverlayStep(OverlayStepKind.ClickIcon, "vpn_key")),

        // --- Toasts / notifications ---
        Toast("Toast-NotifySuccess", "toast-weekly-job", "/market-research/mexc/spot-ticker", true,
            new OverlayStep(OverlayStepKind.ClickIcon, "play_arrow")),
        Toast("Toast-NotifyError", "toast-resend-confirmation", "/auth/resend-confirmation", false,
            new OverlayStep(OverlayStepKind.FillCss, "input[type='email'], input.rz-textbox, .rz-textbox input", "fail@example.com"),
            new OverlayStep(OverlayStepKind.ClickCss, "button[type='submit'], button.rz-primary, button.rz-button-primary")),
        Toast("Toast-NotifyFromCredentialCopy", "toast-credential-copy", "/security/credential-accounts", true,
            new OverlayStep(OverlayStepKind.ClickIcon, "content_copy"))
    ];

    public static IReadOnlyList<string> RequiredComponentNames { get; } =
        Cases
            .Where(c => c.Kind == OverlayKind.FormDialog)
            .Select(c => c.ComponentName)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

    private static OverlayStep ClickAdd() => new(OverlayStepKind.ClickAddIcon);

    private static OverlayCase Form(
        string component,
        string id,
        string route,
        params OverlayStep[] steps) =>
        Form(component, id, route, DesktopWidth, DesktopHeight, steps);

    private static OverlayCase Form(
        string component,
        string id,
        string route,
        int width,
        int height,
        params OverlayStep[] steps) =>
        new(component, id, OverlayKind.FormDialog, route, true, steps,
            ".rz-dialog-wrapper .rz-dialog:visible, .rz-dialog-wrapper:visible .rz-dialog",
            width,
            height);

    private static OverlayCase Confirm(
        string component,
        string id,
        string route,
        params OverlayStep[] steps) =>
        new(component, id, OverlayKind.ConfirmDialog, route, true, steps,
            ".rz-dialog-confirm:visible, .rz-dialog-wrapper:visible .rz-dialog, .rz-dialog-wrapper:visible");

    private static OverlayCase Toast(
        string component,
        string id,
        string route,
        bool requiresAuth,
        params OverlayStep[] steps) =>
        new(component, id, OverlayKind.Toast, route, requiresAuth, steps,
            ".rz-notification-item:visible, .rz-notification-content:visible, .rz-notification:visible");
}
