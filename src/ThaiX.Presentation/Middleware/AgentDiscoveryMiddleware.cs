namespace ThaiX.Presentation.Middleware;

public sealed class AgentDiscoveryMiddleware(RequestDelegate next)
{
    private const string HomepageLinks =
        "</>; rel=\"alternate\"; type=\"text/markdown\", " +
        "</api.md>; rel=\"service-doc\"; type=\"text/markdown\", " +
        "</auth.md>; rel=\"describedby\"; type=\"text/markdown\", " +
        "</.well-known/api-catalog>; rel=\"describedby\"; type=\"application/linkset+json\", " +
        "</.well-known/agent-skills/index.json>; rel=\"describedby\"; type=\"application/json\", " +
        "</.well-known/ai-catalog.json>; rel=\"ai-catalog\"; type=\"application/json\"";

    private const string HomepageMarkdown = """
        # ThaiX

        ThaiX is a personal finance and market intelligence platform. It aggregates market data,
        supports portfolio tracking, and provides notification automation.

        - [Public API documentation](/api.md)
        - [Authentication documentation](/auth.md)
        - [API linkset catalog](/.well-known/api-catalog)
        - [Agent Skills index](/.well-known/agent-skills/index.json)
        - [Agentic Resource Discovery manifest](/.well-known/ai-catalog.json)
        - [Health status](/health)
        """;

    private static bool AcceptsMarkdown(string acceptHeader)
    {
        foreach (var mediaRange in acceptHeader.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parameters = mediaRange.Split(';', StringSplitOptions.TrimEntries);
            if (!parameters[0].Equals("text/markdown", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var qualityParameter = parameters
                .Skip(1)
                .FirstOrDefault(parameter => parameter.StartsWith("q=", StringComparison.OrdinalIgnoreCase));

            if (qualityParameter is null)
            {
                return true;
            }

            if (double.TryParse(
                    qualityParameter.AsSpan(2),
                    System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var quality) &&
                quality > 0 && quality <= 1)
            {
                return true;
            }
        }

        return false;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Value?.StartsWith("/.well-known/agent-skills/", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        }

        if (context.Request.Path == "/" &&
            (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)))
        {
            context.Response.Headers.Append("Link", HomepageLinks);
            context.Response.Headers.Append("Vary", "Accept");

            if (AcceptsMarkdown(context.Request.Headers["Accept"].ToString()))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/markdown; charset=utf-8";

                if (!HttpMethods.IsHead(context.Request.Method))
                {
                    await context.Response.WriteAsync(HomepageMarkdown);
                }

                return;
            }
        }

        await next(context);
    }
}
