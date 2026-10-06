using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExternalData.Lottery.Queries.GetPower655Results;
using ThaiX.Domain.Aggregates.Lottery;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.ExternalApis.Providers;
using ThaiX.Infrastructure.Persistence;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that syncs Power 6/55 lottery results from ketquadientoan.com.
/// Runs every Tuesday, Thursday, and Saturday at 8 PM Vietnam time (after the draw).
/// On first run (empty DB), fetches all results starting from 01-08-2017.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class Power655SyncJob : HangfireJobBase
{
    private static readonly DateOnly FirstDrawDate = new(2017, 8, 1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KetQuaDienToanApiProvider _provider;
    private readonly ICacheService _cache;
    private readonly INotificationRouter _notificationRouter;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<Power655SyncJob> _logger;

    public Power655SyncJob(
        IServiceScopeFactory scopeFactory,
        KetQuaDienToanApiProvider provider,
        ICacheService cache,
        INotificationRouter notificationRouter,
        IDateTimeProvider dateTimeProvider,
        IHangfireJobState hangfireJobState,
        ILogger<Power655SyncJob> logger) : base(hangfireJobState, logger)
    {
        _scopeFactory = scopeFactory;
        _provider = provider;
        _cache = cache;
        _notificationRouter = notificationRouter;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Entry point called by Hangfire. Determines the date range to sync,
    /// fetches results from the external API, and upserts into the database.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.Power655SyncJob))
        {
            return;
        }

        _logger.LogInformation("Power655SyncJob started");

        var started = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(_dateTimeProvider.LocalNow);

        try
        {
            var fromDate = await GetSyncFromDateAsync(cancellationToken);

            if (fromDate > today)
            {
                _logger.LogInformation(
                    "Power655SyncJob: nothing to sync (from {FromDate} > today {Today})",
                    fromDate,
                    today);

                return;
            }

            var allResults = new List<Power655ResultItem>();

            foreach (var (batchFrom, batchTo) in SplitByYear(fromDate, today))
            {
                var dateFrom = batchFrom.ToString("dd-MM-yyyy");
                var dateTo = batchTo.ToString("dd-MM-yyyy");

                _logger.LogInformation(
                    "Power655SyncJob: fetching results from {DateFrom} to {DateTo}",
                    dateFrom,
                    dateTo);

                var results = await _provider.GetPower655ResultsAsync(
                    dateFrom,
                    dateTo,
                    cancellationToken);

                if (results is { Count: > 0 })
                {
                    allResults.AddRange(results);
                }
            }

            if (allResults.Count == 0)
            {
                _logger.LogWarning(
                    "Power655SyncJob: no results returned for {DateFrom} - {DateTo}",
                    fromDate.ToString("dd-MM-yyyy"),
                    today.ToString("dd-MM-yyyy"));

                return;
            }

            var inserted = await UpsertResultsAsync(allResults, cancellationToken);

            if (inserted > 0)
            {
                await _cache.InvalidateGroupAsync(CacheGroups.Lottery, cancellationToken);

                _logger.LogDebug(
                    "Power655SyncJob: invalidated cache group {Group}",
                    CacheGroups.Lottery);
            }

            var elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;

            _logger.LogInformation(
                "Power655SyncJob completed in {Elapsed}s: {Inserted} results upserted from {DateFrom} to {DateTo}",
                elapsed,
                inserted,
                fromDate.ToString("dd-MM-yyyy"),
                today.ToString("dd-MM-yyyy"));

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "Power 6/55 Sync",
                    Type = NotificationCardType.Summary,
                    Severity = NotificationSeverity.Info,
                    Metrics =
                    [
                        new() { Label = "Status", Value = "Success" },
                        new() { Label = "Duration", Value = $"{elapsed}s" },
                        new() { Label = "Inserted", Value = inserted.ToString("N0") },
                        new() { Label = "From", Value = fromDate.ToString("dd-MM-yyyy") },
                        new() { Label = "To", Value = today.ToString("dd-MM-yyyy") }
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
            _logger.LogError(ex, "Power655SyncJob failed");

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "Power 6/55 Sync",
                    Type = NotificationCardType.Error,
                    Severity = NotificationSeverity.Error,
                    Metrics =
                    [
                        new() { Label = "Code", Value = "POWER655_SYNC_FAILED" },
                        new() { Label = "Message", Value = ex.Message }
                    ]
                }
            }, CancellationToken.None);

            throw;
        }
    }

    private static IReadOnlyList<(DateOnly From, DateOnly To)> SplitByYear(
        DateOnly from,
        DateOnly to)
    {
        var batches = new List<(DateOnly From, DateOnly To)>();

        var current = from;

        while (current <= to)
        {
            var endOfYear = new DateOnly(current.Year, 12, 31);
            var batchTo = endOfYear < to ? endOfYear : to;

            batches.Add((current, batchTo));

            current = batchTo.AddDays(1);
        }

        return batches;
    }

    /// <summary>
    /// Finds the last draw date in the database, or returns 01-08-2017 if empty.
    /// The sync range starts from the day after the last known draw date.
    /// </summary>
    private async Task<DateOnly> GetSyncFromDateAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var lastDraw = await db.Power655Results
            .AsNoTracking()
            .MaxAsync(r => (DateOnly?)r.DrawDate, ct);

        if (lastDraw.HasValue)
        {
            _logger.LogDebug("Power655SyncJob: last draw date in DB is {LastDraw}", lastDraw.Value);
            return lastDraw.Value.AddDays(1);
        }

        _logger.LogInformation("Power655SyncJob: no existing data, starting from {StartDate}", FirstDrawDate);
        return FirstDrawDate;
    }

    /// <summary>
    /// Upserts results into the database. Skips draws that already exist (by DrawDate).
    /// </summary>
    private async Task<int> UpsertResultsAsync(
    IReadOnlyList<Power655ResultItem> results,
    CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var parsedResults = results
            .Select(item => new
            {
                Item = item,
                DrawDate = ParseDrawDate(item.DrawDate)
            })
            .ToList();

        var incomingDates = parsedResults
            .Select(x => x.DrawDate)
            .ToHashSet();

        var existingDates = await db.Power655Results
            .Where(r => incomingDates.Contains(r.DrawDate))
            .Select(r => r.DrawDate)
            .ToListAsync(ct);

        var existsSet = existingDates.ToHashSet();

        var entities = new List<Power655Result>();

        foreach (var result in parsedResults)
        {
            // Skip if already exists in database or duplicated in the current batch.
            if (!existsSet.Add(result.DrawDate))
            {
                continue;
            }

            entities.Add(Power655Result.Create(
                result.DrawDate,
                result.Item.DayOfWeek,
                result.Item.Numbers[0],
                result.Item.Numbers[1],
                result.Item.Numbers[2],
                result.Item.Numbers[3],
                result.Item.Numbers[4],
                result.Item.Numbers[5],
                result.Item.BonusNumber,
                result.Item.Jackpot1Value,
                result.Item.Jackpot2Value));
        }

        if (entities.Count == 0)
        {
            return 0;
        }

        db.Power655Results.AddRange(entities);

        await db.SaveChangesAsync(ct);

        await LinkPredictionsAsync(db, entities, ct);

        _logger.LogDebug(
            "Power655SyncJob: inserted {Count} new results",
            entities.Count);

        return entities.Count;
    }

    /// <summary>
    /// Links newly inserted results to predictions for the same draw date,
    /// calculating matched numbers.
    /// </summary>
    private async Task LinkPredictionsAsync(
        ApplicationDbContext db,
        List<Power655Result> newResults,
        CancellationToken ct)
    {
        var drawDates = newResults.Select(r => r.DrawDate).ToHashSet();

        var predictions = await db.Power655Predictions
            .Where(p => p.Power655ResultId == null && drawDates.Contains(p.TargetDrawDate))
            .ToListAsync(ct);

        if (predictions.Count == 0)
        {
            return;
        }

        var resultByDate = newResults.ToDictionary(r => r.DrawDate);

        foreach (var prediction in predictions)
        {
            if (!resultByDate.TryGetValue(prediction.TargetDrawDate, out var result))
            {
                continue;
            }

            var predictedNumbers = new[] { prediction.Num1, prediction.Num2, prediction.Num3, prediction.Num4, prediction.Num5, prediction.Num6 };
            var actualNumbers = new[] { result.Num1, result.Num2, result.Num3, result.Num4, result.Num5, result.Num6 };
            var matched = predictedNumbers.Intersect(actualNumbers).Count();

            prediction.SetResult(result.Id, matched);
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Power655SyncJob: linked {Count} predictions to new results",
            predictions.Count);
    }

    private static DateOnly ParseDrawDate(string value)
    {
        var parts = value.Split(',', StringSplitOptions.TrimEntries);

        var dateText = parts.Length > 1
            ? parts[^1]
            : parts[0];

        return DateOnly.ParseExact(
            dateText,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture);
    }
}
