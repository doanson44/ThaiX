using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Serilog;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.BackgroundJobs;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.Services.Notifications;
using ThaiX.Presentation.Endpoints;
using ThaiX.Presentation.Middleware;

namespace ThaiX.Presentation.Extensions;

/// <summary>
/// Extension methods for configuring the HTTP middleware pipeline and terminal features.
/// Extracted from top-level statements for readability; no behavioral changes.
/// </summary>
public static class WebApplicationExtensions
{
    private delegate void ScheduleJobAction(string jobId, string queue, string cronExpression, RecurringJobOptions options);

    private static void ScheduleRecurringJob<T>(
        string jobId,
        string queue,
        Expression<Action<T>> methodCall,
        string cronExpression,
        RecurringJobOptions options)
    {
        var supportsCustomQueue = false;
        try
        {
            supportsCustomQueue = JobStorage.Current?.HasFeature(JobStorageFeatures.ExtendedApi) == true;
        }
        catch
        {
            // Storage may not be initialized or does not expose feature queries
        }

        if (supportsCustomQueue &&
            !string.IsNullOrWhiteSpace(queue) &&
            !string.Equals(queue, "default", StringComparison.OrdinalIgnoreCase))
        {
            RecurringJob.AddOrUpdate<T>(jobId, queue, methodCall, cronExpression, options);
        }
        else
        {
            RecurringJob.AddOrUpdate<T>(jobId, methodCall, cronExpression, options);
        }
    }

    private static readonly Dictionary<string, ScheduleJobAction> JobSchedulers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [HangfireJobIds.ProcessOutbox] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<OutboxProcessorJob>(
                    jobId,
                    queue,
                    job => job.ProcessPendingMessages(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.CleanupOldOutboxMessages] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<OutboxProcessorJob>(
                    jobId,
                    queue,
                    job => job.CleanupOldMessages(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.ChainBrokerFundsWeeklySyncJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<ChainBrokerFundsSyncJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.ChainBrokerProjectsWeeklySyncJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<ChainBrokerProjectsSyncJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.ChainBrokerUnlocksWeeklySyncJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<ChainBrokerUnlocksSyncJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.IWealthClubTop10DailySyncJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<IWealthClubTop10SyncJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.Power655SyncJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<Power655SyncJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.Power655PredictJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<Power655PredictJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.TwentyFourHMoneyTransactionDailySyncJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<TwentyFourHMoneyTransactionSyncJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.TopStocksWeeklySuggestionJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<TopStocksWeeklySuggestionJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.MexcSpotWeeklySuggestionJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<MexcSpotWeeklySuggestionJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.MexcMarketScannerJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<MexcMarketScannerJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.PriceAlertCheckerJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<PriceAlertCheckerJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.ScheduledPostPublisherJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<ScheduledPostPublisherJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.PollDueNotificationSchedules] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<DueSchedulePoller>(
                    jobId,
                    queue,
                    job => job.ExecuteAsync(CancellationToken.None),
                    cron,
                    options),

            [HangfireJobIds.JsonBinExpiredCleanupJob] = (jobId, queue, cron, options) =>
                ScheduleRecurringJob<JsonBinExpiredCleanupJob>(
                    jobId,
                    queue,
                    job => job.RunAsync(CancellationToken.None),
                    cron,
                    options),
        };

    /// <summary>
    /// Configures the full HTTP middleware pipeline in the correct order.
    /// </summary>
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        var requestLocalizationOptions = app.Services
            .GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value;
        app.UseRequestLocalization(requestLocalizationOptions);

        app.UseCorrelationId();
        app.UseSerilogRequestLogging();
        app.UseGlobalExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // CORS must be before Authentication so preflight OPTIONS get proper headers
        app.UseCors("ClientPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        ConfigureStaticFileStorage(app);
        ConfigureSwagger(app);

        var hangfireConfig = BindHangfireConfiguration(app.Configuration);
        ConfigureHangfireDashboard(app, hangfireConfig);

        return app;
    }

    /// <summary>
    /// Maps all API endpoint groups.
    /// </summary>
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapAuthenticationEndpoints();
        app.MapTokenEndpoints();
        app.MapAccountEndpoints();
        app.MapHealthEndpoints();
        app.MapUserEndpoints();
        app.MapApiClientEndpoints();
        app.MapMasterDataEndpoints();
        app.MapContactEndpoints();
        app.MapFileEndpoints();
        app.MapExternalDataEndpoints();
        app.MapAiEndpoints();
        app.MapSlackEndpoints();
        app.MapTelegramEndpoints();
        app.MapBotCommandEndpoints();
        app.MapNotificationEndpoints();
        app.MapNotificationScheduleEndpoints();
        app.MapTradingEndpoints();
        app.MapMarketScannerEndpoints();
        app.MapPriceAlertEndpoints();
        app.MapPortfolioEndpoints();
        app.MapCryptoPositionEndpoints();
        app.MapStockPositionEndpoints();
        app.MapSavingPositionEndpoints();
        app.MapNoteEndpoints();
        app.MapCredentialAccountEndpoints();
        app.MapResumeEndpoints();
        app.MapResumeAiEndpoints();
        app.MapBlogPostEndpoints();
        app.MapBlogCategoryEndpoints();
        app.MapBlogTagEndpoints();
        app.MapBlogAiEndpoints();
        app.MapLotteryEndpoints();
        app.MapJsonBinEndpoints();
        app.MapJsonBinPublicEndpoints();
        app.MapExpenseTrackerEndpoints();

        return app;
    }

