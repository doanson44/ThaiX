using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotSnapshots;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockSnapshots;
using ThaiX.Application.Features.PriceAlerts.Queries.GetAllPriceAlerts;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Domain.Aggregates.PriceAlerts;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that checks all enabled price alerts against current market prices.
/// - CryptoSpot alerts: batch-fetched from MEXC spot 24hr ticker (always).
/// - VnStock alerts: batch-fetched from VnDirect; skipped outside HOSE trading hours
///   (Monday-Friday, 09:30-15:00 Vietnam Time).
/// Sends a Slack notification and updates the alert trigger record when a condition is met.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class PriceAlertCheckerJob(
    IMediator mediator,
    IApplicationDbContext context,
    INotificationRouter notificationRouter,
    IOptions<PriceAlertCheckerOptions> options,
    IOptions<AffiliateOptions> affiliateOptions,
    IDateTimeProvider dateTimeProvider,
    IHangfireJobState hangfireJobState,
    ILogger<PriceAlertCheckerJob> logger) : HangfireJobBase(hangfireJobState, logger)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.PriceAlertCheckerJob)) return;

        logger.LogInformation("PriceAlertCheckerJob started");

        var alerts = await mediator.Send(new GetAllPriceAlertsQuery(), cancellationToken);
        if (alerts.Count == 0)
        {
            logger.LogDebug("PriceAlertCheckerJob: no active alerts, skipping");
            return;
        }

        var cryptoAlerts = alerts.Where(a => a.AssetType == AssetType.CryptoSpot).ToList();
        var stockAlerts = alerts.Where(a => a.AssetType == AssetType.VnStock).ToList();

        var triggered = new List<(Guid Id, string Symbol, AlertCondition Condition, decimal Target, decimal Actual, string? Note)>();

        // --- CryptoSpot: single batch fetch ---
        if (cryptoAlerts.Count > 0)
        {
            var spotTickers = await mediator.Send(new GetMexcSpotSnapshotsQuery(), cancellationToken);
            var priceMap = spotTickers.ToDictionary(t => t.Symbol, t => t.LastPrice, StringComparer.OrdinalIgnoreCase);

            foreach (var alert in cryptoAlerts)
            {
                if (!priceMap.TryGetValue(alert.Symbol, out var price)) continue;
                if (IsTriggered(alert.Condition, price, alert.TargetPrice))
                    triggered.Add((alert.Id, alert.Symbol, alert.Condition, alert.TargetPrice, price, alert.Note));
            }
        }

        // --- VnStock: single batch fetch (HOSE trading hours only: Mon-Fri 09:30-15:00 VNT) ---
        if (stockAlerts.Count > 0)
        {
            if (IsVnStockTradingTime())
            {
                var stockTickers = await mediator.Send(new GetVnDirectStockSnapshotsQuery(), cancellationToken);
                var stockPriceMap = stockTickers.ToDictionary(t => t.Symbol, t => t.LastPrice, StringComparer.OrdinalIgnoreCase);

                foreach (var alert in stockAlerts)
                {
                    if (!stockPriceMap.TryGetValue(alert.Symbol, out var price)) continue;
                    if (IsTriggered(alert.Condition, price, alert.TargetPrice))
                        triggered.Add((alert.Id, alert.Symbol, alert.Condition, alert.TargetPrice, price, alert.Note));
                }
            }
            else
            {
                logger.LogDebug("PriceAlertCheckerJob: skipping VnStock alerts outside trading hours");
            }
        }

        if (triggered.Count == 0)
        {
            logger.LogDebug("PriceAlertCheckerJob: no alerts triggered");
            return;
        }

        var now = DateTime.UtcNow;
        var alertIds = triggered.Select(t => t.Id).ToHashSet();

        // Load tracked entities for mutation
        var entities = await context.PriceAlerts
            .Where(a => alertIds.Contains(a.Id) && !a.IsDeleted && a.IsEnabled)
            .ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            entity.RecordTrigger(now);
        }

        await context.SaveChangesAsync(cancellationToken);

        // Dispatch notifications
        foreach (var (id, symbol, condition, target, actual, note) in triggered)
        {
            var conditionLabel = condition == AlertCondition.Above ? "above" : "below";
            var noteText = string.IsNullOrWhiteSpace(note) ? string.Empty : $"\nNote: {note}";

            var sections = new List<NotificationSection>();
            if (!string.IsNullOrWhiteSpace(noteText))
            {
                sections.Add(new NotificationSection { Text = noteText.Trim() });
            }

            var affiliateUrl = affiliateOptions.Value.MexcAffiliateUrl;
            if (affiliateOptions.Value.Enabled && !string.IsNullOrWhiteSpace(affiliateUrl))
            {
                sections.Add(new NotificationSection { Text = $"Trade on MEXC: {affiliateUrl}" });
            }

            var nowStr = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            await notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.PriceAlert,
                Card = new NotificationCard
                {
                    Category = "Price Alert",
                    Title = symbol,
                    Type = NotificationCardType.Alert,
                    Severity = NotificationSeverity.Info,
                    Metrics =
                    [
                        new() { Label = "Condition", Value = $"Price {conditionLabel} {target:G}" },
                        new() { Label = "Current", Value = $"{actual:G}" },
                        new() { Label = "Status", Value = "Triggered" },
                        new() { Label = "Time", Value = $"{nowStr} UTC" }
                    ],
                    Sections = sections
                }
            }, cancellationToken);
        }

        logger.LogInformation("PriceAlertCheckerJob completed: {Count} alert(s) triggered", triggered.Count);
    }

    private static bool IsTriggered(AlertCondition condition, decimal actual, decimal target) =>
        condition == AlertCondition.Above ? actual >= target : actual <= target;

    /// <summary>
    /// Returns true when the current time falls within the configured VnStock trading window.
    /// </summary>
    private bool IsVnStockTradingTime()
    {
        var cfg = options.Value;
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dateTimeProvider.TimeZone);

        if (now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return false;

        if (!TimeOnly.TryParse(cfg.VnStockTradingStart, out var start) ||
            !TimeOnly.TryParse(cfg.VnStockTradingEnd, out var end))
        {
            logger.LogWarning(
                "PriceAlertCheckerJob: invalid VnStockTradingStart/End config ('{Start}'/'{End}'), defaulting to allow",
                cfg.VnStockTradingStart, cfg.VnStockTradingEnd);
            return true;
        }

        var time = TimeOnly.FromTimeSpan(now.TimeOfDay);
        return time >= start && time <= end;
    }
}
