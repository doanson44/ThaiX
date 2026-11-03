using System.Text.Json;

namespace ThaiX.Presentation.Endpoints;

public static class AgentDiscoveryEndpoints
{
    private const string RobotsContent = """
        User-agent: *
        Allow: /
        Disallow: /account/
        Disallow: /admin/
        Disallow: /api/
        Disallow: /auth/
        Disallow: /crm/
        Disallow: /notifications/
        Disallow: /portfolio/
        Disallow: /security/
        Disallow: /hangfire

        User-agent: OAI-SearchBot
        Allow: /
        Disallow: /api/

        User-agent: Claude-SearchBot
        Allow: /
        Disallow: /api/

        User-agent: PerplexityBot
        Allow: /
        Disallow: /api/

        User-agent: GPTBot
        Disallow: /

        User-agent: ClaudeBot
        Disallow: /

        User-agent: Google-Extended
        Disallow: /

        Content-Signal: ai-train=no, search=yes, ai-input=no
        """;

    private const string ApiDocumentation = """
        # ThaiX public API

        ThaiX exposes a Minimal API. The following routes are explicitly anonymous and intended
        for public discovery. All other API routes may require authentication and permissions.

        ## Health

        - `GET /health` - application health status.

        ## Published blog content

        - `GET /api/blog/posts/public` - paginated published posts.
        - `GET /api/blog/posts/public/{slug}` - one published post by slug.

        ## Published HTML pages

        - `GET /html/{slug}` - published HTML page wrapper.
        - `GET /api/public-html/{slug}/content` - published HTML content.

        ## Authentication

        ThaiX uses JWT Bearer authentication for protected API routes. See [/auth.md](/auth.md).
        This deployment does not expose an OAuth 2.0 or OpenID Connect authorization server.
        """;

    private const string AuthenticationDocumentation = """
        # ThaiX authentication

        Protected ThaiX API routes use JWT Bearer authentication. Send an issued access token
        using the `Authorization: Bearer <token>` request header.

        ThaiX currently uses ASP.NET Core Identity and a custom JWT issuer. It does not expose
        OAuth 2.0 authorization-server or OpenID Connect provider discovery metadata.

        Do not treat the JWT login/token endpoints as OAuth endpoints. Client registration,
        delegated authorization, dynamic client registration, and agent-specific registration
        are not advertised by this document.

        API permissions are enforced by permission claims. Access to a route depends on the
        permissions required by that route.
        """;

    public static void MapAgentDiscoveryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/robots.txt", (IConfiguration configuration) =>
        {
            var origin = GetCanonicalOrigin(configuration);
            var sitemapLine = origin is null ? string.Empty : $"Sitemap: {origin}/sitemap.xml\n";
            var content = RobotsContent.Replace(
                "Content-Signal: ai-train=no, search=yes, ai-input=no",
                $"{sitemapLine}Content-Signal: ai-train=no, search=yes, ai-input=no");

            return Results.Text(content, "text/plain; charset=utf-8");
        }).AllowAnonymous().ExcludeFromDescription();

        endpoints.MapGet("/sitemap.xml", (IConfiguration configuration) =>
        {
            var origin = GetCanonicalOrigin(configuration);
            if (origin is null)
            {
                return (IResult)Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Canonical public origin is not configured.");
            }

            var xml = $"""<?xml version="1.0" encoding="UTF-8"?><urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"><url><loc>{System.Security.SecurityElement.Escape(origin + "/")}</loc></url></urlset>""";
            return Results.Text(xml, "application/xml; charset=utf-8");
        }).AllowAnonymous().ExcludeFromDescription();

        endpoints.MapGet("/api.md", () => Results.Text(ApiDocumentation, "text/markdown; charset=utf-8"))
            .AllowAnonymous().ExcludeFromDescription();

        endpoints.MapGet("/auth.md", () => Results.Text(AuthenticationDocumentation, "text/markdown; charset=utf-8"))
            .AllowAnonymous().ExcludeFromDescription();

        endpoints.MapGet("/.well-known/api-catalog", () =>
        {
            var linkset = new Dictionary<string, object[]>
            {
                ["service-doc"] = [new { href = "/api.md", type = "text/markdown" }],
                ["status"] = [new { href = "/health", type = "application/json" }]
            };

            return Results.Text(
                JsonSerializer.Serialize(new { linkset = new[] { linkset } }),
                "application/linkset+json; charset=utf-8");
        }).AllowAnonymous().ExcludeFromDescription();
    }

    private static string? GetCanonicalOrigin(IConfiguration configuration)
    {
        var configuredOrigin = configuration["PublicOrigin"] ?? configuration["ClientBaseUrl"];
        if (!Uri.TryCreate(configuredOrigin, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            uri.Query.Length > 0 ||
            uri.Fragment.Length > 0 ||
            uri.AbsolutePath != "/")
        {
            return null;
        }

        return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }
}
