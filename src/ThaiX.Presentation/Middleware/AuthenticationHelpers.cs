using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using ThaiX.Infrastructure.Identity;

namespace ThaiX.Presentation.Middleware;

/// <summary>
/// Shared authentication helpers for Development mode admin tools (Swagger, Hangfire).
/// </summary>
public static class AuthenticationHelpers
{
    private const string CookiePrefix = "ThaiX_DevAuth_";
    private const string CookieProtectionPurpose = "ThaiX.AdminToolAuthentication";

    /// <summary>
    /// Shared configuration for development admin tool login pages (Swagger/Hangfire).
    /// </summary>
    public sealed record DevToolLoginOptions
    {
        public required string ToolName { get; init; }
        public required string ToolIcon { get; init; }
        public required string AuthPath { get; init; }
        public required string RedirectPath { get; init; }
        public required string CookieName { get; init; }
        public string CookiePath { get; init; } = "/";
        public required string RequiredPermission { get; init; }
    }

    /// <summary>
    /// Validates user credentials (supports both username and email) and checks permission.
    /// </summary>
    public static async Task<(bool IsValid, ApplicationUser? User, string? ErrorMessage)> ValidateCredentialsAsync(
        string usernameOrEmail,
        string password,
        string requiredPermission,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return (false, null, "Username and password are required");
        }

        // Try to find user by email first, then by username
        var user = await userManager.FindByEmailAsync(usernameOrEmail)
                   ?? await userManager.FindByNameAsync(usernameOrEmail);

        if (user == null)
        {
            return (false, null, "Invalid username or password");
        }

        // Check password
        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return (false, null, "Invalid username or password");
        }

        // Check permission
        var claims = await userManager.GetClaimsAsync(user);
        var hasPermission = claims.Any(c =>
            c.Type == Application.Common.Constants.ClaimTypeConstants.Permission &&
            c.Value == requiredPermission);

        if (!hasPermission)
        {
            return (false, null, $"Access denied. Required permission: {requiredPermission}");
        }

        return (true, user, null);
    }

    /// <summary>
    /// Validates authentication cookie and checks if user still has required permission.
    /// </summary>
    public static async Task<bool> ValidateAuthenticationCookieAsync(
        HttpContext context,
        string cookieName,
        string requiredPermission,
        string cookiePath,
        UserManager<ApplicationUser> userManager,
        IDataProtectionProvider dataProtectionProvider)
    {
        if (!context.Request.Cookies.TryGetValue(cookieName, out var cookieValue))
        {
            return false;
        }

        var userId = DecryptCookie(cookieValue, dataProtectionProvider);
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.Cookies.Delete(cookieName, new CookieOptions { Path = cookiePath });
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            context.Response.Cookies.Delete(cookieName, new CookieOptions { Path = cookiePath });
            return false;
        }

        var claims = await userManager.GetClaimsAsync(user);
        var hasPermission = claims.Any(c =>
            c.Type == Application.Common.Constants.ClaimTypeConstants.Permission &&
            c.Value == requiredPermission);

        if (!hasPermission)
        {
            context.Response.Cookies.Delete(cookieName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sets authentication cookie for the user.
    /// </summary>
    public static void SetAuthenticationCookie(
        HttpContext context,
        string cookieName,
        Guid userId,
        string cookiePath,
        IDataProtectionProvider dataProtectionProvider)
    {
        var encryptedUserId = EncryptCookie(userId.ToString(), dataProtectionProvider);
        context.Response.Cookies.Append(cookieName, encryptedUserId, new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = cookiePath,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });
    }

    /// <summary>
    /// Gets cookie name for a specific tool.
    /// </summary>
    public static string GetCookieName(string toolName) => $"{CookiePrefix}{toolName}";

    /// <summary>
    /// Handles shared development-mode auth flow for admin tools.
    /// Returns true when access is granted and request should continue.
    /// Returns false when response has been handled (login page or redirect).
    /// </summary>
    public static async Task<bool> HandleDevelopmentToolRequestAsync(
        HttpContext context,
        DevToolLoginOptions options,
        ILogger logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IDataProtectionProvider dataProtectionProvider)
    {
        if (context.Request.Path.Equals(options.AuthPath, StringComparison.OrdinalIgnoreCase) &&
            HttpMethods.IsPost(context.Request.Method))
        {
            await HandleAuthenticationPostAsync(
                context,
                options,
                logger,
                userManager,
                signInManager,
                dataProtectionProvider);
            return false;
        }

        var isAuthenticated = await ValidateAuthenticationCookieAsync(
            context,
            options.CookieName,
            options.RequiredPermission,
            options.CookiePath,
            userManager,
            dataProtectionProvider);

        if (isAuthenticated)
        {
            logger.LogDebug("Development mode: {ToolName} access granted via cookie", options.ToolName);
            return true;
        }

        logger.LogDebug("Development mode: Showing {ToolName} login page", options.ToolName);
        await ShowDevelopmentLoginPageAsync(context, options);
        return false;
    }

    /// <summary>
    /// Generates the shared login page HTML.
    /// </summary>
    public static string GenerateLoginPageHtml(
        string toolName,
        string toolIcon,
        string actionUrl,
        string? errorMessage = null)
    {
        var errorHtml = string.IsNullOrEmpty(errorMessage)
            ? string.Empty
            : $@"<div class='alert alert-danger' role='alert'>
                    <i class='bi bi-exclamation-triangle-fill me-2'></i>{errorMessage}
                 </div>";

        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{toolName} - Authentication Required</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css' rel='stylesheet'>
    <style>
        body {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        }}
        .login-card {{
            background: white;
            border-radius: 15px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
            max-width: 420px;
            width: 100%;
            animation: slideUp 0.4s ease-out;
        }}
        @keyframes slideUp {{
            from {{
                opacity: 0;
                transform: translateY(30px);
            }}
            to {{
                opacity: 1;
                transform: translateY(0);
            }}
        }}
        .login-header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 2rem;
            border-radius: 15px 15px 0 0;
            text-align: center;
        }}
        .login-body {{
            padding: 2rem;
        }}
        .btn-login {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: none;
            padding: 0.75rem;
            font-weight: 600;
            transition: all 0.3s ease;
        }}
        .btn-login:hover {{
            background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
        }}
        .form-control:focus {{
            border-color: #667eea;
            box-shadow: 0 0 0 0.2rem rgba(102, 126, 234, 0.25);
        }}
        .env-badge {{
            position: absolute;
            top: 1rem;
            right: 1rem;
            background: rgba(255, 255, 255, 0.2);
            color: white;
            padding: 0.25rem 0.75rem;
            border-radius: 20px;
            font-size: 0.875rem;
            font-weight: 600;
        }}
        .tool-icon {{
            font-size: 3rem;
            margin-bottom: 1rem;
            animation: pulse 2s ease-in-out infinite;
        }}
        @keyframes pulse {{
            0%, 100% {{
                transform: scale(1);
            }}
            50% {{
                transform: scale(1.05);
            }}
        }}
        .form-label {{
            font-weight: 500;
            color: #495057;
        }}
        .info-text {{
            color: #6c757d;
            font-size: 0.875rem;
        }}
    </style>
