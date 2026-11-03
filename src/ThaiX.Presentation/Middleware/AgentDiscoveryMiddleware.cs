namespace ThaiX.Presentation.Middleware;

public sealed class AgentDiscoveryMiddleware(RequestDelegate next)
{
    private const string HomepageLinks =
        "</>; rel=\"alternate\"; type=\"text/markdown\", " +
        "</api.md>; rel=\"service-doc\"; type=\"text/markdown\", " +
        "</auth.md>; rel=\"describedby\"; type=\"text/markdown\", " +
        "</.well-known/api-catalog>; rel=\"describedby\"; type=\"application/linkset+json\"";

    private const string HomepageMarkdown = """
        # ThaiX

        ThaiX is a personal finance and market intelligence platform. It aggregates market data,
        supports portfolio tracking, and provides notification automation.

        - [Public API documentation](/api.md)
        - [Authentication documentation](/auth.md)
        - [API linkset catalog](/.well-known/api-catalog)
        - [Health status](/health)
        """;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/" &&
            (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)))
        {
            context.Response.Headers.Append("Link", HomepageLinks);
            context.Response.Headers.Append("Vary", "Accept");

            if (context.Request.Headers["Accept"].ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(value => value.StartsWith("text/markdown", StringComparison.OrdinalIgnoreCase)))
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
