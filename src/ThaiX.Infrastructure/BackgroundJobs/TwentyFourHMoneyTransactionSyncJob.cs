using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.MarketData;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.ExternalApis.Providers;
using ThaiX.Infrastructure.Persistence;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that syncs 24HMoney intraday transaction data (SSI matched-order tape)
/// after market close (15:30 SE Asia Standard Time, Mon-Fri) for every symbol ever tracked in the
/// TCBS Top10 portfolio (<see cref="Domain.Aggregates.TcbsTop10.TcbsTop10Ticker"/>).
/// Also purges transaction rows older than 1 year at the end of each run.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class TwentyFourHMoneyTransactionSyncJob : HangfireJobBase
{
    private const int PerPage = 1000;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TwentyFourHMoneyApiProvider _provider;
    private readonly INotificationRouter _notificationRouter;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<TwentyFourHMoneyTransactionSyncJob> _logger;

    public TwentyFourHMoneyTransactionSyncJob(
        IServiceScopeFactory scopeFactory,
        TwentyFourHMoneyApiProvider provider,
        INotificationRouter notificationRouter,
        IDateTimeProvider dateTimeProvider,
        IHangfireJobState hangfireJobState,
        ILogger<TwentyFourHMoneyTransactionSyncJob> logger) : base(hangfireJobState, logger)
    {
        _scopeFactory = scopeFactory;
        _provider = provider;
        _notificationRouter = notificationRouter;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Entry point called by Hangfire. Syncs today's 24HMoney transactions for all tracked symbols,
    /// then purges transactions older than 1 year.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.TwentyFourHMoneyTransactionDailySyncJob)) return;

        _logger.LogInformation("TwentyFourHMoneyTransactionSyncJob started");
        var started = DateTime.UtcNow;
        var tradeDate = DateOnly.FromDateTime(_dateTimeProvider.LocalNow);

        try
        {
            var symbols = await GetTrackedSymbolsAsync(cancellationToken);
            if (symbols.Count == 0)
            {
                _logger.LogInformation(
                    "TwentyFourHMoneyTransactionSyncJob: no tracked symbols found in TcbsTop10Tickers, skipping");
                return;
            }

            var inserted = 0;
            foreach (var symbol in symbols)
            {
                inserted += await SyncSymbolAsync(symbol, tradeDate, cancellationToken).ConfigureAwait(false);
            }

            var purged = await PurgeOldTransactionsAsync(tradeDate, cancellationToken).ConfigureAwait(false);

            var elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;
            _logger.LogInformation(
                "TwentyFourHMoneyTransactionSyncJob completed in {Elapsed}s: {Symbols} symbols, {Inserted} inserted, {Purged} purged",
                elapsed, symbols.Count, inserted, purged);

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "24HMoney Transactions Sync",
                    Type = NotificationCardType.Summary,
                    Severity = NotificationSeverity.Info,
                    Metrics =
                    [
                        new() { Label = "Status", Value = "Success" },
                        new() { Label = "Duration", Value = $"{elapsed}s" },
                        new() { Label = "Symbols", Value = symbols.Count.ToString("N0") },
                        new() { Label = "Inserted", Value = inserted.ToString("N0") },
                        new() { Label = "Purged", Value = purged.ToString("N0") }
                    ]
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TwentyFourHMoneyTransactionSyncJob failed");

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "24HMoney Transactions Sync",
                    Type = NotificationCardType.Error,
                    Severity = NotificationSeverity.Error,
                    Metrics =
                    [
                        new() { Label = "Code", Value = "24HMONEY_TRANSACTIONS_SYNC_FAILED" },
                        new() { Label = "Message", Value = ex.Message }
                    ]
                }
            }, CancellationToken.None);

            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Tracked symbols
    // -------------------------------------------------------------------------

    private async Task<List<string>> GetTrackedSymbolsAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.TcbsTop10Tickers
            .Select(t => t.Ticker)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Sync
    // -------------------------------------------------------------------------

    private async Task<int> SyncSymbolAsync(string symbol, DateOnly tradeDate, CancellationToken ct)
    {
        InternalTransactionsResponse? response;
        try
        {
            response = await _provider
                .GetTransactionListSsiAsync<InternalTransactionsResponse>(symbol, page: 1, perPage: PerPage, ct)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "24HMoney transactions: request failed for symbol {Symbol} — skipping", symbol);
            return 0;
        }

        var items = response?.Data;
        if (items is null or { Count: 0 })
        {
            _logger.LogDebug("24HMoney transactions: no data returned for symbol {Symbol}", symbol);
            return 0;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // TotalVolume is cumulative within the trading day, so it doubles as a natural dedup key
        // per (Symbol, TradeDate) — lets the job be safely re-run without violating the unique index.
        var existingVolumes = await db.TwentyFourHMoneyTransactions
            .Where(t => t.Symbol == symbol && t.TradeDate == tradeDate)
            .Select(t => t.TotalVolume)
            .ToListAsync(ct);
        var seenVolumes = new HashSet<long>(existingVolumes);

        var newItems = items.Where(x => seenVolumes.Add(x.TotalVolume)).ToList();
        if (newItems.Count == 0)
        {
            _logger.LogDebug("24HMoney transactions: symbol {Symbol} already up to date for {TradeDate}", symbol, tradeDate);
            return 0;
        }

        foreach (var item in newItems)
        {
            db.TwentyFourHMoneyTransactions.Add(TwentyFourHMoneyTransaction.Create(
                symbol,
                tradeDate,
                item.Time,
                item.Price,
                item.Change,
                item.MatchQuantity,
                item.TotalVolume,
                item.Side));
        }

        await db.SaveChangesAsync(ct);
        _logger.LogDebug("24HMoney transactions: symbol {Symbol} inserted {Count} new rows", symbol, newItems.Count);
        return newItems.Count;
    }

    // -------------------------------------------------------------------------
    // Retention
    // -------------------------------------------------------------------------

    private async Task<int> PurgeOldTransactionsAsync(DateOnly tradeDate, CancellationToken ct)
    {
        var cutoff = tradeDate.AddYears(-1);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var purged = await db.TwentyFourHMoneyTransactions
            .Where(t => t.TradeDate < cutoff)
            .ExecuteDeleteAsync(ct);

        if (purged > 0)
        {
            _logger.LogInformation("24HMoney transactions: purged {Count} rows older than {Cutoff}", purged, cutoff);
        }

        return purged;
    }

    // =========================================================================
    // Internal deserialization models — scoped to this job only
    // =========================================================================

    private sealed record InternalTransactionsResponse
    {
        [JsonPropertyName("data")]
        public List<InternalTransactionItem>? Data { get; init; }
    }

    private sealed record InternalTransactionItem
    {
        [JsonPropertyName("price")]
        public decimal Price { get; init; }

        [JsonPropertyName("change")]
        public decimal Change { get; init; }

        [JsonPropertyName("match_qtty")]
        public long MatchQuantity { get; init; }

        [JsonPropertyName("total_vol")]
        public long TotalVolume { get; init; }

        [JsonPropertyName("time")]
        public string Time { get; init; } = string.Empty;

        [JsonPropertyName("side")]
        public string Side { get; init; } = string.Empty;
    }
}
