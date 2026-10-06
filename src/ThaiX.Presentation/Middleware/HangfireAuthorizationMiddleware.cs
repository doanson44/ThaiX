using Microsoft.AspNetCore.Identity;
using ThaiX.Infrastructure.Identity;

namespace ThaiX.Presentation.Middleware;

/// <summary>
/// Middleware to protect Hangfire Dashboard access with permission-based authorization.
/// Development mode: Identity-based login (username/email + password)
/// Production mode: JWT-based authorization
/// </summary>
public sealed class HangfireAuthorizationMiddleware
{
    private const string ToolName = "Hangfire Dashboard";
    private const string ToolIcon = "speedometer2";
    private const string AuthPath = "/hangfire/auth";
    private const string RedirectPath = "/hangfire";

    private readonly RequestDelegate _next;
    private readonly ILogger<HangfireAuthorizationMiddleware> _logger;
    private readonly bool _isDevelopment;
    private readonly string _requiredPermission;
    private readonly string _cookieName;

    public HangfireAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<HangfireAuthorizationMiddleware> logger,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = environment.IsDevelopment();
        _requiredPermission = configuration["Hangfire:Dashboard:RequiredPermission"] ?? "System.Admin";
        _cookieName = AuthenticationHelpers.GetCookieName("Hangfire");
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Only intercept Hangfire Dashboard requests
        if (!path.StartsWith("/hangfire", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Development mode: Use Identity-based authentication
        if (_isDevelopment)
        {
            var options = new AuthenticationHelpers.DevToolLoginOptions
            {
                ToolName = ToolName,
                ToolIcon = ToolIcon,
                AuthPath = AuthPath,
                RedirectPath = RedirectPath,
                CookieName = _cookieName,
                RequiredPermission = _requiredPermission
            };

            var isAuthenticated = await AuthenticationHelpers.HandleDevelopmentToolRequestAsync(
                context,
                options,
                _logger,
                userManager,
                signInManager);

            if (!isAuthenticated)
            {
                return;
            }

            await _next(context);
            return;
        }

        // Production mode: Requires JWT authentication (handled by HangfireDashboardAuthorizationFilter)
        await _next(context);
    }
}

/// <summary>
/// Extension methods for HangfireAuthorizationMiddleware registration.
/// </summary>
public static class HangfireAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseHangfireAuthorization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<HangfireAuthorizationMiddleware>();
    }
}
