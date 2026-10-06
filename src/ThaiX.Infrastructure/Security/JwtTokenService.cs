using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ThaiX.Infrastructure.Security;

/// <summary>
/// Service for issuing self-signed JWT tokens.
/// Supports both user tokens (interactive login) and system tokens (M2M).
/// </summary>
public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a JWT token for an authenticated user.
    /// Token includes user identity and permissions.
    /// </summary>
    /// <param name="rememberMe">When true, uses extended lifetime (RememberMeTokenLifetimeMinutes).</param>
    public string GenerateUserToken(Guid userId, string email, IEnumerable<string> permissions, bool rememberMe = false)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        var jwtSettings = GetJwtSettings();
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("actor_type", "user")
        };

        // Add permissions as scope claims
        foreach (var permission in permissions.Distinct())
        {
            claims.Add(new Claim("scope", permission));
        }

        var lifetimeMinutes = rememberMe ? jwtSettings.RememberMeTokenLifetimeMinutes : jwtSettings.UserTokenLifetimeMinutes;

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(lifetimeMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Returns the user token lifetime in seconds for the given rememberMe flag.
    /// </summary>
    public int GetUserTokenExpiresInSeconds(bool rememberMe)
    {
        var jwtSettings = GetJwtSettings();
        var minutes = rememberMe ? jwtSettings.RememberMeTokenLifetimeMinutes : jwtSettings.UserTokenLifetimeMinutes;
        return minutes * 60;
    }

    /// <summary>
    /// Generates a JWT token for a system actor (M2M).
    /// Token includes client identity and scopes.
    /// </summary>
    public string GenerateSystemToken(string clientId, IEnumerable<string> scopes)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID is required.", nameof(clientId));

        var jwtSettings = GetJwtSettings();
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("actor_type", "system"),
            new("client_id", clientId)
        };

        // Add scopes (permissions)
        foreach (var scope in scopes.Distinct())
        {
            claims.Add(new Claim("scope", scope));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(jwtSettings.SystemTokenLifetimeHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns the principal.
    /// Used for manual validation scenarios.
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var jwtSettings = GetJwtSettings();
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudiences = new[] { jwtSettings.Audience },

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Generates a short-lived session token for 2FA verification.
    /// Valid for 5 minutes only.
    /// </summary>
    public string GenerateTwoFactorSessionToken(Guid userId)
    {
        var jwtSettings = GetJwtSettings();
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("purpose", "2fa")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a 2FA session token and returns the user ID.
    /// </summary>
    public Guid? ValidateTwoFactorSessionToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        var purpose = principal.FindFirst("purpose")?.Value;
        if (purpose != "2fa") return null;

        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub == null || !Guid.TryParse(sub, out var userId)) return null;

        return userId;
    }

    private JwtSettings GetJwtSettings()
    {
        var section = _configuration.GetSection("Jwt");

        var issuer = section["Issuer"];
        var audience = section["Audience"];
        var secretKey = section["SecretKey"];

        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("JWT Issuer is not configured.");

        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("JWT Audience is not configured.");

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured.");

        if (secretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters (256 bits).");

        var userTokenLifetime = section.GetValue<int?>("UserTokenLifetimeMinutes") ?? 30;
        var rememberMeLifetime = section.GetValue<int?>("RememberMeTokenLifetimeMinutes") ?? 43200; // 30 days
        var systemTokenLifetime = section.GetValue<int?>("SystemTokenLifetimeHours") ?? 1;

        return new JwtSettings
        {
            Issuer = issuer,
            Audience = audience,
            SecretKey = secretKey,
            UserTokenLifetimeMinutes = userTokenLifetime,
            RememberMeTokenLifetimeMinutes = rememberMeLifetime,
            SystemTokenLifetimeHours = systemTokenLifetime
        };
    }

    private sealed class JwtSettings
    {
        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public required string SecretKey { get; init; }
        public required int UserTokenLifetimeMinutes { get; init; }
        public required int RememberMeTokenLifetimeMinutes { get; init; }
        public required int SystemTokenLifetimeHours { get; init; }
    }
}
