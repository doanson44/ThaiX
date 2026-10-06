using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Features.Notifications.Scheduling;

public interface ISchedulingCalculator
{
    /// <summary>
    /// Calculates the UTC time of the next execution for a given schedule,
    /// based on the configured timezone and recurrence rules.
    /// </summary>
    DateTime? CalculateNextExecuteAtUtc(NotificationSchedule schedule, DateTime utcNow);

    /// <summary>
    /// Calculates the start time of a schedule in UTC,
    /// using its configured execute time and timezone.
    /// </summary>
    DateTime CalculateStartAtUtc(NotificationSchedule schedule);

    /// <summary>
    /// Preview the next N occurrences starting from now.
    /// </summary>
    IReadOnlyList<DateTime> PreviewOccurrences(
        NotificationSchedule schedule,
        int count,
        DateTime utcNow);
}
