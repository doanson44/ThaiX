using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.ChainBroker;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.ExternalApis.Providers;
using ThaiX.Infrastructure.Persistence;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that syncs ChainBroker funds data daily at 01:00 VNT (UTC+7).
/// Fetches all pages (following "next" until null) and performs an upsert
/// using batches of 200 items, each with its own DbContext scope.
/// </summary>
public sealed class ChainBrokerFundsSyncJob : HangfireJobBase
{
    private const int BatchSize = 200;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ChainBrokerApiProvider _provider;
    private readonly INotificationRouter _notificationRouter;
    private readonly ICacheService _cache;
    private readonly ILogger<ChainBrokerFundsSyncJob> _logger;

    public ChainBrokerFundsSyncJob(
        IServiceScopeFactory scopeFactory,
        IHangfireJobState hangfireJobState,
        ChainBrokerApiProvider provider,
        INotificationRouter notificationRouter,
        ICacheService cache,
        ILogger<ChainBrokerFundsSyncJob> logger) : base(hangfireJobState, logger)
    {
        _scopeFactory = scopeFactory;
        _provider = provider;
        _notificationRouter = notificationRouter;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Entry point called by Hangfire. Syncs ChainBroker funds data.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.ChainBrokerFundsWeeklySyncJob)) return;

        using var _ = await AcquireJobLockAsync("sync:chainbroker", cancellationToken);

        _logger.LogInformation("ChainBrokerFundsSyncJob started");
        var started = DateTime.UtcNow;

        try
        {
            var (count, isComplete) = await SyncFundsAsync(cancellationToken);
            var elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;
            if (isComplete)
            {
                _logger.LogInformation("ChainBrokerFundsSyncJob completed in {Elapsed}s: {Count} records", elapsed, count);
            }
            else
            {
                _logger.LogWarning("ChainBrokerFundsSyncJob partially completed in {Elapsed}s: {Count} records; failed pages were skipped", elapsed, count);
            }

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "ChainBroker Funds Sync",
                    Type = NotificationCardType.Summary,
                    Severity = isComplete ? NotificationSeverity.Info : NotificationSeverity.Warning,
                    Metrics =
                    [
                        new() { Label = "Status", Value = isComplete ? "Success" : "Partial" },
                        new() { Label = "Duration", Value = $"{elapsed}s" },
                        new() { Label = "Records", Value = count.ToString("N0") }
                    ]
                }
            }, cancellationToken);

            await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerData, cancellationToken);
            _logger.LogDebug("ChainBrokerFundsSyncJob: cache group {Group} invalidated", CacheGroups.ChainBrokerData);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChainBrokerFundsSyncJob failed");

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Title = "ChainBroker Funds Sync Failed",
                Text = $"❌ ChainBroker funds sync failed: {ex.Message}",
                Severity = NotificationSeverity.Error
            }, CancellationToken.None);

            throw;
        }
    }

    private async Task<(int Count, bool IsComplete)> SyncFundsAsync(CancellationToken ct)
    {
        _logger.LogInformation("ChainBroker: fetching all funds pages from API");
        var apiItems = new List<InternalFundItem>();
        var isComplete = true;
        int page = 1;
        int? totalPages = null;
        string? nextUrl;
        do
        {
            var requestUrl = _provider.GetFundsListSyncUrl(page);
            InternalFundsResponse? response;
            try
            {
                _logger.LogDebug("ChainBroker funds: requesting page {Page}. Url={Url}", page, requestUrl);
                response = await _provider.GetFundsListSyncAsync<InternalFundsResponse>(page, ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                isComplete = false;
                _logger.LogWarning(ex, "ChainBroker funds: request failed on page {Page}. Url={Url}; skipping failed page", page, requestUrl);
                nextUrl = null;

                if (!totalPages.HasValue)
                {
                    _logger.LogWarning(
                        "ChainBroker funds: cannot determine remaining pages after page {Page} failed because total_pages was not available; stopping fetch",
                        page);
                    break;
                }

                page++;
                continue;
            }

            var list = response?.Data?.List;
            if (list is null || list.Results is null)
            {
                isComplete = false;
                _logger.LogWarning("ChainBroker funds: missing page data or results on page {Page}. Url={Url}; skipping page", page, requestUrl);
                nextUrl = null;

                if (!totalPages.HasValue)
                    break;

                page++;
                continue;
            }

            totalPages = list.TotalPages ?? totalPages;
            nextUrl = list.Next;

            if (list.Results.Count == 0)
            {
                isComplete = false;
                _logger.LogWarning("ChainBroker funds: empty results on page {Page}. Url={Url}; skipping page", page, requestUrl);
                nextUrl = null;

                if (!totalPages.HasValue)
                    break;

                page++;
                continue;
            }

            apiItems.AddRange(list.Results);
            _logger.LogDebug("ChainBroker funds: fetched page {Page} ({Count} records)", page, list.Results.Count);
            page++;
        } while (!string.IsNullOrEmpty(nextUrl) || (totalPages.HasValue && page <= totalPages.Value));

        if (apiItems.Count == 0)
        {
            _logger.LogWarning("ChainBroker funds: API returned no data — preserving existing data");
            return (0, false);
        }

        // --- Upsert in batches of BatchSize ---
        var validItems = apiItems
            .Where(r => !string.IsNullOrWhiteSpace(r.Slug))
            .GroupBy(r => r.Slug!, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToArray();

        int upserted = 0;
        foreach (var batch in validItems.Chunk(BatchSize))
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var batchSlugs = new HashSet<string>(batch.Select(r => r.Slug!), StringComparer.OrdinalIgnoreCase);

            var existing = await db.ChainBrokerFunds
                .Where(f => batchSlugs.Contains(f.Slug))
                .ToDictionaryAsync(f => f.Slug, StringComparer.OrdinalIgnoreCase, ct);

            foreach (var r in batch)
            {
                var snapshot = BuildSnapshot(r);
                if (existing.TryGetValue(snapshot.Slug, out var fund))
                    fund.Update(snapshot);
                else
                    db.ChainBrokerFunds.Add(ChainBrokerFund.Create(snapshot));
            }

            await db.SaveChangesAsync(ct);
            upserted += batch.Length;
            _logger.LogDebug("ChainBroker funds: upserted batch ({Done}/{Total})", upserted, validItems.Length);
        }

        // Never delete stale records from a partial API snapshot.
        if (!isComplete)
        {
            _logger.LogWarning("ChainBroker funds: skipped stale fund deletion because the API snapshot is incomplete");
            _logger.LogInformation("ChainBroker funds upsert complete: {Total} from API; partial snapshot", validItems.Length);
            return (validItems.Length, false);
        }

        // --- Remove stale funds ---
        var apiSlugSet = new HashSet<string>(validItems.Select(r => r.Slug!), StringComparer.OrdinalIgnoreCase);

        await using (var deleteScope = _scopeFactory.CreateAsyncScope())
        {
            var db = deleteScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var dbSlugs = await db.ChainBrokerFunds
                .Select(f => f.Slug)
                .ToListAsync(ct);

            var staleSlugs = dbSlugs
                .Where(s => !apiSlugSet.Contains(s))
                .ToList();

            if (staleSlugs.Count > 0)
            {
                foreach (var staleBatch in staleSlugs.Chunk(BatchSize))
                {
                    await db.ChainBrokerFunds
                        .Where(f => staleBatch.Contains(f.Slug))
                        .ExecuteDeleteAsync(ct);

                    _logger.LogDebug("ChainBroker funds: removed {Count} stale records", staleBatch.Length);
                }
            }
        }

        _logger.LogInformation("ChainBroker funds upsert complete: {Total} from API", validItems.Length);
        return (validItems.Length, true);
    }

    private static ChainBrokerFundSnapshot BuildSnapshot(InternalFundItem r) => new()
    {
        Slug = r.Slug!,
        Name = r.Name,
        Logo = r.Logo,
        FundTypeName = r.FundType?.Name,
        FundTypeSlug = r.FundType?.Slug,
        LastInvestment = r.LastInvestment,
        YearFounded = r.YearFounded,
        Status = r.Status,
        AverageCurrentRoi = r.AverageCurrentRoi,
        AverageMarketCap = r.AverageMarketCap,
        AverageInitialMarketCap = r.AverageInitialMarketCap,
        AverageFdmc = r.AverageFdmc,
        AverageInitialFdmc = r.AverageInitialFdmc,
        AveragePublicRaise = r.AveragePublicRaise,
        AveragePrivateRaise = r.AveragePrivateRaise,
        AverageTotalRaise = r.AverageTotalRaise,
        AveragePriceChange24h = r.AveragePriceChange24h,
        AveragePriceChange7d = r.AveragePriceChange7d,
        AveragePriceChange30d = r.AveragePriceChange30d,
        AveragePriceChange1y = r.AveragePriceChange1y,
        ProjectCount = r.ProjectCount ?? 0,
        GainersCount = ExtractGainersCount(r.GainersLosers?.Gainers),
        GainersPercent = ExtractPercent(r.GainersLosers?.Gainers),
        LosersCount = ExtractGainersCount(r.GainersLosers?.Losers),
        LosersPercent = ExtractPercent(r.GainersLosers?.Losers)
    };

    private static int? ExtractGainersCount(System.Text.Json.JsonElement[]? arr) =>
        arr is { Length: > 0 } && arr[0].TryGetInt32(out var v) ? v : null;

    private static string? ExtractPercent(System.Text.Json.JsonElement[]? arr) =>
        arr is { Length: > 1 } ? arr[1].GetString() : null;

    // --- Shared envelope ---

    private sealed record InternalPagedList<T>
    {
        [System.Text.Json.Serialization.JsonPropertyName("count")]
        public int? Count { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("next")]
        public string? Next { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("previous")]
        public string? Previous { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("total_pages")]
        public int? TotalPages { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("page_number")]
        public int? PageNumber { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("results")]
        public List<T>? Results { get; init; }
    }

    // --- Funds ---

    private sealed record InternalFundsResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public InternalFundsData? Data { get; init; }
    }

    private sealed record InternalFundsData
    {
        [System.Text.Json.Serialization.JsonPropertyName("list")]
        public InternalPagedList<InternalFundItem>? List { get; init; }
    }

    private sealed record InternalFundItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("slug")]
        public string? Slug { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("logo")]
        public string? Logo { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("fund_type")]
        public InternalFundType? FundType { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("last_investment")]
        public string? LastInvestment { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("year_founded")]
        public int? YearFounded { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_current_roi")]
        public string? AverageCurrentRoi { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_market_cap")]
        public string? AverageMarketCap { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_initial_market_cap")]
        public string? AverageInitialMarketCap { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_fdmc")]
        public string? AverageFdmc { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_initial_fdmc")]
        public string? AverageInitialFdmc { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_public_raise")]
        public string? AveragePublicRaise { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_private_raise")]
        public string? AveragePrivateRaise { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_total_raise")]
        public string? AverageTotalRaise { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_price_change_24h")]
        public string? AveragePriceChange24h { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_price_change_7d")]
        public string? AveragePriceChange7d { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_price_change_30d")]
        public string? AveragePriceChange30d { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("average_price_change_1y")]
        public string? AveragePriceChange1y { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("project_count")]
        public int? ProjectCount { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("gainers_losers")]
        public InternalGainersLosers? GainersLosers { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("projects")]
        public List<InternalRef>? Projects { get; init; }
    }

    private sealed record InternalFundType
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("slug")]
        public string? Slug { get; init; }
    }

    private sealed record InternalGainersLosers
    {
        [System.Text.Json.Serialization.JsonPropertyName("gainers")]
        public System.Text.Json.JsonElement[]? Gainers { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("losers")]
        public System.Text.Json.JsonElement[]? Losers { get; init; }
    }

    private sealed record InternalRef
    {
        [System.Text.Json.Serialization.JsonPropertyName("slug")]
        public string? Slug { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}
