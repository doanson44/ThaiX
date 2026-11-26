using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Common.Models.MarketScanner;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24Hr;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBybitLinearTickers;
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
public sealed class MexcMarketScannerJob(
    IMediator mediator,
    IMarketSnapshotCache snapshotCache,
    IMarketScannerExchangeSettingsService exchangeSettingsService,
    IMemoryCache memoryCache,
    IEnumerable<IMarketSignalDetector> detectors,
    IMarketSignalCooldownService cooldownService,
    INotificationRouter notificationRouter,
    IOptions<MarketScannerOptions> options,
    IOptions<AffiliateOptions> affiliateOptions,
    IHangfireJobState hangfireJobState,
    ILogger<MexcMarketScannerJob> logger) : HangfireJobBase(hangfireJobState, logger)
{
    private readonly MarketScannerOptions _options = options.Value;
    private readonly ILogger<MexcMarketScannerJob> _logger = logger;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.MexcMarketScannerJob)) return;

        using var _ = await AcquireJobLockAsync(HangfireJobIds.MexcMarketScannerJob, cancellationToken);

        if (!_options.Enabled)
        {
            _logger.LogDebug("MexcMarketScannerJob is disabled via configuration");
            return;
        }

        _logger.LogInformation("MexcMarketScannerJob started");

        // 1. Load active rules via MediatR (benefits from query cache)
        var rules = await mediator.Send(
            new GetAllMarketScannerRulesQuery { IsEnabled = true },
            cancellationToken);

        if (rules.Count == 0)
        {
            _logger.LogDebug("MexcMarketScannerJob: no active rules configured, skipping");
            return;
        }

        // 2. Load exchange requirements and refresh enabled exchange symbol lists when their 24-hour cache expires.
        var exchangeSettings = await exchangeSettingsService.GetAsync(cancellationToken);
        HashSet<string>? bybitSymbols = null;
        HashSet<string>? binanceSymbols = null;

        if (exchangeSettings.RequiredBybit)
        {
            try
            {
                bybitSymbols = await memoryCache.GetOrCreateAsync("MarketScanner:BybitLinearSymbols", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                    var response = await mediator.Send(new GetBybitLinearTickersQuery(), cancellationToken);
                    if (!response.Success)
                        throw new InvalidOperationException(response.Message ?? "Failed to retrieve Bybit linear symbols.");

                    return response.Data
                        .Where(ticker => !string.IsNullOrWhiteSpace(ticker.Symbol))
                        .Select(ticker => NormalizeSymbol(ticker.Symbol))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MexcMarketScannerJob could not retrieve Bybit symbols; skipping the Bybit filter");
            }
        }

        if (exchangeSettings.RequiredBinance)
        {
            try
            {
                binanceSymbols = await memoryCache.GetOrCreateAsync("MarketScanner:BinanceFuturesSymbols", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                    var response = await mediator.Send(new GetBinanceFuturesTicker24HrQuery(), cancellationToken);
                    if (!response.Success)
                        throw new InvalidOperationException(response.Message ?? "Failed to retrieve Binance futures symbols.");

                    return response.Data
                        .Where(ticker => !string.IsNullOrWhiteSpace(ticker.Symbol))
                        .Select(ticker => NormalizeSymbol(ticker.Symbol))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MexcMarketScannerJob could not retrieve Binance symbols; skipping the Binance filter");
            }
        }

        // 3. Fetch MEXC contract tickers (includes funding rate)
        IReadOnlyList<MexcContractSnapshotDto> tickers;
        try
        {
            tickers = await mediator.Send(new GetMexcContractSnapshotsQuery { BypassCache = true }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MexcMarketScannerJob failed to fetch contract tickers");
            return;
        }

        if (tickers.Count == 0)
        {
            _logger.LogWarning("MexcMarketScannerJob: empty or failed ticker response");
            return;
        }

        var scanTickers = tickers
            .Where(ticker =>
                ticker.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase) &&
                !IsStockSymbol(ticker.Symbol))
            .ToArray();

        var now = DateTimeOffset.UtcNow;

        // 4. Project tickers to snapshots and add to cache
        foreach (var ticker in scanTickers)
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

        // 5. Clean up old snapshots
        var retention = TimeSpan.FromMinutes(_options.MaxSnapshotRetentionMinutes);
        snapshotCache.Cleanup(retention);

        // 6. Build detector lookup by signal type
        var detectorMap = detectors.ToDictionary(d => d.SignalType);

        // 7. Run detectors for each rule against every relevant symbol
        var referenceTolerance = TimeSpan.FromSeconds(30);
        foreach (var rule in rules)
        {
            if (!detectorMap.TryGetValue(rule.SignalType, out var detector))
                continue;

            var window = TimeSpan.FromMinutes((int)rule.Window);
            var targetReferenceTime = now - window;

            foreach (var ticker in scanTickers)
            {
                if (cooldownService.IsOnCooldown(ticker.Symbol, rule.SignalType))
                    continue;

                // Include the tolerance in the query so a slightly older reference is not filtered out.
                var snapshots = snapshotCache.GetSnapshots(ticker.Symbol, window + referenceTolerance);
                if (snapshots.Count < 2)
                    continue;

                var referenceSnapshot = snapshots
                    .Where(snapshot =>
                        snapshot.CapturedAtUtc >= targetReferenceTime - referenceTolerance &&
                        snapshot.CapturedAtUtc <= targetReferenceTime + referenceTolerance)
                    .MinBy(snapshot => (snapshot.CapturedAtUtc - targetReferenceTime).Duration());

                if (referenceSnapshot is null)
                    continue;

                var latestSnapshot = snapshots[^1];
                IReadOnlyList<MarketTickerSnapshot> comparisonSnapshots = [referenceSnapshot, latestSnapshot];
                var signals = detector.Detect(rule, comparisonSnapshots);
                foreach (var signal in signals)
                {
                    var normalizedSymbol = NormalizeSymbol(signal.Symbol);
                    if (bybitSymbols is { Count: > 0 } && !bybitSymbols.Contains(normalizedSymbol))
                        continue;

                    if (binanceSymbols is { Count: > 0 } && !binanceSymbols.Contains(normalizedSymbol))
                        continue;

                    cooldownService.MarkTriggered(signal.Symbol, signal.Type);
                    await DispatchSignalAsync(signal, cancellationToken);
                }
            }
        }

        _logger.LogInformation("MexcMarketScannerJob completed");
    }

    private static string NormalizeSymbol(string symbol) =>
        symbol.Replace("_", string.Empty, StringComparison.Ordinal).Trim().ToUpperInvariant();

    private static bool IsStockSymbol(string symbol) =>
        symbol.EndsWith("STOCK_USDT", StringComparison.OrdinalIgnoreCase) ||
        symbol.EndsWith("STOCKUSDT", StringComparison.OrdinalIgnoreCase);

    private async Task DispatchSignalAsync(MarketSignalResult signal, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
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
