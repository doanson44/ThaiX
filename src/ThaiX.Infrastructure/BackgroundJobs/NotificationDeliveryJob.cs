using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.BackgroundJobs;

public sealed class NotificationDeliveryJob
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEnumerable<INotificationRenderer> _renderers;
    private readonly IEnumerable<INotificationChannelProvider> _providers;
    private readonly INotificationDeliveryScheduler _scheduler;
    private readonly ILogger<NotificationDeliveryJob> _logger;

    public NotificationDeliveryJob(
        IApplicationDbContext dbContext,
        IEnumerable<INotificationRenderer> renderers,
        IEnumerable<INotificationChannelProvider> providers,
        INotificationDeliveryScheduler scheduler,
        ILogger<NotificationDeliveryJob> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _renderers = renderers ?? throw new ArgumentNullException(nameof(renderers));
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .Include(x => x.Deliveries)
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

        if (notification is null)
        {
            _logger.LogWarning("Notification {NotificationId} was not found for delivery", notificationId);
            return;
        }

        var utcNow = DateTime.UtcNow;
        var dueDeliveries = notification.Deliveries
            .Where(delivery => delivery.CanAttempt(utcNow))
            .ToList();

        if (dueDeliveries.Count == 0)
        {
            _logger.LogDebug("Notification {NotificationId} has no due deliveries", notificationId);
            return;
        }

        foreach (var delivery in dueDeliveries)
        {
            await DispatchDeliveryAsync(notification, delivery, cancellationToken);
        }

        notification.RefreshStatus();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDeliveryAsync(
        Notification notification,
        NotificationDelivery delivery,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        try
        {
            var renderer = _renderers.FirstOrDefault(x => x.Channel == delivery.Channel);
            if (renderer is null)
            {
                delivery.MarkSkipped($"No renderer registered for channel {delivery.Channel}.");
                return;
            }

            var provider = _providers.FirstOrDefault(x =>
                x.Channel == delivery.Channel
                && string.Equals(x.Provider, delivery.Provider, StringComparison.OrdinalIgnoreCase));

            if (provider is null)
            {
                delivery.MarkSkipped(
                    $"No provider registered for channel {delivery.Channel} and provider {delivery.Provider}.");
                return;
            }

            delivery.MarkRendering();
            var route = new NotificationRoute
            {
                Channel = delivery.Channel,
                Provider = delivery.Provider,
                Destination = delivery.Destination
            };

            var rendered = renderer.Render(CreateMessage(notification), route, delivery.IdempotencyKey);
            delivery.MarkPayloadRendered(rendered.PayloadJson);
            delivery.StartSending(utcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = await provider.SendAsync(rendered, cancellationToken);
            if (result.Success)
            {
                delivery.MarkSent(result.ProviderMessageId);
                _logger.LogInformation(
                    "Notification delivery {DeliveryId} sent through {Channel}",
                    delivery.Id,
                    delivery.Channel);
                return;
            }

            var nextAttemptAtUtc = GetNextAttemptAtUtc(delivery, result.RetryAfter, DateTime.UtcNow);
            delivery.MarkFailed(
                result.ErrorCode ?? "provider_failed",
                result.ErrorMessage ?? "Provider returned a failed notification result.",
                nextAttemptAtUtc);

            if (nextAttemptAtUtc is not null)
            {
                await _scheduler.ScheduleNotificationAsync(notification.Id, nextAttemptAtUtc, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Notification delivery {DeliveryId} failed through {Channel}",
                delivery.Id,
                delivery.Channel);

            var nextAttemptAtUtc = GetNextAttemptAtUtc(delivery, retryAfter: null, DateTime.UtcNow);
            delivery.MarkFailed(
                ex.GetType().Name,
                ex.Message,
                nextAttemptAtUtc);

            if (nextAttemptAtUtc is not null)
            {
                await _scheduler.ScheduleNotificationAsync(notification.Id, nextAttemptAtUtc, cancellationToken);
            }
        }
    }

    private static NotificationMessage CreateMessage(Notification notification)
    {
        return new NotificationMessage
        {
            Kind = notification.Kind,
            Severity = notification.Severity,
            Text = notification.Body,
            Title = notification.Subject,
            TemplateKey = notification.TemplateKey,
            RecipientUserId = notification.RecipientUserId,
            DataJson = notification.DataJson,
            SourceEventId = notification.SourceEventId,
            DeduplicationKey = notification.DeduplicationKey,
            ScheduledAtUtc = notification.ScheduledAtUtc,
            Card = NotificationCard.FromJson(notification.DataJson)
        };
    }

    private static DateTime? GetNextAttemptAtUtc(
        NotificationDelivery delivery,
        TimeSpan? retryAfter,
        DateTime utcNow)
    {
        if (delivery.AttemptCount >= delivery.MaxAttempts)
        {
            return null;
        }

        var delay = retryAfter ?? GetBackoffDelay(delivery.AttemptCount);
        return utcNow.Add(delay);
    }

    private static TimeSpan GetBackoffDelay(int attemptCount)
    {
        var attempt = Math.Max(1, attemptCount);
        var seconds = Math.Min(300, Math.Pow(2, attempt) * 15);
        var jitter = Random.Shared.Next(0, 10);
        return TimeSpan.FromSeconds(seconds + jitter);
    }
}
