namespace ThaiX.Client.VisualTests;

/// <summary>
/// Single source of truth for every routable page in ThaiX.Client, used by the
/// full-system style-consistency and visual-regression Playwright suites.
/// Kept in sync with <c>@page</c> directives via <see cref="RouteCatalogCoverageTests"/>.
/// </summary>
public static class RouteCatalog
{
    /// <summary>Fixed GUID used to exercise <c>{Id:guid}</c> edit/detail templates.</summary>
    public const string SmokeEntityId = "00000000-0000-0000-0000-000000000001";

    public static readonly (string Name, int Width, int Height)[] Viewports =
    [
        ("Desktop", 1280, 800),
        ("Tablet", 768, 1024),
        ("Mobile", 375, 812)
    ];

    // Reachable without an authenticated session (mostly AuthLayout; /resume/{slug}
    // and /blog/{slug} use public layouts — smoke slugs exercise those templates).
    public static readonly string[] PublicRoutes =
    [
        "/auth/login",
        "/auth/register",
        "/auth/forgot-password",
        "/auth/reset-password",
        "/auth/confirm-email",
        "/auth/resend-confirmation",
        "/auth/lockout",
        "/auth/login-2fa",
        "/auth/health",
        "/auth/external-login-callback",
        "/not-found",
        "/resume/route-catalog-smoke-test",
        "/blog",
        "/blog/route-catalog-smoke-test",
        "/public/json-bins/share/route-catalog-smoke-test",
        "/tools",
        "/tools/json-formatter",
        "/tools/markdown-preview",
        "/tools/position-size"
    ];

    // Rendered under MainLayout, requires the admin session established by AuthFixture.
    public static readonly string[] AuthenticatedRoutes =
    [
        "/",
        "/account/profile",
        "/account/change-password",
        "/account/2fa",
        "/account/personal-data",
        "/account/enable-authenticator",
        "/admin/users",
        "/admin/command-tester",
        "/admin/ai-tester",
        "/admin/resume",
        "/admin/json-bins",
        "/admin/json-bins/create",
        $"/admin/json-bins/{SmokeEntityId}",
        "/admin/blog/posts",
        "/admin/blog/posts/new",
        $"/admin/blog/posts/{SmokeEntityId}",
        "/admin/blog/categories",
        "/admin/blog/tags",
        "/crm/contacts",
        "/crm/notes",
        "/master-data/countries",
        "/master-data/cities",
        "/master-data/districts",
        "/master-data/banks",
        "/market-data/bank-interest-rates",
        "/market-data/sacombank-exchange-rates",
        "/market-data/tcbs-top10",
        "/market-data/mexc/spot-ticker-socket",
        "/market-data/mexc/contract-ticker-socket",
        "/market-data/power-655-analysis",
        "/market-research/dragon-capital",
        "/market-research/coingecko",
        "/market-research/vndirect/top-stocks",
        "/market-research/chainbroker/funds",
        "/market-research/chainbroker/projects",
        "/market-research/chainbroker/unlocks",
        "/market-research/mexc/spot-ticker",
        "/market-research/mexc/contract-ticker",
        "/notifications/schedules",
        "/notifications/sender",
        "/portfolio/list",
        $"/portfolio/{SmokeEntityId}",
        "/portfolio/price-alerts",
        "/portfolio/market-scanner-rules",
        "/portfolio/expense-tracker",
        "/security/credential-accounts",
        "/security/api-clients"
    ];

    public static IReadOnlyList<string> AllCatalogRoutes { get; } =
        PublicRoutes.Concat(AuthenticatedRoutes).ToArray();

    // Representative sample spanning every feature area, used for the design-token
    // (CSS custom property) cross-page consistency check.
    public static readonly (string Route, bool RequiresAuth)[] DesignTokenSampleRoutes =
    [
        ("/", true),
        ("/blog", false),
        ("/master-data/countries", true),
        ("/portfolio/list", true),
        ("/portfolio/expense-tracker", true),
        ("/market-data/bank-interest-rates", true),
        ("/market-research/chainbroker/funds", true),
        ("/security/api-clients", true),
        ("/auth/login", false)
    ];

    public static IEnumerable<object[]> DesignTokenSampleRoutesData()
    {
        foreach (var (route, requiresAuth) in DesignTokenSampleRoutes)
            yield return new object[] { route, requiresAuth };
    }

    public static IEnumerable<object[]> AllRoutesAllViewports()
    {
        foreach (var route in PublicRoutes)
            foreach (var (name, w, h) in Viewports)
                yield return new object[] { route, false, name, w, h };

        foreach (var route in AuthenticatedRoutes)
            foreach (var (name, w, h) in Viewports)
                yield return new object[] { route, true, name, w, h };
    }
}
