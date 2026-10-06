using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using ThaiX.Application.Common.Constants;
using ThaiX.Infrastructure.Identity;
using ThaiX.Presentation.OpenApi;

namespace ThaiX.Presentation.Extensions;

/// <summary>
/// Extension methods for configuring DI services in Program.cs.
/// Extracted from top-level statements for readability; no behavioral changes.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds request localization services using configuration from "Localization" section.
    /// </summary>
    public static IServiceCollection AddRequestLocalization(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        var localizationSection = configuration.GetSection("Localization");
        var defaultCulture = localizationSection.GetValue<string>("DefaultCulture") ?? "en-US";
        var supportedCultureNames = localizationSection.GetSection("SupportedCultures").Get<string[]>()
            ?? ["en-US", "vi-VN"];
        var supportedCultures = supportedCultureNames
            .Select(c => new CultureInfo(c))
            .ToList();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.RequestCultureProviders =
            [
                new AcceptLanguageHeaderRequestCultureProvider(),
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider()
            ];
        });

        return services;
    }

    /// <summary>
    /// Adds CORS policy "ClientPolicy" using origins from "AllowedOrigins" configuration.
    /// </summary>
    public static IServiceCollection AddClientCors(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("ClientPolicy", policy =>
            {
                var clientOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
                    ?? new[] { "https://localhost:7129", "http://localhost:5201" };

                policy.WithOrigins(clientOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Adds permission-based authorization policies from Domain constants.
    /// </summary>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            var permissions = Domain.Common.Constants.Permissions.GetAll();
            foreach (var permission in permissions)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(
                        new Infrastructure.Security.PermissionRequirement(permission)));
            }
        });

        return services;
    }

    /// <summary>
    /// Adds JWT Bearer authentication and optional Google external login.
    /// All token validation parameters and event handlers are preserved exactly.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        // Cookie scheme used only for external login (Google) callback sign-in. Not used for API.
        authBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "ThaiX.ExternalLogin";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            options.SlidingExpiration = true;
        });

        // Add Google external login
        var googleConfig = configuration.GetSection("Authentication:Google");
        if (!string.IsNullOrEmpty(googleConfig["ClientId"]) &&
            googleConfig["ClientId"] != "YOUR_GOOGLE_CLIENT_ID")
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleConfig["ClientId"]!;
                options.ClientSecret = googleConfig["ClientSecret"]!;
                options.CallbackPath = "/api/account/signin-google";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
        }

        // Add JWT Bearer
        authBuilder.AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var secretKey = jwtSettings["SecretKey"]!;

            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT SecretKey must be configured and at least 32 characters (256 bits). " +
                    "Use environment variables or Azure Key Vault in production.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudiences = new[] { audience },

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,

                NameClaimType = ClaimTypes.NameIdentifier
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var principal = context.Principal!;
                    var identity = (ClaimsIdentity)principal.Identity!;
                    var scopeValues = principal.FindAll("scope")
                        .Select(c => c.Value)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .Distinct(StringComparer.Ordinal)
                        .ToList();

                    var existingPermissions = identity.FindAll(ClaimTypeConstants.Permission)
                        .Select(c => c.Value)
                        .ToHashSet(StringComparer.Ordinal);

                    foreach (var scopeValue in scopeValues)
                    {
                        if (existingPermissions.Contains(scopeValue))
                        {
                            continue;
                        }

                        identity.AddClaim(new Claim(ClaimTypeConstants.Permission, scopeValue));
                        existingPermissions.Add(scopeValue);
                    }

                    var actorType = principal.FindFirst("actor_type")?.Value;

                    if (actorType == "system")
                    {
                        if (principal.HasClaim(c =>
                            c.Type == ClaimTypes.Email ||
                            c.Type == "email"))
                        {
                            context.Fail("System token cannot contain user claims (email)");
                            Log.Warning("Rejected system token with user claims: {ClientId}",
                                principal.FindFirst("client_id")?.Value);
                            return Task.CompletedTask;
                        }

                        Log.Information(
                            "System token validated: ClientId={ClientId}, Scopes={Scopes}",
                            principal.FindFirst("client_id")?.Value,
                            string.Join(", ", scopeValues));
                    }
                    else if (actorType == "user")
                    {
                        var sub = principal.FindFirst("sub")?.Value
                            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var email = principal.FindFirst(ClaimTypes.Email)?.Value
                            ?? principal.FindFirst("email")?.Value;

                        if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email))
                        {
                            context.Fail("User token missing required claims (sub, email)");
                            Log.Warning("Rejected user token with missing claims");
                            return Task.CompletedTask;
                        }

                        Log.Information(
                            "User token validated: UserId={UserId}, Email={Email}, Scopes={Scopes}",
                            sub, email, string.Join(", ", scopeValues));
                    }
                    else
                    {
                        context.Fail("Token missing actor_type claim or invalid value");
                        Log.Warning("Rejected token with invalid actor_type: {ActorType}", actorType);
                        return Task.CompletedTask;
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT authentication failed: {Error}, Exception: {Exception}",
                        context.Exception.Message,
                        context.Exception.GetType().Name);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Log.Debug("JWT authentication challenged: {Error}, {ErrorDescription}",
                        context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };

            options.RequireHttpsMetadata = !isDevelopment;
        });

        return services;
    }

    /// <summary>
    /// Adds Swagger/OpenAPI generation services with JWT Bearer security definition.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ThaiX API",
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Enter your token directly.",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(document =>
            {
                var schemeRef = new OpenApiSecuritySchemeReference("Bearer", document);
                return new OpenApiSecurityRequirement
                {
                    [schemeRef] = new List<string>()
                };
            });

            c.OperationFilter<AnonymousOperationSecurityFilter>();
        });

        return services;
    }

    /// <summary>
    /// Adds ASP.NET Core Identity for ApplicationUser/ApplicationRole with EF stores.
    /// </summary>
    public static IServiceCollection AddApplicationIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<Infrastructure.Persistence.ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// Configures the HTTP JSON serializer used by Minimal API endpoints to serialize and
    /// deserialize enum values as PascalCase strings (e.g. "PricePump" instead of 0).
    /// </summary>
    public static IServiceCollection AddHttpJsonStringEnums(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}
