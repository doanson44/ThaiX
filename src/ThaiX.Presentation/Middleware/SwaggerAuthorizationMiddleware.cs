using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using ThaiX.Domain.Common.Constants;
using ThaiX.Infrastructure.Identity;

namespace ThaiX.Presentation.Middleware;

/// <summary>
/// Middleware to protect Swagger UI access with permission-based authorization.
/// Development mode: Identity-based login (username/email + password)
/// Production mode: JWT-based authorization
/// </summary>
public sealed class SwaggerAuthorizationMiddleware
{
    private const string ToolName = "Swagger API Documentation";
    private const string ToolIcon = "file-earmark-code";
    private const string AuthPath = "/swagger/auth";
    private const string RedirectPath = "/swagger/index.html";

    private readonly RequestDelegate _next;
    private readonly ILogger<SwaggerAuthorizationMiddleware> _logger;
    private readonly string _requiredPermission;
    private readonly bool _isDevelopment;
    private readonly string _cookieName;

    public SwaggerAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<SwaggerAuthorizationMiddleware> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _requiredPermission = configuration.GetValue<string>("Swagger:RequiredPermission")
                             ?? Permissions.SwaggerView;
        _isDevelopment = environment.IsDevelopment();
        _cookieName = AuthenticationHelpers.GetCookieName("Swagger");
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAuthorizationService authorizationService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IDataProtectionProvider dataProtectionProvider)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Only intercept Swagger UI paths (not the JSON spec itself)
        if (!path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Allow access to swagger.json without authentication (needed by UI)
        if (path.EndsWith("/swagger.json", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith("/v1/swagger.json", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Development mode: Username/Password authentication via Identity
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
                signInManager,
                dataProtectionProvider);

            if (!isAuthenticated)
            {
                return;
            }

            await _next(context);
            return;
        }

        // Production mode: Permission-based authorization (JWT)
        var currentUser = context.User;

        // Check authentication
        if (!currentUser.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthenticated access attempt to Swagger UI from {IPAddress}",
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 401;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <title>Authentication Required</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 50px; text-align: center; }
        h1 { color: #d9534f; }
        p { color: #666; }
    </style>
</head>
<body>
    <h1>401 - Authentication Required</h1>
    <p>You must be authenticated to access Swagger API Documentation.</p>
    <p>Contact administrator for access.</p>
</body>
</html>");
            return;
        }

        // Check authorization (permission-based)
        var authResult = await authorizationService.AuthorizeAsync(currentUser, null, _requiredPermission);

        if (!authResult.Succeeded)
        {
            var userId = currentUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogWarning("User {UserId} attempted to access Swagger UI without '{Permission}' permission",
                userId, _requiredPermission);

            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync($@"
<!DOCTYPE html>
<html>
<head>
    <title>Access Denied</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 50px; text-align: center; }}
        h1 {{ color: #d9534f; }}
        p {{ color: #666; }}
    </style>
</head>
<body>
    <h1>403 - Access Denied</h1>
    <p>You do not have permission to access Swagger API Documentation.</p>
    <p>Required permission: <strong>{_requiredPermission}</strong></p>
</body>
</html>");
            return;
        }

        // User is authenticated and authorized - allow access
        _logger.LogDebug("User {UserId} accessed Swagger UI",
            currentUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

        await _next(context);
    }
}

/// <summary>
/// Extension methods for registering SwaggerAuthorizationMiddleware.
/// </summary>
public static class SwaggerAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseSwaggerAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SwaggerAuthorizationMiddleware>();
    }
}
