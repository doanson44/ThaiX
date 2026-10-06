using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Notifications;

public sealed class NotificationDelivery : BaseAuditableEntity
{
    private NotificationDelivery()
    {
    }

    public Guid NotificationId { get; private set; }

    public Notification Notification { get; private set; } = null!;

    public NotificationChannel Channel { get; private set; }

    public string Destination { get; private set; } = null!;

    public string Provider { get; private set; } = null!;

    public NotificationDeliveryStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public int MaxAttempts { get; private set; }

    public DateTime? NextAttemptAtUtc { get; private set; }

    public DateTime? LastAttemptAtUtc { get; private set; }

    public string? ProviderMessageId { get; private set; }

    public string? ErrorCode { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string IdempotencyKey { get; private set; } = null!;

    public string? RenderedPayloadJson { get; private set; }

    internal static NotificationDelivery Create(
        Guid notificationId,
        NotificationChannel channel,
        string destination,
        string provider,
        string idempotencyKey,
        int maxAttempts,
        DateTime? scheduledAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);

        return new NotificationDelivery
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            Channel = channel,
            Destination = destination.Trim(),
            Provider = provider.Trim(),
            Status = NotificationDeliveryStatus.Pending,
            AttemptCount = 0,
            MaxAttempts = maxAttempts,
            NextAttemptAtUtc = scheduledAtUtc,
            IdempotencyKey = idempotencyKey.Trim()
        };
    }

    public bool CanAttempt(DateTime utcNow)
    {
        return Status is NotificationDeliveryStatus.Pending or NotificationDeliveryStatus.RetryScheduled
            && (NextAttemptAtUtc is null || NextAttemptAtUtc <= utcNow)
            && AttemptCount < MaxAttempts;
    }

    public void MarkRendering()
    {
        if (Status is NotificationDeliveryStatus.Sent or NotificationDeliveryStatus.Cancelled)
        {
            return;
        }

        Status = NotificationDeliveryStatus.Rendering;
    }

    public void MarkPayloadRendered(string payloadJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);

        RenderedPayloadJson = payloadJson.Trim();
    }

    public void StartSending(DateTime utcNow)
    {
        if (AttemptCount >= MaxAttempts)
        {
            throw new InvalidOperationException("Delivery has no remaining attempts.");
        }

        Status = NotificationDeliveryStatus.Sending;
        AttemptCount++;
        LastAttemptAtUtc = utcNow;
        NextAttemptAtUtc = null;
        ErrorCode = null;
        ErrorMessage = null;
    }

    public void MarkSent(string? providerMessageId)
    {
        Status = NotificationDeliveryStatus.Sent;
        ProviderMessageId = string.IsNullOrWhiteSpace(providerMessageId)
            ? null
            : providerMessageId.Trim();
        NextAttemptAtUtc = null;
        ErrorCode = null;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorCode, string errorMessage, DateTime? nextAttemptAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        ErrorCode = TrimToMaxLength(errorCode, 128);
        ErrorMessage = TrimToMaxLength(errorMessage, 2000);

        if (AttemptCount >= MaxAttempts || nextAttemptAtUtc is null)
        {
            Status = NotificationDeliveryStatus.Failed;
            NextAttemptAtUtc = null;
            return;
        }

        Status = NotificationDeliveryStatus.RetryScheduled;
        NextAttemptAtUtc = nextAttemptAtUtc;
    }

    public void MarkSkipped(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Status = NotificationDeliveryStatus.Skipped;
        ErrorCode = "skipped";
        ErrorMessage = TrimToMaxLength(reason, 2000);
        NextAttemptAtUtc = null;
    }

    public void Cancel(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Status = NotificationDeliveryStatus.Cancelled;
        ErrorCode = "cancelled";
        ErrorMessage = TrimToMaxLength(reason, 2000);
        NextAttemptAtUtc = null;
    }

    private static string TrimToMaxLength(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength
            ? trimmed
            : trimmed[..maxLength];
    }
}
