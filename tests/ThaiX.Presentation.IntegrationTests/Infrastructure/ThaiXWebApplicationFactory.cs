using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Testcontainers.MariaDb;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Infrastructure.BackgroundJobs;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.Persistence;
using ThaiX.Infrastructure.Persistence.Interceptors;
using ThaiX.Infrastructure.Services.Notifications;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

/// <summary>
/// Custom <see cref="WebApplicationFactory{TEntryPoint}"/> that spins up a
/// MariaDB container via Testcontainers and replaces the connection string.
/// Implements <see cref="IAsyncLifetime"/> so xUnit manages its lifecycle.
/// </summary>
public sealed class ThaiXWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MariaDbContainer _dbContainer = new MariaDbBuilder("mariadb:11.4")
        .WithCommand("--character-set-server=utf8mb4", "--collation-server=utf8mb4_unicode_ci")
        .WithPassword("Test@Passw0rd!")
        .Build();

    /// <summary>
    /// The connection string for the Testcontainers MariaDB instance.
    /// Available after <see cref="InitializeAsync"/> completes.
    /// </summary>
    public string ConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        Environment.SetEnvironmentVariable("Hangfire__Enabled", "false");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _dbContainer.GetConnectionString());

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();

            // Use the test project's output directory to find appsettings.Testing.json
            var testAssemblyPath = Path.GetDirectoryName(typeof(ThaiXWebApplicationFactory).Assembly.Location)!;
            config.SetBasePath(testAssemblyPath);
            config.AddJsonFile("appsettings.Testing.json", optional: false);

            // Override connection string with Testcontainers-provided one
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString()
            });
        });

        builder.ConfigureServices((context, services) =>
        {
            // Replace the real DbContext registration with Testcontainers connection
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IApplicationDbContext>();

            services.AddSingleton<MariaDbCommandInterceptor>();

            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseMySQL(_dbContainer.GetConnectionString())
                    .ReplaceService<IHistoryRepository, MariaDbHistoryRepository>();
                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
                    serviceProvider.GetRequiredService<OutboxInterceptor>(),
                    serviceProvider.GetRequiredService<MariaDbCommandInterceptor>());
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // Replace email sender with a no-op for tests
            services.RemoveAll<IEmailSender>();
            services.AddScoped<IEmailSender, FakeEmailSender>();

            // Replace AI generation with a deterministic fake for integration tests
            services.RemoveAll<IAiTextGenerationService>();
            services.AddScoped<IAiTextGenerationService, FakeAiTextGenerationService>();

            // Replace Slack notification with a no-op for deterministic tests.
            services.RemoveAll<ISlackNotificationService>();
            services.AddScoped<ISlackNotificationService, FakeSlackNotificationService>();

            // Replace Telegram notification with a no-op for deterministic tests.
            services.RemoveAll<ITelegramNotificationService>();
            services.AddScoped<ITelegramNotificationService, FakeTelegramNotificationService>();

            // Hangfire is disabled in test config, but many endpoints/jobs still depend on IBackgroundJobClient.
            services.RemoveAll<IBackgroundJobClient>();
            services.AddSingleton<IBackgroundJobClient, FakeBackgroundJobClient>();
            services.RemoveAll<IHangfireJobState>();
            services.AddSingleton<IHangfireJobState, AlwaysEnabledHangfireJobState>();

            // Ensure background jobs and related services are resolvable in testing.
            services.AddScoped<OutboxProcessorJob>();
            services.AddScoped<NotificationDeliveryJob>();
            services.AddScoped<ContactImportJobProcessor>();
            services.AddScoped<ContactImportJobScheduler>();
            services.AddScoped<IContactImportJobScheduler, ContactImportJobScheduler>();
            services.AddScoped<ChainBrokerFundsSyncJob>();
            services.AddScoped<ChainBrokerProjectsSyncJob>();
            services.AddScoped<ChainBrokerUnlocksSyncJob>();
            services.AddScoped<IWealthClubTop10SyncJob>();
            services.AddScoped<TopStocksWeeklySuggestionJob>();
            services.AddScoped<MexcSpotWeeklySuggestionJob>();
            services.AddScoped<MexcMarketScannerJob>();
            services.AddScoped<PriceAlertCheckerJob>();
            services.AddScoped<BotTradeSuggestionAsyncJob>();
            services.AddScoped<PersistWeeklySuggestionReportJob>();
            services.AddScoped<ScheduledPostPublisherJob>();
            services.AddScoped<TwentyFourHMoneyTransactionSyncJob>();
            services.AddScoped<Power655SyncJob>();
            services.AddScoped<Power655PredictJob>();

            services.Configure<MarketScannerOptions>(
                context.Configuration.GetSection(MarketScannerOptions.SectionName));
            services.AddSingleton<IMarketSnapshotCache, FakeMarketSnapshotCache>();
            services.AddSingleton<IMarketSignalCooldownService, FakeMarketSignalCooldownService>();
            services.AddTransient<IMarketSignalDetector, FakeMarketSignalDetector>();

            services.Configure<PriceAlertCheckerOptions>(
                context.Configuration.GetSection(PriceAlertCheckerOptions.SectionName));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Ensure the database schema is created before the test host starts.
        // This avoids forcing WebApplicationFactory to build its internal host during fixture setup.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySQL(_dbContainer.GetConnectionString())
            .ReplaceService<IHistoryRepository, MariaDbHistoryRepository>()
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var dbContext = new ApplicationDbContext(options, NullLogger<ApplicationDbContext>.Instance);
        await dbContext.Database.ExecuteSqlRawAsync("ALTER DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("Hangfire__Enabled", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }

    private sealed class AlwaysEnabledHangfireJobState : IHangfireJobState
    {
        public bool IsEnabled(string jobId) => true;
    }
}
