using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using ThaiX.Domain.Common.Constants;
using ThaiX.Infrastructure.Identity;

namespace ThaiX.Presentation.Middleware;

/// <summary>
/// Middleware to protect Swagger UI access with permission-based authentication.
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
    private readonly string _cookieName;

    public SwaggerAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<SwaggerAuthorizationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _requiredPermission = configuration.GetValue<string>("Swagger:RequiredPermission")
                             ?? Permissions.SwaggerView;
        _cookieName = AuthenticationHelpers.GetCookieName("Swagger");
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IDataProtectionProvider dataProtectionProvider)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (!path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

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
