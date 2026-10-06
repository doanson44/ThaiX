using Hangfire;
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
/// Hangfire recurring job that syncs ChainBroker unlocks data daily at 03:00 VNT (UTC+7).
/// Fetches all pages (following "next" until null) and performs an upsert
/// using batches of 200 items, each with its own DbContext scope.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class ChainBrokerUnlocksSyncJob : HangfireJobBase
{
    private const int BatchSize = 200;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ChainBrokerApiProvider _provider;
    private readonly INotificationRouter _notificationRouter;
    private readonly ICacheService _cache;
    private readonly ILogger<ChainBrokerUnlocksSyncJob> _logger;

    public ChainBrokerUnlocksSyncJob(
        IServiceScopeFactory scopeFactory,
        ChainBrokerApiProvider provider,
        INotificationRouter notificationRouter,
        ICacheService cache,
        IHangfireJobState hangfireJobState,
        ILogger<ChainBrokerUnlocksSyncJob> logger) : base(hangfireJobState, logger)
    {
        _scopeFactory = scopeFactory;
        _provider = provider;
        _notificationRouter = notificationRouter;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Entry point called by Hangfire. Syncs ChainBroker unlocks data.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.ChainBrokerUnlocksWeeklySyncJob)) return;

        _logger.LogInformation("ChainBrokerUnlocksSyncJob started");
        var started = DateTime.UtcNow;

        try
        {
            var count = await SyncUnlocksAsync(cancellationToken);
            var elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;
            _logger.LogInformation("ChainBrokerUnlocksSyncJob completed in {Elapsed}s: {Count} records", elapsed, count);

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "ChainBroker Unlocks Sync",
                    Type = NotificationCardType.Summary,
                    Severity = NotificationSeverity.Info,
                    Metrics =
                    [
                        new() { Label = "Status", Value = "Success" },
                        new() { Label = "Duration", Value = $"{elapsed}s" },
                        new() { Label = "Records", Value = count.ToString("N0") }
                    ]
                }
            }, cancellationToken);

            await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerData, cancellationToken);
            _logger.LogDebug("ChainBrokerUnlocksSyncJob: cache group {Group} invalidated", CacheGroups.ChainBrokerData);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChainBrokerUnlocksSyncJob failed");

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "ChainBroker Unlocks Sync",
                    Type = NotificationCardType.Error,
                    Severity = NotificationSeverity.Error,
                    Metrics =
                    [
                        new() { Label = "Code", Value = "CHAINBROKER_UNLOCKS_SYNC_FAILED" },
                        new() { Label = "Message", Value = ex.Message }
                    ]
                }
            }, CancellationToken.None);

            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Unlocks
    // -------------------------------------------------------------------------

    private async Task<int> SyncUnlocksAsync(CancellationToken ct)
    {
        _logger.LogInformation("ChainBroker: fetching all unlocks pages from API");
        var apiItems = new List<InternalUnlockItem>();
        int page = 1;
        string? nextUrl;
        do
        {
            var requestUrl = _provider.GetUnlocksListSyncUrl(page);
            InternalUnlocksResponse? response;
            try
            {
                _logger.LogDebug("ChainBroker unlocks: requesting page {Page}. Url={Url}", page, requestUrl);
                response = await _provider.GetUnlocksListSyncAsync<InternalUnlocksResponse>(page, ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChainBroker unlocks: request failed on page {Page}. Url={Url} — aborting fetch", page, requestUrl);
                break;
            }

            var list = response?.Data?.List;
            if (list?.Results is null or { Count: 0 })
            {
                _logger.LogWarning("ChainBroker unlocks: empty or null results on page {Page}. Url={Url}", page, requestUrl);
                break;
            }
            nextUrl = list.Next;
            apiItems.AddRange(list.Results);
            _logger.LogDebug("ChainBroker unlocks: fetched page {Page} ({Count} records)", page, list.Results.Count);
            page++;
        } while (!string.IsNullOrEmpty(nextUrl));

        if (apiItems.Count == 0)
        {
            _logger.LogWarning("ChainBroker unlocks: API returned no data — preserving existing data");
            return 0;
        }

        // --- Upsert in batches of BatchSize ---
        var validItems = apiItems
            .Where(r => !string.IsNullOrWhiteSpace(r.Slug))
            .GroupBy(r => (Slug: r.Slug!.Trim().ToLowerInvariant(), Date: r.NextUnlock))
            .Select(g => g.First())
            .ToArray();

        int upserted = 0;
        foreach (var batch in validItems.Chunk(BatchSize))
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var batchSlugs = new HashSet<string>(batch.Select(r => r.Slug!), StringComparer.OrdinalIgnoreCase);

            var existingList = await db.ChainBrokerUnlocks
                .Where(u => batchSlugs.Contains(u.Slug))
                .ToListAsync(ct);

            var existingLookup = existingList.ToDictionary(
                u => (u.Slug.ToLowerInvariant(), u.NextUnlockDate));

            foreach (var r in batch)
            {
                var parsedDate = ParseUnlockDate(r.NextUnlock);
                var key = (r.Slug!.Trim().ToLowerInvariant(), parsedDate);
                var snapshot = new ChainBrokerUnlockSnapshot
                {
                    Slug = r.Slug!,
                    Name = r.Name,
                    Logo = r.Logo,
                    Ticker = r.Ticker,
                    NextUnlock = r.NextUnlock,
                    UnlockAmount = r.UnlockAmount,
                    UnlockValue = r.UnlockValue,
                    RoundName = r.RoundName,
                    Circulation = r.Circulation,
                    PriceChange24h = r.PriceChange24h,
                    PriceChange7d = r.PriceChange7d,
                    PriceChange30d = r.PriceChange30d,
                    PriceChange1y = r.PriceChange1y,
                    Volume24h = r.Volume24h,
                    Percent = r.Percent
                };

                if (existingLookup.TryGetValue(key, out var unlock))
                    unlock.Update(snapshot);
                else
                    db.ChainBrokerUnlocks.Add(ChainBrokerUnlock.Create(snapshot));
            }

            await db.SaveChangesAsync(ct);
            upserted += batch.Length;
            _logger.LogDebug("ChainBroker unlocks: upserted batch ({Done}/{Total})", upserted, validItems.Length);
        }

        // --- Remove stale unlocks ---
        var apiKeySet = validItems
            .Select(r => (r.Slug!.Trim().ToLowerInvariant(), ParseUnlockDate(r.NextUnlock)))
            .ToHashSet();

        await using (var deleteScope = _scopeFactory.CreateAsyncScope())
        {
            var db = deleteScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var dbItems = await db.ChainBrokerUnlocks
                .Select(u => new { u.Id, u.Slug, u.NextUnlockDate })
                .ToListAsync(ct);

            var staleIds = dbItems
                .Where(k => !apiKeySet.Contains((k.Slug.ToLowerInvariant(), k.NextUnlockDate)))
                .Select(k => k.Id)
                .ToList();

            if (staleIds.Count > 0)
            {
                foreach (var staleBatch in staleIds.Chunk(BatchSize))
                {
                    await db.ChainBrokerUnlocks
                        .Where(u => staleBatch.Contains(u.Id))
                        .ExecuteDeleteAsync(ct);

                    _logger.LogDebug("ChainBroker unlocks: removed {Count} stale records", staleBatch.Length);
                }
            }
        }

        _logger.LogInformation("ChainBroker unlocks upsert complete: {Total} from API", validItems.Length);
        return validItems.Length;
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static DateOnly? ParseUnlockDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();
        if (DateOnly.TryParseExact(s, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var iso))
            return iso;
        if (DateOnly.TryParseExact(s, "MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var unlockFmt))
            return unlockFmt;
        return null;
    }

    // =========================================================================
    // Internal deserialization models — scoped to this job only
    // =========================================================================

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

    private sealed record InternalUnlocksResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public InternalUnlocksData? Data { get; init; }
    }

    private sealed record InternalUnlocksData
    {
        [System.Text.Json.Serialization.JsonPropertyName("list")]
        public InternalPagedList<InternalUnlockItem>? List { get; init; }
    }

    private sealed record InternalUnlockItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("slug")]
        public string? Slug { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("logo")]
        public string? Logo { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("ticker")]
        public string? Ticker { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("next_unlock")]
        public string? NextUnlock { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("unlock_amount")]
        public string? UnlockAmount { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("unlock_value")]
        public string? UnlockValue { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("round_name")]
        public string? RoundName { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("circulation")]
        public string? Circulation { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("price_change_24h")]
        public string? PriceChange24h { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("price_change_7d")]
        public string? PriceChange7d { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("price_change_30d")]
        public string? PriceChange30d { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("price_change_1y")]
        public string? PriceChange1y { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("volume_24h")]
        public string? Volume24h { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("percent")]
        public string? Percent { get; init; }
    }
}
