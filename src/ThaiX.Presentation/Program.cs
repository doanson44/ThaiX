using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using ThaiX.Application;
using ThaiX.Infrastructure;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Endpoints;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting ThaiX.Presentation application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // ---------- Service Registration ----------

    builder.Services.AddRequestLocalization(builder.Configuration);
    builder.Services.AddClientCors(builder.Configuration);
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;

        var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
        foreach (var proxy in knownProxies.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            if (!IPAddress.TryParse(proxy, out var address))
            {
                throw new InvalidOperationException($"ForwardedHeaders:KnownProxies contains an invalid IP address: '{proxy}'.");
            }

            options.KnownProxies.Add(address);
        }
    });
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHealthChecks();
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("EmailOtpRequest", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
        options.AddPolicy("EmailOtpVerify", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(5),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    });
    builder.Services.AddPermissionAuthorization();
    builder.Services.AddHttpJsonStringEnums();
    builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment.IsDevelopment());
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddSwaggerDocumentation();

    // Add Application and Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure Identity
    builder.Services.AddApplicationIdentity();

    // ---------- Build & Configure Pipeline ----------

    var app = builder.Build();

    // Initialize database (apply migrations and seed data)
    await ThaiX.Infrastructure.Persistence.DbInitializer.InitializeAsync(app.Services);

    // Configure the HTTP middleware pipeline
    app.ConfigureMiddlewarePipeline();
    app.UseRateLimiter();

    app.MapStaticAssets();

    // Map all API endpoints
    app.MapApiEndpoints();
    app.MapPublicConfigurationEndpoints();

    // SPA fallback for client-side routing
    app.MapFallbackToFile("index.html");

    // Schedule Hangfire recurring jobs after the application has started.
    // Scheduling is best-effort and must not block or fail application startup.
    app.Lifetime.ApplicationStarted.Register(() => app.ScheduleHangfireJobs());

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ThaiX.Presentation application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program;
