namespace ThaiX.Client.Services.Notifications;

public sealed record CreateNotificationScheduleRequest
{
    public string Name { get; init; } = null!;
    public string TemplateKey { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public string? DataJson { get; init; }

    // Recurrence
    public string Type { get; init; } = "EveryXDays";
    public int? IntervalDays { get; init; }
    public string? DayOfWeek { get; init; }
    public int? DayOfMonth { get; init; }
    public string MonthlyOverflowPolicy { get; init; } = "SkipMonth";
    public string MisfirePolicy { get; init; } = "Skip";
    public int? CatchUpLimit { get; init; }

    public string TimeZoneId { get; init; } = "Asia/Ho_Chi_Minh";
    public string ExecuteTimeLocal { get; init; } = "08:00:00";
    public string? OneTimeAtLocal { get; init; }
    public string? StartDateLocal { get; init; }
    public string? EndAtUtc { get; init; }
    public int MaxConsecutiveFailures { get; init; } = 3;
}

public sealed record UpdateNotificationScheduleRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string TemplateKey { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public string? DataJson { get; init; }

    // Recurrence
    public string Type { get; init; } = "EveryXDays";
    public int? IntervalDays { get; init; }
    public string? DayOfWeek { get; init; }
    public int? DayOfMonth { get; init; }
    public string MonthlyOverflowPolicy { get; init; } = "SkipMonth";
    public string MisfirePolicy { get; init; } = "Skip";
    public int? CatchUpLimit { get; init; }

    public string ExecuteTimeLocal { get; init; } = "08:00:00";
    public string? OneTimeAtLocal { get; init; }
    public string? StartDateLocal { get; init; }
    public string? EndAtUtc { get; init; }
    public int MaxConsecutiveFailures { get; init; } = 3;
}

public sealed record ScheduleRecurrenceRequest
{
    public ScheduleType Type { get; init; } = ScheduleType.EveryXDays;
    public int? IntervalDays { get; init; }
    public DayOfWeek? DayOfWeek { get; init; }
    public int? DayOfMonth { get; init; }
    public MonthlyOverflowPolicy MonthlyOverflowPolicy { get; init; } = MonthlyOverflowPolicy.SkipMonth;
    public MisfirePolicy MisfirePolicy { get; init; } = MisfirePolicy.Skip;
    public int? CatchUpLimit { get; init; }
}

public sealed record NotificationScheduleListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Type { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime? NextExecuteAtUtc { get; init; }
    public int FailureCount { get; init; }
    public int ExecutionCount { get; init; }
    public DateTime? LastTriggeredAtUtc { get; init; }
}

public sealed record NotificationScheduleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string TemplateKey { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public string? DataJson { get; init; }
    public ScheduleRecurrenceRequest Recurrence { get; init; } = null!;
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

public sealed record ScheduleExecutionDto
{
    public Guid Id { get; init; }
    public Guid ScheduleId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime OccurrenceTimeUtc { get; init; }
    public int OccurrenceOrdinal { get; init; }
    public DateTime TriggeredAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string? ErrorMessage { get; init; }
}