</head>
<body>
    <div class='login-card'>
        <div class='login-header position-relative'>
            <span class='env-badge'>
                <i class='bi bi-shield-lock me-1'></i>Secure Access
            </span>
            <div class='tool-icon'>
                <i class='bi bi-{toolIcon}'></i>
            </div>
            <h4 class='mb-1'>{toolName}</h4>
            <p class='mb-0 opacity-75'>Authentication Required</p>
        </div>
        <div class='login-body'>
            {errorHtml}
            <form method='post' action='{actionUrl}'>
                <div class='mb-3'>
                    <label for='username' class='form-label'>
                        <i class='bi bi-person-circle me-1'></i>Username or Email
                    </label>
                    <input type='text' class='form-control' id='username' name='username' 
                           placeholder='admin or admin@thaix.local' required autofocus>
                    <div class='form-text info-text'>
                        <i class='bi bi-info-circle me-1'></i>Use your ThaiX account credentials.
                    </div>
                </div>
                <div class='mb-4'>
                    <label for='password' class='form-label'>
                        <i class='bi bi-key me-1'></i>Password
                    </label>
                    <input type='password' class='form-control' id='password' name='password' 
                           placeholder='Enter your password' required>
                </div>
                <button type='submit' class='btn btn-primary btn-login w-100'>
                    <i class='bi bi-box-arrow-in-right me-2'></i>Sign In
                </button>
            </form>
            <hr class='my-4'>
            <div class='text-center info-text'>
                <i class='bi bi-shield-check me-1'></i>
                Use your ThaiX account credentials
            </div>
        </div>
    </div>
</body>
</html>";
    }

    private static string EncryptCookie(string value, IDataProtectionProvider dataProtectionProvider)
    {
        return dataProtectionProvider
            .CreateProtector(CookieProtectionPurpose)
            .Protect(value);
    }

    private static string? DecryptCookie(
        string encryptedValue,
        IDataProtectionProvider dataProtectionProvider)
    {
        try
        {
            return dataProtectionProvider
                .CreateProtector(CookieProtectionPurpose)
                .Unprotect(encryptedValue);
        }
        catch
        {
            return null;
        }
    }

    private static async Task HandleAuthenticationPostAsync(
        HttpContext context,
        DevToolLoginOptions options,
        ILogger logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IDataProtectionProvider dataProtectionProvider)
    {
        try
        {
            if (!context.Request.HasFormContentType)
            {
                await ShowDevelopmentLoginPageAsync(context, options, "Invalid authentication request");
                return;
            }

            var form = await context.Request.ReadFormAsync();
            var usernameOrEmail = form["username"].ToString();
            var password = form["password"].ToString();

            var (isValid, user, errorMessage) = await ValidateCredentialsAsync(
                usernameOrEmail,
                password,
                options.RequiredPermission,
                userManager,
                signInManager);

            if (!isValid)
            {
                logger.LogWarning("{ToolName} login failed from {IPAddress}: {Error}",
                    options.ToolName,
                    context.Connection.RemoteIpAddress,
                    errorMessage);
                await ShowDevelopmentLoginPageAsync(context, options, errorMessage);
                return;
            }

            SetAuthenticationCookie(
                context,
                options.CookieName,
                user!.Id,
                options.CookiePath,
                dataProtectionProvider);

            logger.LogInformation("User {Email} successfully authenticated for {ToolName} from {IPAddress}",
                user.Email,
                options.ToolName,
                context.Connection.RemoteIpAddress);

            context.Response.Redirect(options.RedirectPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during {ToolName} authentication", options.ToolName);
            await ShowDevelopmentLoginPageAsync(context, options, "An error occurred during authentication");
        }
    }

    private static async Task ShowDevelopmentLoginPageAsync(
        HttpContext context,
        DevToolLoginOptions options,
        string? errorMessage = null)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "text/html";

        var html = GenerateLoginPageHtml(
            toolName: options.ToolName,
            toolIcon: options.ToolIcon,
            actionUrl: options.AuthPath,
            errorMessage: errorMessage);

        await context.Response.WriteAsync(html);
    }
}
