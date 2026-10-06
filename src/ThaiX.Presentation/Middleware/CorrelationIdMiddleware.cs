namespace ThaiX.Presentation.Middleware;

/// <summary>
/// Middleware that ensures every request has a correlation ID for tracking.
/// The correlation ID is either extracted from request headers or generated.
/// It is added to the response headers and made available to logging context.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request header
        var correlationId = GetCorrelationIdFromRequest(context);

        // If not provided, use TraceIdentifier as correlation ID
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = context.TraceIdentifier;
        }

        // Store in HttpContext.Items for access by other middleware/handlers
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
            }
            return Task.CompletedTask;
        });

        // Add to Serilog LogContext if available
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private string? GetCorrelationIdFromRequest(HttpContext context)
    {
        // Check multiple common header names
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            return correlationId.FirstOrDefault();
        }

        if (context.Request.Headers.TryGetValue("X-Request-ID", out var requestId))
        {
            return requestId.FirstOrDefault();
        }

        if (context.Request.Headers.TryGetValue("Request-Id", out var altRequestId))
        {
            return altRequestId.FirstOrDefault();
        }

        return null;
    }
}

/// <summary>
/// Extension methods for registering CorrelationIdMiddleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
