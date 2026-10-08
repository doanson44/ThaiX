using Hangfire.Dashboard;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ThaiX.Infrastructure.Identity;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire dashboard authorization filter with support for:
/// - Development mode: Validates authentication cookie from middleware
/// - Production mode: Permission-based authorization (requires JWT authentication)
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string _requiredPermission;
    private readonly string _cookieName;
    private readonly IDataProtector _cookieProtector;

    public HangfireDashboardAuthorizationFilter(
        string requiredPermission,
        IDataProtectionProvider dataProtectionProvider)
    {
        _requiredPermission = requiredPermission ?? string.Empty;
        _cookieName = "ThaiX_DevAuth_Hangfire";
        _cookieProtector = dataProtectionProvider.CreateProtector("ThaiX.AdminToolAuthentication");
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (!httpContext.Request.Cookies.TryGetValue(_cookieName, out var cookieValue))
        {
            return false;
        }

        string? userId;
        try
        {
            userId = _cookieProtector.Unprotect(cookieValue);
        }
        catch
        {
            userId = null;
        }

        if (!Guid.TryParse(userId, out _))
        {
            DeleteCookie(httpContext);
            return false;
        }

        var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();

        if (user == null)
        {
            DeleteCookie(httpContext);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_requiredPermission))
        {
            return true;
        }

        var claims = userManager.GetClaimsAsync(user).GetAwaiter().GetResult();
        var hasPermission = claims.Any(c =>
            c.Type == Application.Common.Constants.ClaimTypeConstants.Permission &&
            c.Value == _requiredPermission);

        if (!hasPermission)
        {
            DeleteCookie(httpContext);
            return false;
        }

        return true;
    }

    private static void DeleteCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete("ThaiX_DevAuth_Hangfire", new CookieOptions
        {
            Path = "/hangfire"
        });
    }

}
