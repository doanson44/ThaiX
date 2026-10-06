namespace ThaiX.Domain.Aggregates.Notifications.Enums;

public enum ScheduleExecutionStatus
{
    Claimed = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4,
    Skipped = 5,
    Misfired = 6
}
