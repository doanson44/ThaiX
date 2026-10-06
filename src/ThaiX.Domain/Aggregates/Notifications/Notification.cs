using ThaiX.Domain.Common.Entities;
using ThaiX.Domain.Common.Events;

namespace ThaiX.Domain.Aggregates.Notifications;

public sealed class Notification : BaseAuditableEntity
{
    private readonly List<NotificationDelivery> _deliveries = [];

    private Notification()
    {
    }

    public NotificationKind Kind { get; private set; }

    public NotificationSeverity Severity { get; private set; }

    public string TemplateKey { get; private set; } = null!;

    public string Subject { get; private set; } = null!;

    public string Body { get; private set; } = null!;

    public string? DataJson { get; private set; }

    public Guid? RecipientUserId { get; private set; }

    public string? SourceEventId { get; private set; }

    public string DeduplicationKey { get; private set; } = null!;

    public DateTime? ScheduledAtUtc { get; private set; }

    public NotificationStatus Status { get; private set; }

    public IReadOnlyCollection<NotificationDelivery> Deliveries => _deliveries.AsReadOnly();

    public static Notification Create(
        NotificationKind kind,
        NotificationSeverity severity,
        string templateKey,
        string subject,
        string body,
        string? dataJson,
        Guid? recipientUserId,
        string? sourceEventId,
        string deduplicationKey,
        DateTime? scheduledAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(deduplicationKey);

        return new Notification
        {
            Id = Guid.NewGuid(),
            Kind = kind,
            Severity = severity,
            TemplateKey = templateKey.Trim(),
            Subject = subject.Trim(),
            Body = body.Trim(),
            DataJson = string.IsNullOrWhiteSpace(dataJson) ? null : dataJson.Trim(),
            RecipientUserId = recipientUserId,
            SourceEventId = string.IsNullOrWhiteSpace(sourceEventId) ? null : sourceEventId.Trim(),
            DeduplicationKey = deduplicationKey.Trim(),
            ScheduledAtUtc = scheduledAtUtc,
            Status = NotificationStatus.Pending
        };
    }

    public NotificationDelivery AddDelivery(
        NotificationChannel channel,
        string destination,
        string provider,
        string idempotencyKey,
        int maxAttempts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        var existing = _deliveries.FirstOrDefault(delivery =>
            delivery.Channel == channel
            && string.Equals(delivery.Destination, destination.Trim(), StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return existing;
        }

        var delivery = NotificationDelivery.Create(
            Id,
            channel,
            destination,
            provider,
            idempotencyKey,
            maxAttempts,
            ScheduledAtUtc);

        _deliveries.Add(delivery);
        Status = NotificationStatus.Ready;
        RaiseReadyForDelivery();

        return delivery;
    }

    public void Cancel(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        foreach (var delivery in _deliveries)
        {
            delivery.Cancel(reason);
        }

        Status = NotificationStatus.Cancelled;
    }

    public void RefreshStatus()
    {
        if (_deliveries.Count == 0)
        {
            Status = NotificationStatus.Pending;
            return;
        }

        if (_deliveries.All(static delivery => delivery.Status == NotificationDeliveryStatus.Sent))
        {
            Status = NotificationStatus.Delivered;
            return;
        }

        if (_deliveries.Any(static delivery => delivery.Status == NotificationDeliveryStatus.Sent))
        {
            Status = NotificationStatus.PartiallyDelivered;
            return;
        }

        if (_deliveries.All(static delivery =>
                delivery.Status is NotificationDeliveryStatus.Failed
                    or NotificationDeliveryStatus.Cancelled
                    or NotificationDeliveryStatus.Skipped))
        {
            Status = _deliveries.Any(static delivery => delivery.Status == NotificationDeliveryStatus.Failed)
                ? NotificationStatus.Failed
                : NotificationStatus.Cancelled;
            return;
        }

        Status = NotificationStatus.Ready;
    }

    private void RaiseReadyForDelivery()
    {
        if (DomainEvents.OfType<NotificationReadyForDeliveryEvent>().Any(e => e.NotificationId == Id))
        {
            return;
        }

        AddDomainEvent(new NotificationReadyForDeliveryEvent(Id, ScheduledAtUtc));
    }
}
