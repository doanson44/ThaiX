using ThaiX.Domain.Aggregates.Notifications.Enums;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Notifications;

public sealed class NotificationScheduleExecution : BaseAuditableEntity
{
    private NotificationScheduleExecution() { }

    public Guid ScheduleId { get; private set; }
    public NotificationSchedule Schedule { get; private set; } = null!;
    public ScheduleExecutionStatus Status { get; private set; }
    public DateTime OccurrenceTimeUtc { get; private set; }
    public int OccurrenceOrdinal { get; private set; }
    public DateTime TriggeredAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public Guid? NotificationId { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static NotificationScheduleExecution Create(
        Guid scheduleId,
        DateTime occurrenceTimeUtc,
        int ordinal)
        => new()
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            Status = ScheduleExecutionStatus.Claimed,
            OccurrenceTimeUtc = occurrenceTimeUtc,
            OccurrenceOrdinal = ordinal,
            TriggeredAtUtc = DateTime.UtcNow
        };

    public void MarkRunning() => Status = ScheduleExecutionStatus.Running;

    public void MarkSucceeded(Guid notificationId, DateTime completedAtUtc)
    {
        Status = ScheduleExecutionStatus.Succeeded;
        NotificationId = notificationId;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkFailed(string errorCode, string errorMessage, DateTime completedAtUtc)
    {
        Status = ScheduleExecutionStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkSkipped(string reason, DateTime completedAtUtc)
    {
        Status = ScheduleExecutionStatus.Skipped;
        ErrorMessage = reason;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkMisfired(string reason, DateTime completedAtUtc)
    {
        Status = ScheduleExecutionStatus.Misfired;
        ErrorMessage = reason;
        CompletedAtUtc = completedAtUtc;
    }
}
