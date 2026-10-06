using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.ChainBroker;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.ExternalApis.Providers;
using ThaiX.Infrastructure.Persistence;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that syncs ChainBroker projects data daily at 02:00 VNT (UTC+7).
/// Requires funds to be synced first (ChainBrokerFundsSyncJob).
/// Fetches all pages for each fund (following "next" until null) and performs an upsert
/// using batches of 200 items, each with its own DbContext scope.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class ChainBrokerProjectsSyncJob : HangfireJobBase
{
    private const int BatchSize = 200;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ChainBrokerApiProvider _provider;
    private readonly INotificationRouter _notificationRouter;
    private readonly ICacheService _cache;
    private readonly ILogger<ChainBrokerProjectsSyncJob> _logger;

    public ChainBrokerProjectsSyncJob(
        IServiceScopeFactory scopeFactory,
        ChainBrokerApiProvider provider,
        INotificationRouter notificationRouter,
        ICacheService cache,
        IHangfireJobState hangfireJobState,
        ILogger<ChainBrokerProjectsSyncJob> logger) : base(hangfireJobState, logger)
    {
        _scopeFactory = scopeFactory;
        _provider = provider;
        _notificationRouter = notificationRouter;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Entry point called by Hangfire. Syncs ChainBroker projects data.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.ChainBrokerProjectsWeeklySyncJob)) return;

        _logger.LogInformation("ChainBrokerProjectsSyncJob started");
        var started = DateTime.UtcNow;

        using var _ = await AsyncLock.GetLockByKey("sync:chainbroker").LockAsync(cancellationToken);

        try
        {
            var count = await SyncProjectsAsync(cancellationToken);
            var elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;
            _logger.LogInformation("ChainBrokerProjectsSyncJob completed in {Elapsed}s: {Count} records", elapsed, count);

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Title = "ChainBroker Projects Sync Completed",
                Text = $"✅ ChainBroker projects sync finished in {elapsed}s.\nProjects: {count}",
                Severity = NotificationSeverity.Info
            }, cancellationToken);

            await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerData, cancellationToken);
            _logger.LogDebug("ChainBrokerProjectsSyncJob: cache group {Group} invalidated", CacheGroups.ChainBrokerData);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChainBrokerProjectsSyncJob failed");

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "ChainBroker Projects Sync",
                    Type = NotificationCardType.Error,
                    Severity = NotificationSeverity.Error,
                    Metrics =
                    [
                        new() { Label = "Code", Value = "CHAINBROKER_PROJECTS_SYNC_FAILED" },
                        new() { Label = "Message", Value = ex.Message }
                    ]
                }
            }, CancellationToken.None);

            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Projects
    // -------------------------------------------------------------------------

    private async Task<int> SyncProjectsAsync(CancellationToken ct)
    {
        _logger.LogInformation("ChainBroker: fetching projects by fund from API");

        List<(Guid Id, string Slug, string? Name)> fundInfos;
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var raw = await db.ChainBrokerFunds
                .AsNoTracking()
                .Select(f => new { f.Id, f.Slug, f.Name })
                .ToListAsync(ct);
            fundInfos = raw.ConvertAll(f => (f.Id, f.Slug, f.Name));
        }

        if (fundInfos.Count == 0)
        {
            _logger.LogWarning("ChainBroker projects: no funds in DB — skipping project sync");
            return 0;
        }

        var fundTasks = fundInfos
            .Select(f => FetchProjectsByFundAsync(f.Slug, ct))
            .ToList();

        var fundLookup = fundInfos.ToDictionary(
            f => f.Slug,
            f => f.Id,
            StringComparer.OrdinalIgnoreCase);

        var fundResults = await Task.WhenAll(fundTasks).ConfigureAwait(false);

        var apiItems = new Dictionary<string, InternalProjectItem>(StringComparer.OrdinalIgnoreCase);

        // Accumulate all fund IDs per project slug using the query parameter fund slug → DB Id.
        // A project can appear in multiple funds across parallel queries; merge here.
        var allFundsPerProject = new Dictionary<string, HashSet<Guid>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (fundSlug, items) in fundResults)
        {
            if (!fundLookup.TryGetValue(fundSlug, out var fundId))
                continue;

            foreach (var (slug, item) in items)
            {
                apiItems.TryAdd(slug, item);

                if (!allFundsPerProject.TryGetValue(slug, out var fundSet))
                {
                    fundSet = [];
                    allFundsPerProject[slug] = fundSet;
                }

                fundSet.Add(fundId);
            }
        }

        if (apiItems.Count == 0)
        {
            _logger.LogWarning("ChainBroker projects: API returned no data — preserving existing data");
            return 0;
        }

        // --- Upsert in batches of BatchSize ---
        var allSlugs = apiItems.Keys.ToArray();
        int upserted = 0;

        foreach (var batch in allSlugs.Chunk(BatchSize))
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var batchSet = new HashSet<string>(batch, StringComparer.OrdinalIgnoreCase);

            var existing = await db.ChainBrokerProjects
                .Include(p => p.Blockchains)
                .Include(p => p.Tags)
                .Include(p => p.Launchpads)
                .Where(p => batchSet.Contains(p.Slug))
                .ToDictionaryAsync(p => p.Slug, StringComparer.OrdinalIgnoreCase, ct);

            foreach (var slug in batch)
            {
                if (!apiItems.TryGetValue(slug, out var r)) continue;

                var snapshot = BuildSnapshot(r, allFundsPerProject.GetValueOrDefault(slug));

                if (existing.TryGetValue(slug, out var project))
                {
                    project.UpdateWithoutFunds(snapshot);
                    await UpdateProjectFundsAsync(db, project.Id, snapshot.Funds, ct);
                }
                else
                {
                    db.ChainBrokerProjects.Add(ChainBrokerProject.Create(snapshot));
                }
            }

            await db.SaveChangesAsync(ct);
            upserted += batch.Length;
            _logger.LogDebug("ChainBroker projects: upserted batch ({Done}/{Total})", upserted, allSlugs.Length);
        }

        // --- Remove stale projects ---
        var apiSlugSet = new HashSet<string>(apiItems.Keys, StringComparer.OrdinalIgnoreCase);

        await using (var deleteScope = _scopeFactory.CreateAsyncScope())
        {
            var db = deleteScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var dbSlugs = await db.ChainBrokerProjects
                .Select(p => p.Slug)
                .ToListAsync(ct);

            var staleSlugs = dbSlugs
                .Where(s => !apiSlugSet.Contains(s))
                .ToList();

            if (staleSlugs.Count > 0)
            {
                foreach (var staleBatch in staleSlugs.Chunk(BatchSize))
                {
                    await db.ChainBrokerProjects
                        .Where(p => staleBatch.Contains(p.Slug))
                        .ExecuteDeleteAsync(ct);

                    _logger.LogDebug("ChainBroker projects: removed {Count} stale records", staleBatch.Length);
                }
            }
        }

        _logger.LogInformation("ChainBroker projects upsert complete: {Total} unique from API", apiItems.Count);
        return apiItems.Count;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<(string FundSlug, Dictionary<string, InternalProjectItem> Items)> FetchProjectsByFundAsync(
        string fundSlug,
        CancellationToken ct)
    {
        var result = new Dictionary<string, InternalProjectItem>(StringComparer.OrdinalIgnoreCase);
        int page = 1;
        string? nextUrl;
        do
        {
            var requestUrl = _provider.GetProjectsByFundSyncUrl(page, fundSlug);
            InternalProjectsResponse? response;
            try
            {
                _logger.LogDebug("ChainBroker projects[{Fund}]: requesting page {Page}. Url={Url}", fundSlug, page, requestUrl);
                response = await _provider.GetProjectsByFundSyncAsync<InternalProjectsResponse>(page, fundSlug, ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChainBroker projects[{Fund}]: request failed on page {Page}. Url={Url} — skipping fund", fundSlug, page, requestUrl);
                return (fundSlug, result);
            }

            var list = response?.Data?.List;
            if (list?.Results is null or { Count: 0 })
            {
                _logger.LogDebug("ChainBroker projects: no results for fund {Fund} page {Page}", fundSlug, page);
                break;
            }

            nextUrl = list.Next;
            foreach (var item in list.Results)
            {
                if (!string.IsNullOrWhiteSpace(item.Slug))
                    result.TryAdd(item.Slug, item);
            }

            _logger.LogDebug("ChainBroker projects: fund {Fund} page {Page} ({Count} records)", fundSlug, page, list.Results.Count);
            page++;
        } while (!string.IsNullOrEmpty(nextUrl));

        return (fundSlug, result);
    }

    private static ChainBrokerProjectSnapshot BuildSnapshot(
        InternalProjectItem r,
        HashSet<Guid>? mergedFundIds = null) => new()
        {
            Slug = r.Slug!,
            Name = r.Name,
            Logo = r.Logo,
            Ticker = r.Ticker,
            CurrentPrice = r.CurrentPrice,
            PublicPrice = r.PublicPrice,
            PrivatePrice = r.PrivatePrice,
            AthPrice = r.AthPrice,
            AtlPrice = r.AtlPrice,
            PublicRoi = r.PublicRoi,
            PrivateRoi = r.PrivateRoi,
            PublicAthRoi = r.PublicAthRoi,
            PrivateAthRoi = r.PrivateAthRoi,
            BrokerScore = r.BrokerScore,
            SecurityScore = r.SecurityScore,
            TwitterScore = r.TwitterScore,
            Rank = r.Rank,
            PublicRaise = r.PublicRaise,
            PrivateRaise = r.PrivateRaise,
            TotalRaise = r.TotalRaise,
            PrivateAnnounceDate = r.PrivateAnnounceDate,
            ListingDate = r.ListingDate,
            IdoDate = r.IdoDate,
            NextUnlock = r.NextUnlock,
            MarketCap = r.MarketCap,
            ReportedMarketCap = r.ReportedMarketCap,
            InitialMarketCap = r.InitialMarketCap,
            Fdmc = r.Fdmc,
            InitialFdmc = r.InitialFdmc,
            Volume24h = r.Volume24h,
            CurrentCirculation = r.CurrentCirculation,
            InitialCirculation = r.InitialCirculation,
            TotalCirculation = r.TotalCirculation,
            PercentCirculating = r.PercentCirculating,
            PriceChange24h = r.PriceChange24h,
            PriceChange7d = r.PriceChange7d,
            PriceChange30d = r.PriceChange30d,
            PriceChange1y = r.PriceChange1y,
            Blockchains = r.Blockchains?
            .Where(b => !string.IsNullOrWhiteSpace(b.Name))
            .Select(b => b.Name!)
            .ToList() ?? [],
            Tags = r.Tags?
            .Where(t => !string.IsNullOrWhiteSpace(t.Name))
            .Select(t => (t.Name!, t.Slug))
            .ToList() ?? [],
            Funds = mergedFundIds?.ToList() ?? [],
            Launchpads = r.Launchpads?
            .Where(l => !string.IsNullOrWhiteSpace(l.Slug))
            .Select(l => (l.Slug!, l.Name))
            .ToList() ?? []
        };

    private static async Task UpdateProjectFundsAsync(
        ApplicationDbContext db,
        Guid projectId,
        IReadOnlyList<Guid> incomingFundIds,
        CancellationToken ct)
    {
        var incomingSet = new HashSet<Guid>(incomingFundIds);

        var existingFundRefs = await db.ChainBrokerProjectFundRefs
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(ct);

        var existingFundIds = new HashSet<Guid>(existingFundRefs.Select(x => x.FundId));

        var removed = existingFundRefs
            .Where(x => !incomingSet.Contains(x.FundId))
            .ToList();

        if (removed.Count > 0)
            db.ChainBrokerProjectFundRefs.RemoveRange(removed);

        foreach (var fundId in incomingSet.Except(existingFundIds))
        {
            db.ChainBrokerProjectFundRefs.Add(ChainBrokerProjectFundRef.Create(projectId, fundId));
        }
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

    private sealed record InternalProjectsResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public InternalProjectsData? Data { get; init; }
    }

    private sealed record InternalProjectsData
    {
        [System.Text.Json.Serialization.JsonPropertyName("list")]
        public InternalPagedList<InternalProjectItem>? List { get; init; }
    }

    private sealed record InternalProjectItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("slug")]
        public string? Slug { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("logo")]
        public string? Logo { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("ticker")]
        public string? Ticker { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("current_price")]
        public string? CurrentPrice { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("public_price")]
        public string? PublicPrice { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("private_price")]
        public string? PrivatePrice { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("ath_price")]
        public string? AthPrice { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("atl_price")]
        public string? AtlPrice { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("public_roi")]
        public string? PublicRoi { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("private_roi")]
        public string? PrivateRoi { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("public_ath_roi")]
        public string? PublicAthRoi { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("private_ath_roi")]
        public string? PrivateAthRoi { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("broker_score")]
        public string? BrokerScore { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("security_score")]
        public string? SecurityScore { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("twitter_score")]
        public int? TwitterScore { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("rank")]
        public int? Rank { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("public_raise")]
        public string? PublicRaise { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("private_raise")]
        public string? PrivateRaise { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("total_raise")]
        public string? TotalRaise { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("private_announce_date")]
        public string? PrivateAnnounceDate { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("listing_date")]
        public string? ListingDate { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("ido_date")]
        public string? IdoDate { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("next_unlock")]
        public string? NextUnlock { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("market_cap")]
        public string? MarketCap { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("reported_market_cap")]
        public string? ReportedMarketCap { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("initial_market_cap")]
        public string? InitialMarketCap { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("fdmc")]
        public string? Fdmc { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("initial_fdmc")]
        public string? InitialFdmc { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("volume_24h")]
        public string? Volume24h { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("current_circulation")]
        public string? CurrentCirculation { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("initial_circulation")]
        public string? InitialCirculation { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("total_circulation")]
        public string? TotalCirculation { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("percent_circulating")]
        public string? PercentCirculating { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("price_change_24h")]
        public string? PriceChange24h { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("price_change_7d")]
        public string? PriceChange7d { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("price_change_30d")]
        public string? PriceChange30d { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("price_change_1y")]
        public string? PriceChange1y { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("blockchains")]
        public List<InternalNameOnly>? Blockchains { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("tags")]
        public List<InternalRef>? Tags { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("launchpads")]
        public List<InternalRef>? Launchpads { get; init; }
    }

    private sealed record InternalNameOnly
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed record InternalRef
    {
        [System.Text.Json.Serialization.JsonPropertyName("slug")]
        public string? Slug { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}
