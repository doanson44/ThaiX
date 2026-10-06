using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Features.Notifications.Queries.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid Id) : IAppQuery<NotificationDto?>;

public sealed record NotificationDto
{
    public required Guid Id { get; init; }
    public required NotificationKind Kind { get; init; }
    public required NotificationSeverity Severity { get; init; }
    public required string TemplateKey { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? DataJson { get; init; }
    public Guid? RecipientUserId { get; init; }
    public string? SourceEventId { get; init; }
    public required string DeduplicationKey { get; init; }
    public DateTime? ScheduledAtUtc { get; init; }
    public required NotificationStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public required IReadOnlyList<NotificationDeliveryDto> Deliveries { get; init; }
}

public sealed record NotificationDeliveryDto
{
    public required Guid Id { get; init; }
    public required NotificationChannel Channel { get; init; }
    public required string Destination { get; init; }
    public required string Provider { get; init; }
    public required NotificationDeliveryStatus Status { get; init; }
    public required int AttemptCount { get; init; }
    public required int MaxAttempts { get; init; }
    public DateTime? NextAttemptAtUtc { get; init; }
    public DateTime? LastAttemptAtUtc { get; init; }
    public string? ProviderMessageId { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public required string IdempotencyKey { get; init; }
}
