using Serilog;
using ThaiX.Application;
using ThaiX.Infrastructure;
using ThaiX.Presentation.Extensions;

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
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHealthChecks();
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

    app.MapStaticAssets();

    // Map all API endpoints
    app.MapApiEndpoints();

    // SPA fallback for client-side routing
    app.MapFallbackToFile("index.html");

    // Schedule Hangfire recurring jobs (if enabled)
    app.ScheduleHangfireJobs();

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
