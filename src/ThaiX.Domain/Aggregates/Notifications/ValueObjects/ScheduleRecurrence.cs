using System.Text.Json.Serialization;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Domain.Aggregates.Notifications.ValueObjects;

public sealed class ScheduleRecurrence
{
    [JsonConstructor]
    private ScheduleRecurrence() { }

    public ScheduleType Type { get; init; }
    public int? IntervalDays { get; init; }
    public DayOfWeek? DayOfWeek { get; init; }
    public int? DayOfMonth { get; init; }
    public MonthlyOverflowPolicy MonthlyOverflowPolicy { get; init; } = MonthlyOverflowPolicy.SkipMonth;
    public MisfirePolicy MisfirePolicy { get; init; }
    public int? CatchUpLimit { get; init; }

    public static ScheduleRecurrence OneTime(MisfirePolicy misfire = MisfirePolicy.Skip)
        => new()
        {
            Type = ScheduleType.OneTime,
            MisfirePolicy = misfire
        };

    public static ScheduleRecurrence EveryXDays(int intervalDays, MisfirePolicy misfire = MisfirePolicy.Skip)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(intervalDays, 0);
        return new()
        {
            Type = ScheduleType.EveryXDays,
            IntervalDays = intervalDays,
            MisfirePolicy = misfire
        };
    }

    public static ScheduleRecurrence Weekly(DayOfWeek dayOfWeek, MisfirePolicy misfire = MisfirePolicy.Skip)
        => new()
        {
            Type = ScheduleType.Weekly,
            DayOfWeek = dayOfWeek,
            MisfirePolicy = misfire
        };

    public static ScheduleRecurrence Monthly(
        int dayOfMonth,
        MonthlyOverflowPolicy overflowPolicy = MonthlyOverflowPolicy.SkipMonth,
        MisfirePolicy misfire = MisfirePolicy.Skip)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dayOfMonth, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dayOfMonth, 31);
        return new()
        {
            Type = ScheduleType.Monthly,
            DayOfMonth = dayOfMonth,
            MonthlyOverflowPolicy = overflowPolicy,
            MisfirePolicy = misfire
        };
    }
}
