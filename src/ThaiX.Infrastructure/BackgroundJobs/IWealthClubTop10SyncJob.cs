using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Domain.Aggregates.TcbsTop10;
using ThaiX.Infrastructure.ExternalApis.Providers;
using ThaiX.Infrastructure.Persistence;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that syncs TCBS Top 10 portfolio updates from iWealth Club daily at 05:00 VNT.
/// Paginates via lastContentId cursor until a known contentId is found in the database.
/// Parses each HTML output to extract postedAt, effectiveDate, added/removed tickers, and image GUIDs.
/// Insert-only: existing records are never modified.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class IWealthClubTop10SyncJob : HangfireJobBase
{
    // Matches: +) Thêm mới: **[CTG](...)**  or  +) Thêm mới: **[CTG](...)**, **[NLG](...)**
    private static readonly Regex AddedTickersRegex =
        new(@"Thêm mới\s*:\s*((?:\*\*\[([A-Z0-9]+)\][^\*]*\*\*[,\s]*)+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches: +) Loại bỏ: **[MWG](...)** ...
    private static readonly Regex RemovedTickersRegex =
        new(@"Loại bỏ\s*:\s*((?:\*\*\[([A-Z0-9]+)\][^\*]*\*\*[,\s]*)+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Extract all tickers from a segment like **[CTG](...)**, **[NLG](...)**
    private static readonly Regex TickerExtractRegex =
        new(@"\*\*\[([A-Z0-9]+)\]", RegexOptions.Compiled);

    // Matches: <time ... datetime="2026-03-10T18:21:07+07:00" ...>
    private static readonly Regex PostedAtRegex =
        new(@"datetime=""(\d{4}-\d{2}-\d{2})T", RegexOptions.Compiled);

    // Matches: Chi tiết danh mục từ DD/MM/YYYY
    private static readonly Regex EffectiveDateRegex =
        new(@"Chi tiết danh mục từ (\d{2}/\d{2}/\d{4})", RegexOptions.Compiled);

    // Matches: file-guid:xxxxxxxx-... (3 occurrences in order)
    private static readonly Regex FileGuidRegex =
        new(@"file-guid:([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})", RegexOptions.Compiled);

    // Matches topic: Danh mục Analyst Top10
    private static readonly Regex Top10TopicRegex =
        new(@"Danh mục Analyst Top10", RegexOptions.Compiled);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWealthClubApiProvider _provider;
    private readonly INotificationRouter _notificationRouter;
    private readonly ICacheService _cache;
    private readonly ILogger<IWealthClubTop10SyncJob> _logger;

    public IWealthClubTop10SyncJob(
        IServiceScopeFactory scopeFactory,
        IWealthClubApiProvider provider,
        INotificationRouter notificationRouter,
        ICacheService cache,
        IHangfireJobState hangfireJobState,
        ILogger<IWealthClubTop10SyncJob> logger) : base(hangfireJobState, logger)
    {
        _scopeFactory = scopeFactory;
        _provider = provider;
        _notificationRouter = notificationRouter;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>Entry point called by Hangfire.</summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.IWealthClubTop10DailySyncJob)) return;

        _logger.LogInformation("IWealthClubTop10SyncJob started");
        var started = DateTime.UtcNow;

        try
        {
            var result = await SyncAsync(cancellationToken);
            var elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;
            _logger.LogInformation("IWealthClubTop10SyncJob completed in {Elapsed}s: {Count} new records", elapsed, result.InsertedCount);

            var text = $"✅ iWealth Club Top10 sync finished in {elapsed}s.\nNew records: {result.InsertedCount}";
            if (result.InsertedCount > 0)
            {
                text += "\n" + BuildTickerSummaryText(result.AddedTickers, result.RemovedTickers);
            }

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Title = "iWealth Club Top10 Sync Completed",
                Text = text,
                Severity = NotificationSeverity.Info
            }, cancellationToken);

            await _cache.InvalidateGroupAsync(CacheGroups.TcbsTop10Data, cancellationToken);
            _logger.LogDebug("IWealthClubTop10SyncJob: cache group {Group} invalidated", CacheGroups.TcbsTop10Data);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IWealthClubTop10SyncJob failed");

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "iWealth Club Top10 Sync",
                    Type = NotificationCardType.Error,
                    Severity = NotificationSeverity.Error,
                    Metrics =
                    [
                        new() { Label = "Code", Value = "IWEALTH_TOP10_SYNC_FAILED" },
                        new() { Label = "Message", Value = ex.Message }
                    ]
                }
            }, CancellationToken.None);

            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Core sync logic
    // -------------------------------------------------------------------------

    private async Task<SyncResult> SyncAsync(CancellationToken ct)
    {
        // Load all existing contentIds from DB upfront to avoid per-item round trips
        await using var initialScope = _scopeFactory.CreateAsyncScope();
        var initialDb = initialScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existingContentIds = await initialDb.TcbsTop10Portfolios
            .AsNoTracking()
            .Select(p => p.SourceContentId)
            .ToHashSetAsync(ct);

        _logger.LogDebug("IWealthClubTop10SyncJob: {Count} existing contentIds loaded from DB", existingContentIds.Count);

        var newItems = new List<ParsedPortfolio>();
        string? cursor = null; // null = first call

        while (true)
        {
            StreamResponse? response;
            try
            {
                response = await _provider.GetStreamAsync<StreamResponse>(cursor, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IWealthClubTop10SyncJob: API call failed (cursor={Cursor})", cursor);
                break;
            }

            if (response?.Content is null or { Count: 0 })
            {
                _logger.LogWarning("IWealthClubTop10SyncJob: empty response (cursor={Cursor})", cursor);
                break;
            }

            bool reachedKnown = false;
            string? nextCursor = null;

            foreach (var (_, item) in response.Content)
            {
                // Track the last contentId seen to use as cursor for next page
                nextCursor = item.Id.ToString();

                if (existingContentIds.Contains(item.Id))
                {
                    _logger.LogDebug("IWealthClubTop10SyncJob: contentId {Id} already in DB — stopping", item.Id);
                    reachedKnown = true;
                    break;
                }

                // Only process Top10 posts
                if (!Top10TopicRegex.IsMatch(item.Output))
                {
                    _logger.LogDebug("IWealthClubTop10SyncJob: contentId {Id} is not a Top10 post — skipping", item.Id);
                    continue;
                }

                var parsed = TryParse(item.Id, item.Output);
                if (parsed is null)
                {
                    _logger.LogWarning("IWealthClubTop10SyncJob: could not parse contentId {Id} — skipping", item.Id);
                    continue;
                }

                newItems.Add(parsed);
            }

            if (reachedKnown || nextCursor is null)
                break;

            cursor = nextCursor;
        }

        if (newItems.Count == 0)
        {
            _logger.LogInformation("IWealthClubTop10SyncJob: no new Top10 posts found");
            return new SyncResult(0, Array.Empty<string>(), Array.Empty<string>());
        }

        var addedTickers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var removedTickers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Insert all new items in a single scope
        await using var insertScope = _scopeFactory.CreateAsyncScope();
        var db = insertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var parsed in newItems)
        {
            var portfolio = TcbsTop10Portfolio.Create(parsed.SourceContentId, parsed.PostedAt, parsed.EffectiveDate);

            foreach (var ticker in parsed.AddedTickers)
            {
                portfolio.AddTicker(ticker, TcbsPortfolioChangeType.Added);
                addedTickers.Add(ticker);
            }

            foreach (var ticker in parsed.RemovedTickers)
            {
                portfolio.AddTicker(ticker, TcbsPortfolioChangeType.Removed);
                removedTickers.Add(ticker);
            }

            if (parsed.DetailImageGuid.HasValue)
                portfolio.AddImage(TcbsPortfolioImageType.PortfolioDetail, parsed.DetailImageGuid.Value);

            if (parsed.PeriodImageGuid.HasValue)
                portfolio.AddImage(TcbsPortfolioImageType.PeriodPerformance, parsed.PeriodImageGuid.Value);

            if (parsed.YtdImageGuid.HasValue)
                portfolio.AddImage(TcbsPortfolioImageType.YtdPerformance, parsed.YtdImageGuid.Value);

            db.TcbsTop10Portfolios.Add(portfolio);
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("IWealthClubTop10SyncJob: inserted {Count} new portfolios", newItems.Count);
        return new SyncResult(newItems.Count, addedTickers.OrderBy(x => x).ToArray(), removedTickers.OrderBy(x => x).ToArray());
    }

    private static string BuildTickerSummaryText(IReadOnlyCollection<string> addedTickers, IReadOnlyCollection<string> removedTickers)
    {
        var addedText = addedTickers.Count == 0
            ? "Added: none"
            : $"Added: {string.Join(", ", addedTickers)}";

        var removedText = removedTickers.Count == 0
            ? "Removed: none"
            : $"Removed: {string.Join(", ", removedTickers)}";

        return $"{addedText}\n{removedText}";
    }

    private sealed record SyncResult(int InsertedCount, IReadOnlyCollection<string> AddedTickers, IReadOnlyCollection<string> RemovedTickers);

    // -------------------------------------------------------------------------
    // HTML parser
    // -------------------------------------------------------------------------

    private ParsedPortfolio? TryParse(long contentId, string html)
    {
        // PostedAt: first datetime attribute in the HTML
        var postedAtMatch = PostedAtRegex.Match(html);
        if (!postedAtMatch.Success || !DateOnly.TryParseExact(postedAtMatch.Groups[1].Value, "yyyy-MM-dd", out var postedAt))
        {
            _logger.LogDebug("IWealthClubTop10SyncJob: contentId {Id} — could not parse postedAt", contentId);
            return null;
        }

        // EffectiveDate: "Chi tiết danh mục từ DD/MM/YYYY"
        var effectiveDateMatch = EffectiveDateRegex.Match(html);
        DateOnly effectiveDate;
        if (!effectiveDateMatch.Success || !DateOnly.TryParseExact(effectiveDateMatch.Groups[1].Value, "dd/MM/yyyy", out effectiveDate))
        {
            // Fallback: effective = postedAt + 1 day
            effectiveDate = postedAt.AddDays(1);
            _logger.LogDebug("IWealthClubTop10SyncJob: contentId {Id} — effectiveDate fallback to postedAt+1", contentId);
        }

        // Added tickers
        var addedTickers = ExtractTickers(AddedTickersRegex, html);

        // Removed tickers
        var removedTickers = ExtractTickers(RemovedTickersRegex, html);

        // Images: up to 3 file-guid occurrences in order
        var guids = FileGuidRegex.Matches(html)
            .Select(m => Guid.TryParse(m.Groups[1].Value, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .Distinct()
            .Take(3)
            .ToArray();

        return new ParsedPortfolio(
            SourceContentId: contentId,
            PostedAt: postedAt,
            EffectiveDate: effectiveDate,
            AddedTickers: addedTickers,
            RemovedTickers: removedTickers,
            DetailImageGuid: guids.Length > 0 ? guids[0] : null,
            PeriodImageGuid: guids.Length > 1 ? guids[1] : null,
            YtdImageGuid: guids.Length > 2 ? guids[2] : null);
    }

    private static List<string> ExtractTickers(Regex sectionRegex, string html)
    {
        var sectionMatch = sectionRegex.Match(html);
        if (!sectionMatch.Success)
            return [];

        return TickerExtractRegex.Matches(sectionMatch.Value)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();
    }

    // -------------------------------------------------------------------------
    // Internal DTOs
    // -------------------------------------------------------------------------

    private sealed record StreamResponse
    {
        public Dictionary<string, StreamItem> Content { get; init; } = [];
    }

    private sealed record StreamItem
    {
        public long Id { get; init; }
        public string Output { get; init; } = string.Empty;
    }

    private sealed record ParsedPortfolio(
        long SourceContentId,
        DateOnly PostedAt,
        DateOnly EffectiveDate,
        List<string> AddedTickers,
        List<string> RemovedTickers,
        Guid? DetailImageGuid,
        Guid? PeriodImageGuid,
        Guid? YtdImageGuid);
}
