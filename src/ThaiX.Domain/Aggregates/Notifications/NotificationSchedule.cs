using ThaiX.Domain.Aggregates.Notifications.Enums;
using ThaiX.Domain.Aggregates.Notifications.ValueObjects;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Notifications;

public sealed class NotificationSchedule : BaseAuditableEntity
{
    private NotificationSchedule() { }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string TemplateKey { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public string? DataJson { get; private set; }
    public ScheduleRecurrence Recurrence { get; private set; } = null!;
    public ScheduleStatus Status { get; private set; }
    public string TimeZoneId { get; private set; } = null!;
    public TimeOnly ExecuteTimeLocal { get; private set; }
    public DateTime? OneTimeAtLocal { get; private set; }
    public DateTime? StartDateLocal { get; private set; }
    public DateTime? StartAtUtc { get; private set; }
    public DateTime? EndAtUtc { get; private set; }
    public DateTime? LastTriggeredAtUtc { get; private set; }
    public DateTime? LastSuccessfulAtUtc { get; private set; }
    public DateTime? NextExecuteAtUtc { get; private set; }
    public int FailureCount { get; private set; }
    public int MaxConsecutiveFailures { get; private set; }
    public int ExecutionCount { get; private set; }

    public static NotificationSchedule Create(
        string name,
        string templateKey,
        string subject,
        string body,
        ScheduleRecurrence recurrence,
        string timeZoneId,
        TimeOnly executeTimeLocal,
        DateTime? oneTimeAtLocal,
        DateTime? startDateLocal,
        int maxConsecutiveFailures = 3)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        return new()
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            TemplateKey = templateKey.Trim(),
            Subject = subject.Trim(),
            Body = body.Trim(),
            Recurrence = recurrence,
            Status = ScheduleStatus.Draft,
            TimeZoneId = timeZoneId.Trim(),
            ExecuteTimeLocal = executeTimeLocal,
            OneTimeAtLocal = oneTimeAtLocal,
            StartDateLocal = startDateLocal,
            MaxConsecutiveFailures = maxConsecutiveFailures
        };
    }

    public void Update(
        string name,
        string? description,
        string templateKey,
        string subject,
        string body,
        string? dataJson,
        ScheduleRecurrence recurrence,
        TimeOnly executeTimeLocal,
        DateTime? oneTimeAtLocal,
        DateTime? startDateLocal,
        DateTime? endAtUtc,
        int maxConsecutiveFailures)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        Name = name.Trim();
        Description = description?.Trim();
        TemplateKey = templateKey.Trim();
        Subject = subject.Trim();
        Body = body.Trim();
        DataJson = string.IsNullOrWhiteSpace(dataJson) ? null : dataJson.Trim();
        Recurrence = recurrence;
        ExecuteTimeLocal = executeTimeLocal;
        OneTimeAtLocal = oneTimeAtLocal;
        StartDateLocal = startDateLocal;
        EndAtUtc = endAtUtc;
        MaxConsecutiveFailures = maxConsecutiveFailures;
    }

    public void Activate(DateTime startAtUtc, DateTime nextExecuteAtUtc)
    {
        Status = ScheduleStatus.Active;
        StartAtUtc = startAtUtc;
        NextExecuteAtUtc = nextExecuteAtUtc;
        FailureCount = 0;
    }

    public void Pause()
    {
        if (Status != ScheduleStatus.Active) return;
        Status = ScheduleStatus.Paused;
    }

    public void Resume(DateTime nextExecuteAtUtc)
    {
        if (Status != ScheduleStatus.Paused) return;
        Status = ScheduleStatus.Active;
        NextExecuteAtUtc = nextExecuteAtUtc;
    }

    public void Disable()
    {
        Status = ScheduleStatus.Disabled;
    }

    public void Delete()
    {
        Status = ScheduleStatus.Deleted;
    }

    public void Complete()
    {
        Status = ScheduleStatus.Completed;
    }

    public void RecordExecution(DateTime triggeredAtUtc, DateTime nextExecuteAtUtc)
    {
        LastTriggeredAtUtc = triggeredAtUtc;
        NextExecuteAtUtc = nextExecuteAtUtc;
        ExecutionCount++;
    }

    public void RecordSuccess(DateTime completedAtUtc)
    {
        LastSuccessfulAtUtc = completedAtUtc;
        FailureCount = 0;
    }

    public void RecordFailure()
    {
        FailureCount++;
        if (FailureCount >= MaxConsecutiveFailures)
        {
            Status = ScheduleStatus.Disabled;
        }
    }

    public bool IsDue(DateTime utcNow)
        => Status == ScheduleStatus.Active
           && NextExecuteAtUtc <= utcNow;

    public bool IsExpired(DateTime utcNow)
        => EndAtUtc is not null && EndAtUtc.Value <= utcNow;
}
