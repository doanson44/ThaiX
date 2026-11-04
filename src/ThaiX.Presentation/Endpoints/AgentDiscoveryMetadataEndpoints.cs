using System.Text.Json;

namespace ThaiX.Presentation.Endpoints;

public static class AgentDiscoveryMetadataEndpoints
{
    private const string AgentSkillsSchema =
        "https://schemas.agentskills.io/discovery/0.2.0/schema.json";

    private const string AgentSkillDigest =
        "sha256:f0613a5dd22648c4ca0826825e95840907ae946439744f09cc576b9b4cea2b71";

    public static void MapAgentDiscoveryMetadataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods(
            "/.well-known/agent-skills/index.json",
            [HttpMethods.Get, HttpMethods.Head],
            (HttpContext context) =>
            {
                SetDiscoveryHeaders(context);
                var index = new Dictionary<string, object>
                {
                    ["$schema"] = AgentSkillsSchema,
                    ["skills"] = new[]
                    {
                        new
                        {
                            name = "thaix-api-discovery",
                            type = "skill-md",
                            description = "Discover and use ThaiX public API documentation, authentication guidance, and health status.",
                            url = "/.well-known/agent-skills/thaix-api/SKILL.md",
                            digest = AgentSkillDigest
                        }
                    }
                };

                return Results.Json(index);
            })
            .AllowAnonymous()
            .ExcludeFromDescription();

        endpoints.MapMethods(
            "/.well-known/ai-catalog.json",
            [HttpMethods.Get, HttpMethods.Head],
            (HttpContext context, IConfiguration configuration) =>
            {
                var origin = GetCanonicalOrigin(configuration);
                if (origin is null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status503ServiceUnavailable,
                        title: "Canonical public origin is not configured.");
                }

                var domain = new Uri(origin).Host;
                var manifest = new
                {
                    specVersion = "1.0",
                    host = new
                    {
                        displayName = "ThaiX",
                        identifier = domain,
                        documentationUrl = $"{origin}/api.md"
                    },
                    entries = new object[]
                    {
                        new
                        {
                            identifier = $"urn:air:{domain}:api:public-documentation",
                            displayName = "ThaiX public API documentation",
                            type = "text/markdown",
                            url = $"{origin}/api.md",
                            description = "Documentation of the ThaiX API routes explicitly intended for anonymous public access.",
                            representativeQueries = new[]
                            {
                                "Which ThaiX API endpoints are publicly accessible?",
                                "How can I discover ThaiX published blog content?",
                                "How do I check ThaiX API health?"
                            }
                        },
                        new
                        {
                            identifier = $"urn:air:{domain}:skill:thaix-api-discovery",
                            displayName = "ThaiX API discovery skill",
                            type = "application/ai-skill+md",
                            url = $"{origin}/.well-known/agent-skills/thaix-api/SKILL.md",
                            description = "Instructions for discovering ThaiX public API capabilities and understanding its JWT authentication boundary.",
                            representativeQueries = new[]
                            {
                                "How should an AI agent discover and use the ThaiX API?",
                                "What authentication method does ThaiX support for machine clients?",
                                "Which ThaiX endpoints are safe to call without authentication?"
                            }
                        }
                    }
                };

                SetDiscoveryHeaders(context);
                return Results.Json(manifest);
            })
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    private static void SetDiscoveryHeaders(HttpContext context)
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        context.Response.Headers.CacheControl = "public, max-age=3600";
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
