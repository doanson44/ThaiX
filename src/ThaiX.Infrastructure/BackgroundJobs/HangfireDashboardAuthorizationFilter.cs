using Hangfire.Dashboard;
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
    private readonly bool _isDevelopment;
    private readonly string _cookieName;

    public HangfireDashboardAuthorizationFilter(string requiredPermission, bool isDevelopment = false)
    {
        _requiredPermission = requiredPermission ?? string.Empty;
        _isDevelopment = isDevelopment;
        _cookieName = $"ThaiX_DevAuth_Hangfire"; // Must match AuthenticationHelpers.GetCookieName("Hangfire")
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Development mode: Check for authentication cookie
        if (_isDevelopment)
        {
            // If cookie exists and valid, allow access
            if (httpContext.Request.Cookies.TryGetValue(_cookieName, out var cookieValue))
            {
                var userId = DecryptCookie(cookieValue);
                if (!string.IsNullOrEmpty(userId))
                {
                    // Validate user still exists and has permission
                    var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();

                    if (user != null)
                    {
                        var claims = userManager.GetClaimsAsync(user).GetAwaiter().GetResult();
                        var hasPermission = claims.Any(c =>
                            c.Type == Application.Common.Constants.ClaimTypeConstants.Permission &&
                            c.Value == _requiredPermission);

                        if (hasPermission)
                        {
                            return true;
                        }
                    }
                }

                // Invalid cookie - remove it
                httpContext.Response.Cookies.Delete(_cookieName);
            }

            // No valid cookie - deny access (will show custom login page via middleware)
            return false;
        }

        // Production mode: Use standard permission check
        var currentUserService = httpContext.RequestServices
            .GetRequiredService<Application.Common.Interfaces.ICurrentUserService>();

        if (!currentUserService.IsAuthenticated)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_requiredPermission))
        {
            return true;
        }

        var userPermissions = currentUserService.GetPermissions();
        return userPermissions.Contains(_requiredPermission);
    }

    private static string? DecryptCookie(string encryptedValue)
    {
        try
        {
            var bytes = Convert.FromBase64String(encryptedValue);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }
}
