using ThaiX.Domain.Aggregates.Notifications.Enums;
using ThaiX.Domain.Aggregates.Notifications.ValueObjects;

namespace ThaiX.Application.Features.Notifications.Scheduling.Dtos;

public sealed record NotificationScheduleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string TemplateKey { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public string? DataJson { get; init; }
    public ScheduleRecurrence Recurrence { get; init; } = null!;
    public ScheduleStatus Status { get; init; }
    public string TimeZoneId { get; init; } = null!;
    public TimeOnly ExecuteTimeLocal { get; init; }
    public DateTime? OneTimeAtLocal { get; init; }
    public DateTime? StartDateLocal { get; init; }
    public DateTime? StartAtUtc { get; init; }
    public DateTime? EndAtUtc { get; init; }
    public DateTime? LastTriggeredAtUtc { get; init; }
    public DateTime? LastSuccessfulAtUtc { get; init; }
    public DateTime? NextExecuteAtUtc { get; init; }
    public int FailureCount { get; init; }
    public int ExecutionCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed record NotificationScheduleListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public ScheduleType Type { get; init; }
    public ScheduleStatus Status { get; init; }
    public DateTime? NextExecuteAtUtc { get; init; }
    public int FailureCount { get; init; }
    public int ExecutionCount { get; init; }
    public DateTime? LastTriggeredAtUtc { get; init; }
}

public sealed record ScheduleExecutionDto
{
    public Guid Id { get; init; }
    public Guid ScheduleId { get; init; }
    public ScheduleExecutionStatus Status { get; init; }
    public DateTime OccurrenceTimeUtc { get; init; }
    public int OccurrenceOrdinal { get; init; }
    public DateTime TriggeredAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed record UpcomingScheduleDto
{
    public Guid ScheduleId { get; init; }
    public string ScheduleName { get; init; } = null!;
    public DateTime NextExecuteAtUtc { get; init; }
    public ScheduleType Type { get; init; }
}
