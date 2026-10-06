using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Common.Models.MarketScanner;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractSnapshots;
using ThaiX.Application.Features.MarketScanner.Queries.GetAllMarketScannerRules;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that fetches the full MEXC futures snapshot every 3 minutes,
/// stores rolling in-memory data, detects anomalies via configured rules, and dispatches
/// Slack notifications. This is a system-level anomaly detection engine.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class MexcMarketScannerJob(
    IMediator mediator,
    IMarketSnapshotCache snapshotCache,
    IEnumerable<IMarketSignalDetector> detectors,
    IMarketSignalCooldownService cooldownService,
    INotificationRouter notificationRouter,
    IOptions<MarketScannerOptions> options,
    IOptions<AffiliateOptions> affiliateOptions,
    IHangfireJobState hangfireJobState,
    ILogger<MexcMarketScannerJob> logger) : HangfireJobBase(hangfireJobState, logger)
{
    private readonly MarketScannerOptions _options = options.Value;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.MexcMarketScannerJob)) return;

        if (!_options.Enabled)
        {
            logger.LogDebug("MexcMarketScannerJob is disabled via configuration");
            return;
        }

        logger.LogInformation("MexcMarketScannerJob started");

        // 1. Load active rules via MediatR (benefits from query cache)
        var rules = await mediator.Send(
            new GetAllMarketScannerRulesQuery { IsEnabled = true },
            cancellationToken);

        if (rules.Count == 0)
        {
            logger.LogDebug("MexcMarketScannerJob: no active rules configured, skipping");
            return;
        }

        // 2. Fetch MEXC contract tickers (includes funding rate)
        IReadOnlyList<MexcContractSnapshotDto> tickers;
        try
        {
            tickers = await mediator.Send(new GetMexcContractSnapshotsQuery { BypassCache = true }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MexcMarketScannerJob failed to fetch contract tickers");
            return;
        }

        if (tickers.Count == 0)
        {
            logger.LogWarning("MexcMarketScannerJob: empty or failed ticker response");
            return;
        }

        var now = DateTimeOffset.UtcNow;

        // 3. Project tickers to snapshots and add to cache
        foreach (var ticker in tickers)
        {
            var snapshot = new MarketTickerSnapshot
            {
                Symbol = ticker.Symbol,
                Price = ticker.LastPrice,
                FundingRate = ticker.FundingRate,
                Volume24h = ticker.Volume24h,
                CapturedAtUtc = now
            };

            snapshotCache.AddSnapshot(snapshot);
        }

        // 4. Clean up old snapshots
        var retention = TimeSpan.FromMinutes(_options.MaxSnapshotRetentionMinutes);
        snapshotCache.Cleanup(retention);

        // 5. Build detector lookup by signal type
        var detectorMap = detectors.ToDictionary(d => d.SignalType);

        // 6. Run detectors for each rule against every relevant symbol
        foreach (var rule in rules)
        {
            if (!detectorMap.TryGetValue(rule.SignalType, out var detector))
                continue;

            var window = TimeSpan.FromMinutes((int)rule.Window);

            foreach (var ticker in tickers)
            {
                if (cooldownService.IsOnCooldown(ticker.Symbol, rule.SignalType))
                    continue;

                var snapshots = snapshotCache.GetSnapshots(ticker.Symbol, window);
                if (snapshots.Count < 2)
                    continue;

                var signals = detector.Detect(rule, snapshots);
                foreach (var signal in signals)
                {
                    cooldownService.MarkTriggered(signal.Symbol, signal.Type);
                    await DispatchSignalAsync(signal, cancellationToken);
                }
            }
        }

        logger.LogInformation("MexcMarketScannerJob completed");
    }

    private async Task DispatchSignalAsync(MarketSignalResult signal, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "MarketScanner signal detected: {Symbol} {Type} Value={Value}",
            signal.Symbol, signal.Type, signal.Value);

        var sections = new List<NotificationSection>();
        var affiliateUrl = affiliateOptions.Value.MexcAffiliateUrl;
        if (affiliateOptions.Value.Enabled && !string.IsNullOrWhiteSpace(affiliateUrl))
        {
            sections.Add(new NotificationSection { Text = $"Trade on MEXC: {affiliateUrl}" });
        }

        await notificationRouter.DispatchAsync(new NotificationMessage
        {
            Kind = NotificationKind.MarketScanner,
            Card = new NotificationCard
            {
                Category = "Market Scanner",
                Title = signal.Symbol,
                Type = NotificationCardType.Signal,
                Severity = NotificationSeverity.Warning,
                Metrics =
                [
                    new() { Label = "Signal", Value = signal.Type.ToString() },
                    new() { Label = "Value", Value = signal.Value.ToString(CultureInfo.InvariantCulture) }
                ],
                Sections = sections
            }
        }, cancellationToken);
    }
}