    /// <summary>
    /// Schedules Hangfire recurring jobs based on configuration.
    /// </summary>
    public static WebApplication ScheduleHangfireJobs(this WebApplication app)
    {
        var hangfireConfig = BindHangfireConfiguration(app.Configuration);

        if (!hangfireConfig.Enabled || !hangfireConfig.Server.Enabled)
        {
            return app;
        }

        try
        {
            RemoveStaleRecurringJobs(hangfireConfig);
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Hangfire: failed to remove stale recurring jobs during startup. Continuing without cleanup.");
        }

        var timeZone = app.Services.GetRequiredService<IDateTimeProvider>().TimeZone;
        var recurringJobOptions = new RecurringJobOptions { TimeZone = timeZone };

        foreach (var (jobId, jobConfig) in hangfireConfig.RecurringJobs)
        {
            if (!jobConfig.Enabled)
            {
                Log.Information("Hangfire recurring job '{JobId}' is disabled", jobId);
                continue;
            }

            if (!JobSchedulers.TryGetValue(jobId, out var schedule))
            {
                Log.Warning("Unknown Hangfire recurring job: {JobId}", jobId);
                continue;
            }

            try
            {
                schedule(jobId, jobConfig.Queue, jobConfig.CronExpression, recurringJobOptions);
                Log.Information(
                    "Hangfire recurring job '{JobId}' scheduled: {Cron}, Queue: {Queue}, TimeZone: {TimeZone}",
                    jobId, jobConfig.CronExpression, jobConfig.Queue, timeZone.Id);
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Hangfire: failed to schedule recurring job '{JobId}'. Continuing startup.",
                    jobId);
            }
        }

        return app;
    }

    #region Private Helpers

    private static void ConfigureStaticFileStorage(WebApplication app)
    {
        var env = app.Services.GetRequiredService<IWebHostEnvironment>();
        var storageOptions = app.Services.GetRequiredService<IOptions<StorageSettings>>().Value;
        var storagePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, storageOptions.RootPath));

        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
        }

        var requestPath = string.IsNullOrEmpty(storageOptions.BaseUrl)
            ? "/files"
            : storageOptions.BaseUrl.TrimEnd('/');

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storagePath),
            RequestPath = requestPath
        });
    }

    private static void RemoveStaleRecurringJobs(HangfireConfiguration hangfireConfig)
    {
        var configuredJobIds = new HashSet<string>(
            hangfireConfig.RecurringJobs.Keys,
            StringComparer.OrdinalIgnoreCase);

        using var connection = JobStorage.Current.GetConnection();
        foreach (var job in connection.GetRecurringJobs())
        {
            if (configuredJobIds.Contains(job.Id))
            {
                continue;
            }

            RecurringJob.RemoveIfExists(job.Id);
            Log.Information("Hangfire: removed stale recurring job '{JobId}'", job.Id);
        }
    }

    private static void ConfigureSwagger(WebApplication app)
    {
        var swaggerEnabled = app.Environment.IsDevelopment() ||
                            app.Configuration.GetValue("Swagger:Enabled", false);

        if (!swaggerEnabled)
        {
            return;
        }

        app.UseSwaggerAuthorization();

        var requiredPermission = app.Configuration.GetValue<string>("Swagger:RequiredPermission")
                                ?? Domain.Common.Constants.Permissions.SwaggerView;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ThaiX API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "ThaiX API Documentation";
            options.DefaultModelsExpandDepth(-1);
        });

        Log.Information(
            "Swagger UI configured at /swagger (RequiredPermission: {Permission})",
            requiredPermission);
    }

    private static void ConfigureHangfireDashboard(WebApplication app, HangfireConfiguration hangfireConfig)
    {
        if (!hangfireConfig.Enabled || !hangfireConfig.Dashboard.Enabled)
        {
            return;
        }

        app.UseHangfireAuthorization();

        var dashboardOptions = new DashboardOptions
        {
            Authorization = new[]
            {
                new HangfireDashboardAuthorizationFilter(
                    hangfireConfig.Dashboard.RequiredPermission,
                    isDevelopment: app.Environment.IsDevelopment())
            },
            AppPath = hangfireConfig.Dashboard.AppPath,
            StatsPollingInterval = hangfireConfig.Dashboard.StatsPollingInterval,
            DisplayStorageConnectionString = hangfireConfig.Dashboard.DisplayStorageConnectionString,
            IsReadOnlyFunc = _ => hangfireConfig.Dashboard.ReadOnly
        };

        app.UseHangfireDashboard(hangfireConfig.Dashboard.Path, dashboardOptions);

        Log.Information(
            "Hangfire dashboard configured at {Path} (ReadOnly: {ReadOnly}, RequiredPermission: {Permission})",
            hangfireConfig.Dashboard.Path,
            hangfireConfig.Dashboard.ReadOnly,
            hangfireConfig.Dashboard.RequiredPermission);
    }

    private static HangfireConfiguration BindHangfireConfiguration(IConfiguration configuration)
    {
        var hangfireConfig = new HangfireConfiguration();
        configuration.GetSection(HangfireConfiguration.SectionName).Bind(hangfireConfig);
        return hangfireConfig;
    }

    #endregion
}
