using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTopStocks;
using ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;
using ThaiX.Application.Trading;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that runs every Monday at 09:00 VNT.
/// Retrieves VnDirect top stocks, selects top 5 by composite score, then computes trade suggestions and sends a notification.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class TopStocksWeeklySuggestionJob : HangfireJobBase
{
    private const int TopCount = 5;
    private const int CandidateScanLimit = 50;
    private static readonly IReadOnlyList<TimeframeRequest> TimeframeRequests =
    [
        new("Day1", false),
        new("Week1", true),
        new("Month1", true)
    ];

    private readonly IMediator _mediator;
    private readonly INotificationRouter _notificationRouter;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<TopStocksWeeklySuggestionJob> _logger;

    public TopStocksWeeklySuggestionJob(
        IMediator mediator,
        INotificationRouter notificationRouter,
        IBackgroundJobClient backgroundJobClient,
        IDateTimeProvider dateTimeProvider,
        IHangfireJobState hangfireJobState,
        ILogger<TopStocksWeeklySuggestionJob> logger) : base(hangfireJobState, logger)
    {
        _mediator = mediator;
        _notificationRouter = notificationRouter;
        _backgroundJobClient = backgroundJobClient;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.TopStocksWeeklySuggestionJob)) return;

        _logger.LogInformation("TopStocksWeeklySuggestionJob started");
        var started = DateTime.UtcNow;
        NotificationMessage? notificationMessage = null;
        var notificationToken = cancellationToken;

        try
        {
            var topStocks = await _mediator.Send(
                new GetVnDirectTopStocksQuery
                {
                    IncludeEnrichment = true,
                    MaxEventsLookbackDays = 30
                },
                cancellationToken);

            if (!topStocks.Success || topStocks.Data.Count == 0)
            {
                _logger.LogWarning("TopStocksWeeklySuggestionJob: no stock data returned (Success={Success})", topStocks.Success);
                notificationMessage = new NotificationMessage
                {
                    Kind = NotificationKind.GoodEntryStocks,
                    Card = new NotificationCard
                    {
                        Category = "Weekly Suggestion",
                        Title = "Top Stocks Suggestion Report",
                        Type = NotificationCardType.Signal,
                        Severity = NotificationSeverity.Warning,
                        Metrics =
                        [
                            new() { Label = "Status", Value = "No Data" },
                            new() { Label = "Message", Value = "Data source returned no stock candidates for this weekly run." }
                        ]
                    }
                };
                return;
            }

            var candidates = topStocks.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .DistinctBy(x => x.Code)
                .OrderByDescending(x => x.CompositeScore)
                .ThenByDescending(x => x.TechnicalScore)
                .ThenByDescending(x => x.PortfolioScore)
                .Take(CandidateScanLimit)
                .Select(x => new StockCandidate(x.Code, x.CompositeScore))
                .ToList();

            if (candidates.Count == 0)
            {
                _logger.LogWarning("TopStocksWeeklySuggestionJob: no valid stock code after filtering");
                return;
            }

            var timeframeResults = new List<TimeframeRunResult>(TimeframeRequests.Count);
            foreach (var timeframeRequest in TimeframeRequests)
            {
                var timeframeResult = await RunTimeframeAsync(candidates, timeframeRequest, cancellationToken);
                timeframeResults.Add(timeframeResult);
            }

            var elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;
            var hasAnyPick = timeframeResults.Any(x => x.Picked > 0);
            EnqueuePersistReport(started, elapsed, timeframeResults);

            notificationMessage = new NotificationMessage
            {
                Kind = NotificationKind.GoodEntryStocks,
                Card = new NotificationCard
                {
                    Category = "Weekly Suggestion",
                    Title = "Top Stocks Suggestion Report",
                    Type = NotificationCardType.Signal,
                    Severity = hasAnyPick ? NotificationSeverity.Info : NotificationSeverity.Warning,
                    Metrics =
                    [
                        new() { Label = "Run", Value = BuildHeaderLine() },
                        new() { Label = "Duration", Value = $"{elapsed}s" },
                        new() { Label = "Picks", Value = $"{timeframeResults.Sum(x => x.Picked)}/{TopCount * TimeframeRequests.Count}" }
                    ],
                    Sections = BuildSections(BuildHeaderLine(), elapsed, timeframeResults)
                }
            };

            _logger.LogInformation("TopStocksWeeklySuggestionJob completed in {Elapsed}s", elapsed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TopStocksWeeklySuggestionJob failed");
            notificationToken = CancellationToken.None;
            notificationMessage = new NotificationMessage
            {
                Kind = NotificationKind.SystemAlert,
                Card = new NotificationCard
                {
                    Category = "System",
                    Title = "Top Stocks Weekly Suggestion Job Failed",
                    Type = NotificationCardType.Error,
                    Severity = NotificationSeverity.Error,
                    Metrics =
                    [
                        new() { Label = "Code", Value = "TOPSTOCKS_WEEKLY_SUGGESTION_FAILED" },
                        new() { Label = "Message", Value = ex.Message }
                    ]
                }
            };

            throw;
        }
        finally
        {
            // Ensure exactly one notification per job run.
            if (notificationMessage is not null)
            {
                await _notificationRouter.DispatchAsync(notificationMessage, notificationToken);
            }
        }
    }

    private string BuildHeaderLine()
    {
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _dateTimeProvider.TimeZone);
        return nowVn.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private async Task<TimeframeRunResult> RunTimeframeAsync(
        IReadOnlyList<StockCandidate> candidates,
        TimeframeRequest timeframeRequest,
        CancellationToken cancellationToken)
    {
        var suggestions = new List<(int Rank, string Symbol, decimal CompositeScore, TradeSuggestionDto Suggestion)>();
        var picked = 0;
        var scanned = 0;

        for (var i = 0; i < candidates.Count; i++)
        {
            if (picked >= TopCount)
            {
                break;
            }

            var stock = candidates[i];
            scanned++;

            try
            {
                var suggestion = await _mediator.Send(
                    new GetTradeSuggestionQuery
                    {
                        Symbol = stock.Code,
                        MarketType = MarketType.Stock,
                        Timeframe = timeframeRequest.Timeframe,
                        UseLongTermTimeframe = timeframeRequest.UseLongTermTimeframe,
                        MarketRegime = MarketRegime.RiskOn,
                        EventRisk = EventRisk.Low
                    },
                    cancellationToken);

                if (!HasEntry(suggestion))
                {
                    continue;
                }

                picked++;
                suggestions.Add((picked, stock.Code, stock.CompositeScore, suggestion));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "TopStocksWeeklySuggestionJob: failed to compute suggestion for {Code} ({Timeframe})",
                    stock.Code,
                    timeframeRequest.Timeframe);
            }
        }

        return new TimeframeRunResult(timeframeRequest.Timeframe, scanned, candidates.Count, picked, suggestions);
    }

    /// <summary>
    /// Builds structured sections for notification channels that support them (e.g. Telegram).
    /// Each symbol gets its own section with signal, entry/SL/TP and score.
    /// </summary>
    private static IReadOnlyList<NotificationSection> BuildSections(
        string runTime,
        int elapsedSeconds,
        IReadOnlyList<TimeframeRunResult> timeframeResults)
    {
        var sections = new List<NotificationSection>
        {
            new() { Text = $"Run: {runTime} | Duration: {elapsedSeconds}s" }
        };

        foreach (var tf in timeframeResults)
        {
            sections.Add(new NotificationSection
            {
                Title = $"Timeframe: {tf.Timeframe} ({tf.Picked}/{TopCount} picked, scanned {tf.Scanned}/{tf.TotalCandidates})",
                Text = string.Empty
            });

            if (tf.Suggestions.Count == 0)
            {
                sections.Add(new NotificationSection { Text = "No symbols met entry criteria." });
                continue;
            }

            foreach (var (rank, symbol, score, suggestion) in tf.Suggestions)
            {
                var signal = suggestion.Signal?.ToUpperInvariant() ?? "NONE";
                var signalEmoji = signal switch
                {
                    "BUY" => "\ud83d\udfe2",
                    "SELL" => "\ud83d\udd34",
                    "HOLD" => "\ud83d\udfe1",
                    _ => "\u26aa"
                };

                var body = new System.Text.StringBuilder();
                body.Append($"Signal: {signalEmoji} {signal} (conf: {suggestion.Confidence:0.00})\n");
                body.Append($"Entry: {FormatPrice(suggestion.EntryPrice)}  SL: {FormatPrice(suggestion.StopLoss)}\n");
                body.Append($"TP1: {FormatPrice(suggestion.TakeProfit1)}  TP2: {FormatPrice(suggestion.TakeProfit2)}\n");
                body.Append($"Score: {score:0.00}");

                sections.Add(new NotificationSection
                {
                    Title = $"#{rank} {symbol}",
                    Text = body.ToString().Trim()
                });
            }
        }

        return sections;
    }

    private static string FormatPrice(decimal? value)
        => value.HasValue
            ? value.Value.ToString("0.####", CultureInfo.InvariantCulture)
            : "-";

    private static bool HasEntry(TradeSuggestionDto suggestion)
    {
        if (suggestion.EntryPrice is null)
        {
            return false;
        }

        return !string.Equals(suggestion.Signal, "None", StringComparison.OrdinalIgnoreCase);
    }

    private void EnqueuePersistReport(
        DateTime runAtUtc,
        int elapsedSeconds,
        IReadOnlyList<TimeframeRunResult> timeframeResults)
    {
        var reportKey = $"TopStocksWeekly:{runAtUtc:yyyyMMddHHmmss}";
        var pickedItems = timeframeResults
            .SelectMany(x => x.Suggestions.Select(s => new WeeklySuggestionReportPersistItem(
                Timeframe: x.Timeframe,
                Rank: s.Rank,
                Symbol: s.Symbol,
                MarketType: SuggestionAssetClass.Stock,
                CompositeScore: s.CompositeScore,
                Signal: s.Suggestion.Signal ?? "NONE",
                Confidence: s.Suggestion.Confidence,
                EntryPrice: s.Suggestion.EntryPrice ?? 0m,
                StopLoss: s.Suggestion.StopLoss,
                TakeProfit1: s.Suggestion.TakeProfit1,
                TakeProfit2: s.Suggestion.TakeProfit2)))
            .ToList();

        var status = pickedItems.Count == 0
            ? WeeklySuggestionReportStatus.NoData
            : WeeklySuggestionReportStatus.Succeeded;

        var payload = new WeeklySuggestionReportPersistPayload(
            ReportKey: reportKey,
            RunAtUtc: runAtUtc,
            ElapsedSeconds: elapsedSeconds,
            CandidateScanLimit: CandidateScanLimit,
            TopCount: TopCount,
            AssetClass: SuggestionAssetClass.Stock,
            ReportType: SuggestionReportType.TopSuggestionsWeekly,
            Status: status,
            Items: pickedItems);

        _backgroundJobClient.Enqueue<PersistWeeklySuggestionReportJob>(
            x => x.RunAsync(payload, CancellationToken.None));
    }

    private sealed record TimeframeRequest(string Timeframe, bool UseLongTermTimeframe);

    private sealed record StockCandidate(string Code, decimal CompositeScore);

    private sealed record TimeframeRunResult(
        string Timeframe,
        int Scanned,
        int TotalCandidates,
        int Picked,
        IReadOnlyList<(int Rank, string Symbol, decimal CompositeScore, TradeSuggestionDto Suggestion)> Suggestions);
}
