using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.CreateNotificationSchedule;

[InvalidateCache(CacheGroups.NotificationSchedules)]
public sealed record CreateNotificationScheduleCommand : IAppCommand<NotificationScheduleDto>
{
    public string Name { get; init; } = null!;
    public string TemplateKey { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public string? DataJson { get; init; }

    // Recurrence
    public ScheduleType Type { get; init; } = ScheduleType.EveryXDays;
    public int? IntervalDays { get; init; }
    public DayOfWeek? DayOfWeek { get; init; }
    public int? DayOfMonth { get; init; }
    public MonthlyOverflowPolicy MonthlyOverflowPolicy { get; init; } = MonthlyOverflowPolicy.SkipMonth;
    public MisfirePolicy MisfirePolicy { get; init; } = MisfirePolicy.Skip;
    public int? CatchUpLimit { get; init; }

    public string TimeZoneId { get; init; } = "Asia/Ho_Chi_Minh";
    public TimeOnly ExecuteTimeLocal { get; init; }
    public DateTime? OneTimeAtLocal { get; init; }
    public DateTime? StartDateLocal { get; init; }
    public DateTime? EndAtUtc { get; init; }
    public int MaxConsecutiveFailures { get; init; } = 3;
}
