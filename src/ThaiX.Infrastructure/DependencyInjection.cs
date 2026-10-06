using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Configuration;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Trading;
using ThaiX.Infrastructure.BackgroundJobs;
using ThaiX.Infrastructure.Caching;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;
using ThaiX.Infrastructure.ExternalApis.Providers;
using ThaiX.Infrastructure.ExternalApis.Resilience;
using ThaiX.Infrastructure.ExternalApis.Transport;
using ThaiX.Infrastructure.Identity;
using ThaiX.Infrastructure.Import.Csv;
using ThaiX.Infrastructure.Persistence;
using ThaiX.Infrastructure.Persistence.Interceptors;
using ThaiX.Infrastructure.Security;
using ThaiX.Infrastructure.Services;
using ThaiX.Infrastructure.Services.Ai;
using ThaiX.Infrastructure.Services.BotCommands;
using ThaiX.Infrastructure.Services.Indicators;
using ThaiX.Infrastructure.Services.MarketData;
using ThaiX.Infrastructure.Services.MarketScanner;
using ThaiX.Infrastructure.Services.MarketScanner.Detectors;
using ThaiX.Infrastructure.Services.Notifications;

namespace ThaiX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Interceptors
        services
            .AddScoped<AuditableEntityInterceptor>()
            .AddScoped<OutboxInterceptor>()
            .AddSingleton<MariaDbCommandInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseMySQL(connectionString)
                .ReplaceService<IHistoryRepository, MariaDbHistoryRepository>();

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<MariaDbCommandInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Options
        services.AddOptions<EmailSettings>()
            .BindConfiguration(EmailSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SlackSettings>()
            .BindConfiguration(SlackSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<TelegramSettings>()
            .BindConfiguration(TelegramSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<NotificationSettings>()
            .BindConfiguration(NotificationSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<BotCommandSettings>()
            .BindConfiguration(BotCommandSettings.SectionName)
            .ValidateOnStart();

        services.AddOptions<AiSettings>()
            .BindConfiguration(AiSettings.SectionName)
            .ValidateDataAnnotations()
            .Validate(static x => x.Providers.Count > 0, "At least one AI provider must be configured.")
            .ValidateOnStart();

        services.AddOptions<LiquidityScoringOptions>()
            .BindConfiguration(LiquidityScoringOptions.SectionName);

        services.Configure<NotificationSchedulingSettings>(
            configuration.GetSection(NotificationSchedulingSettings.SectionName));

        services.Configure<StorageSettings>(
            configuration.GetSection(StorageSettings.SectionName));

        services.Configure<ExternalApisOptions>(
            configuration.GetSection(ExternalApisOptions.SectionName));

        services.Configure<ProxyOptions>(
            configuration.GetSection(ProxyOptions.SectionName));

        // Core services
        services
            .AddScoped<IJsonBinService, JsonBinService>()
            .AddSingleton<IEncryptionService, AesEncryptionService>()
            .AddSingleton<ICredentialPasswordEncryptionService, CredentialPasswordEncryptionService>()
            .AddSingleton<IDateTimeProvider, DateTimeProviderService>()
            .AddScoped<ICurrentUserService, CurrentUserService>()
            .AddScoped<IAuthenticationService, Authentication.AuthenticationService>()
            .AddScoped<IIdentityUserService, Services.IdentityUserService>()
            .AddScoped<Identity.IdentityUserService>()
            .AddScoped<ApiClientService>()
            .AddScoped<JwtTokenService>();

        // AI
        services
            .AddScoped<IAiTextGenerationService, FallbackAiTextGenerationService>();

        // Notification
        services
            .AddScoped<IEmailSender, GmailEmailSender>()
            .AddScoped<INotificationRouter, NotificationRouter>()
            .AddScoped<INotificationDeliveryScheduler, HangfireNotificationDeliveryScheduler>()
            .AddScoped<INotificationRouteResolver, NotificationRouteResolver>()
            .AddSingleton<INotificationTemplateProvider, DefaultNotificationTemplateProvider>();

        services.AddHttpClient<ISlackNotificationService, SlackNotificationService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<SlackSettings>>().Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.BotToken);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddHttpClient<ITelegramNotificationService, TelegramNotificationService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // Notification formatters
        services
            .AddScoped<SlackNotificationFormatter>()
            .AddScoped<TelegramNotificationFormatter>()
            .AddScoped<DiscordNotificationFormatter>()
            .AddScoped<EmailNotificationFormatter>()
            .AddScoped<WebPushNotificationFormatter>()
            .AddScoped<INotificationFormatter>(sp => sp.GetRequiredService<SlackNotificationFormatter>())
            .AddScoped<INotificationFormatter>(sp => sp.GetRequiredService<TelegramNotificationFormatter>())
            .AddScoped<INotificationFormatter>(sp => sp.GetRequiredService<DiscordNotificationFormatter>())
            .AddScoped<INotificationFormatter>(sp => sp.GetRequiredService<EmailNotificationFormatter>())
            .AddScoped<INotificationFormatter>(sp => sp.GetRequiredService<WebPushNotificationFormatter>())
            .AddScoped<INotificationRenderer, SlackNotificationRenderer>()
            .AddScoped<INotificationRenderer, TelegramNotificationRenderer>()
            .AddScoped<INotificationChannelProvider, SlackNotificationChannelProvider>()
            .AddScoped<INotificationChannelProvider, TelegramNotificationChannelProvider>()
            .AddScoped<Application.Features.Notifications.Scheduling.ISchedulingCalculator, Application.Features.Notifications.Scheduling.SchedulingCalculator>()
            .AddScoped<DueSchedulePoller>()
            .AddScoped<NotificationDeliveryJob>();

        // Bot
        services
            .AddScoped<IBotUserMappingService, BotUserMappingService>()
            .AddSingleton<IBotAsyncExecutionStateStore, InMemoryBotAsyncExecutionStateStore>()
            .AddScoped<IBotAsyncExecutionService, HangfireBotAsyncExecutionService>();

        // Authorization
        services
            .AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Import
        services
            .AddScoped<IContactCsvParser, ContactCsvParser>();

        // Storage
        services
            .AddSingleton<IFileStorage, LocalFileStorage>()
            .AddSingleton<IFileUrlProvider, StorageFileUrlProvider>();

        // Cache
        services
            .AddMemoryCache()
            .AddSingleton<MemoryCacheGroupManager>()
            .AddSingleton<ICacheService, MemoryCacheService>();

        // External API
        services
            .AddSingleton<ExternalApiCacheKeyBuilder>()
            .AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ExternalApisOptions>>().Value;
                return new ExternalApiRateLimiter(options.MaxConcurrentPerHost);
            })
            .AddTransient<ExternalApiResilienceHandler>();

        services.AddHttpClient<HttpTransport>()
            .AddHttpMessageHandler<ExternalApiResilienceHandler>();

        services.AddHttpClient<ProxyTransport>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ProxyOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        })
        .AddHttpMessageHandler<ExternalApiResilienceHandler>();

        services
            .AddScoped<IApiTransport, ApiTransportRouter>()
            .AddScoped<ExternalApiService>()
            .AddScoped<CafeFApiProvider>()
            .AddScoped<BinanceApiProvider>()
            .AddScoped<BybitApiProvider>()
            .AddScoped<DragonCapitalApiProvider>()
            .AddScoped<SacombankApiProvider>()
            .AddScoped<YahooFinanceApiProvider>()
            .AddScoped<CoinGeckoApiProvider>()
            .AddScoped<MexcApiProvider>()
            .AddScoped<IWealthClubApiProvider>()
            .AddScoped<VnDirectApiProvider>()
            .AddScoped<TwentyFourHMoneyApiProvider>()
            .AddScoped<ChainBrokerApiProvider>()
            .AddScoped<KetQuaDienToanApiProvider>()
            .AddScoped<VnExpressApiProvider>()
            .AddScoped<IExternalDataService, ExternalDataService>()
            .AddScoped<IIndicatorService, IndicatorService>()
            .AddScoped<IExternalMarketDataService, ExternalMarketDataService>();

        AddHangfire(services, configuration, connectionString);

        return services;
    }

    private static void AddHangfire(
        IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.Configure<HangfireConfiguration>(configuration.GetSection(HangfireConfiguration.SectionName));

        var hangfireConfig = new HangfireConfiguration();
        configuration.GetSection(HangfireConfiguration.SectionName).Bind(hangfireConfig);

        if (!hangfireConfig.Enabled)
        {
            return;
        }

        // Register Hangfire services
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseFilter(new AutomaticRetryAttribute
            {
                Attempts = hangfireConfig.RetryAttempts,
                LogEvents = true
            })
            .UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
            {
                QueuePollInterval = hangfireConfig.Storage.QueuePollInterval,
                JobExpirationCheckInterval = hangfireConfig.Storage.JobExpirationCheckInterval,
                CountersAggregateInterval = hangfireConfig.Storage.CountersAggregateInterval,
                PrepareSchemaIfNecessary = hangfireConfig.Storage.PrepareSchemaIfNecessary,
                DashboardJobListLimit = hangfireConfig.Storage.DashboardJobListLimit,
                TransactionTimeout = hangfireConfig.Storage.TransactionTimeout,
                TablesPrefix = "Hangfire_"
            })));

        // Register Hangfire server (if enabled)
        if (hangfireConfig.Server.Enabled)
        {
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = hangfireConfig.Server.WorkerCount;
                options.Queues = hangfireConfig.Server.Queues;
                options.ShutdownTimeout = hangfireConfig.Server.ShutdownTimeout;
            });
        }

        // Register background jobs
        services.AddSingleton<IHangfireJobState, HangfireJobState>();
        services.AddScoped<OutboxProcessorJob>();
        services.AddScoped<ContactImportJobProcessor>();
        services.AddScoped<IContactImportJobScheduler, ContactImportJobScheduler>();
        services.AddScoped<ChainBrokerFundsSyncJob>();
        services.AddScoped<ChainBrokerProjectsSyncJob>();
        services.AddScoped<ChainBrokerUnlocksSyncJob>();
        services.AddScoped<IWealthClubTop10SyncJob>();
        services.AddScoped<TwentyFourHMoneyTransactionSyncJob>();
        services.AddScoped<Power655SyncJob>();
        services.AddScoped<Power655PredictJob>();
        services.AddScoped<TopStocksWeeklySuggestionJob>();
        services.AddScoped<MexcSpotWeeklySuggestionJob>();

        // Market scanner
        services.Configure<MarketScannerOptions>(configuration.GetSection(MarketScannerOptions.SectionName));
        services.AddSingleton<IMarketSnapshotCache, InMemoryMarketSnapshotCache>();
        services.AddSingleton<IMarketSignalCooldownService, InMemoryMarketSignalCooldownService>();
        services.AddTransient<IMarketSignalDetector, PricePumpDetector>();
        services.AddTransient<IMarketSignalDetector, FundingFlipDetector>();
        services.AddTransient<IMarketSignalDetector, FundingMultipleDetector>();
        services.AddTransient<IMarketSignalDetector, VolumeSpikeDetector>();
        services.AddScoped<MexcMarketScannerJob>();

        // Price alert checker
        services.Configure<PriceAlertCheckerOptions>(configuration.GetSection(PriceAlertCheckerOptions.SectionName));
        services.Configure<AffiliateOptions>(configuration.GetSection(AffiliateOptions.SectionName));
        services.AddScoped<PriceAlertCheckerJob>();
        services.AddScoped<BotTradeSuggestionAsyncJob>();

        // Blog
        services.AddScoped<ScheduledPostPublisherJob>();

        // JsonBin
        services.AddScoped<JsonBinExpiredCleanupJob>();
    }
}

