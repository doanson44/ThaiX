using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Configuration;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling;

public sealed class SchedulingCalculator : ISchedulingCalculator
{
    private readonly TimeZoneInfo _defaultTimeZone;

    public SchedulingCalculator(IOptions<NotificationSchedulingSettings> settings)
    {
        _defaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.Value.TimeZoneId);
    }

    public DateTime CalculateStartAtUtc(NotificationSchedule schedule)
    {
        var tz = GetTimeZone(schedule.TimeZoneId);
        var nowInTz = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var startDate = schedule.Recurrence.Type == ScheduleType.OneTime
            ? schedule.OneTimeAtLocal ?? nowInTz
            : schedule.StartDateLocal ?? nowInTz;

        var executeDateTime = startDate.Date.Add(schedule.ExecuteTimeLocal.ToTimeSpan());
        return TimeZoneInfo.ConvertTimeToUtc(executeDateTime, tz);
    }

    public DateTime? CalculateNextExecuteAtUtc(NotificationSchedule schedule, DateTime utcNow)
    {
        return schedule.Recurrence.Type switch
        {
            ScheduleType.OneTime => ComputeOneTime(schedule),
            ScheduleType.EveryXDays => ComputeEveryXDays(schedule, utcNow),
            ScheduleType.Weekly => ComputeWeekly(schedule, utcNow),
            ScheduleType.Monthly => ComputeMonthly(schedule, utcNow),
            _ => null
        };
    }

    public IReadOnlyList<DateTime> PreviewOccurrences(
        NotificationSchedule schedule, int count, DateTime utcNow)
    {
        var result = new List<DateTime>(count);
        var next = CalculateNextExecuteAtUtc(schedule, utcNow);

        for (var i = 0; i < count && next.HasValue; i++)
        {
            result.Add(next.Value);

            // Simulate next run by temporarily advancing
            var tempSchedule = CloneForPreview(schedule, next.Value);
            next = CalculateNextExecuteAtUtc(tempSchedule, next.Value.AddSeconds(1));
        }

        return result;
    }

    private DateTime? ComputeOneTime(NotificationSchedule schedule)
    {
        if (schedule.OneTimeAtLocal is null) return null;
        if (schedule.NextExecuteAtUtc.HasValue) return null;

        var tz = GetTimeZone(schedule.TimeZoneId);
        var oneTimeLocal = schedule.OneTimeAtLocal.Value.Date.Add(schedule.ExecuteTimeLocal.ToTimeSpan());
        return SafeConvert(oneTimeLocal, tz);
    }

    private DateTime? ComputeEveryXDays(NotificationSchedule schedule, DateTime utcNow)
    {
        if (schedule.Recurrence.IntervalDays is not { } interval || interval <= 0)
            return null;

        var tz = GetTimeZone(schedule.TimeZoneId);
        var nowInTz = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
        var today = nowInTz.Date;

        // Build target time for today
        var target = today.Add(schedule.ExecuteTimeLocal.ToTimeSpan());
        if (target <= nowInTz)
        {
            target = target.AddDays(interval);
        }

        // Align from start date if set
        if (schedule.StartDateLocal is { } startDate)
        {
            var startLocal = startDate.Date.Add(schedule.ExecuteTimeLocal.ToTimeSpan());
            if (target < startLocal)
            {
                target = startLocal;
            }
            else if (target > startLocal)
            {
                var daysSinceStart = (target - startLocal).Days;
                var remainder = daysSinceStart % interval;
                if (remainder != 0)
                {
                    target = target.AddDays(interval - remainder);
                }
            }
        }

        return SafeConvert(target, tz);
    }

    private DateTime? ComputeWeekly(NotificationSchedule schedule, DateTime utcNow)
    {
        if (schedule.Recurrence.DayOfWeek is not { } targetDow)
            return null;

        var tz = GetTimeZone(schedule.TimeZoneId);
        var nowInTz = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
        var today = nowInTz.Date;

        var daysUntil = ((int)targetDow - (int)today.DayOfWeek + 7) % 7;
        var target = today.AddDays(daysUntil).Add(schedule.ExecuteTimeLocal.ToTimeSpan());

        if (target <= nowInTz)
        {
            target = target.AddDays(7);
        }

        return SafeConvert(target, tz);
    }

    private DateTime? ComputeMonthly(NotificationSchedule schedule, DateTime utcNow)
    {
        if (schedule.Recurrence.DayOfMonth is not { } targetDom)
            return null;

        var tz = GetTimeZone(schedule.TimeZoneId);
        var nowInTz = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
        var year = nowInTz.Year;
        var month = nowInTz.Month;

        var target = BuildMonthlyTarget(year, month, targetDom, schedule.Recurrence.MonthlyOverflowPolicy, schedule.ExecuteTimeLocal);
        if (target is null || target <= nowInTz)
        {
            // Next month
            month++;
            if (month > 12)
            {
                month = 1;
                year++;
            }
            target = BuildMonthlyTarget(year, month, targetDom, schedule.Recurrence.MonthlyOverflowPolicy, schedule.ExecuteTimeLocal);
        }

        return target is null ? null : SafeConvert(target.Value, tz);
    }

    private static DateTime? BuildMonthlyTarget(
        int year, int month, int dayOfMonth,
        MonthlyOverflowPolicy overflowPolicy,
        TimeOnly executeTime)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        if (dayOfMonth > daysInMonth)
        {
            return overflowPolicy switch
            {
                MonthlyOverflowPolicy.SkipMonth => null,
                MonthlyOverflowPolicy.RunOnLastDay => new DateTime(year, month, daysInMonth).Add(executeTime.ToTimeSpan()),
                _ => null
            };
        }

        return new DateTime(year, month, dayOfMonth).Add(executeTime.ToTimeSpan());
    }

    private DateTime? SafeConvert(DateTime localDateTime, TimeZoneInfo tz)
    {
        if (tz.IsInvalidTime(localDateTime))
        {
            // DST spring-forward gap: push to the next valid time
            localDateTime = localDateTime.AddHours(1);
        }

        if (tz.IsAmbiguousTime(localDateTime))
        {
            // DST fall-back: use the later occurrence (standard time)
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, tz);
        }

        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, tz);
    }

    private TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return _defaultTimeZone;
        }
    }

    private static NotificationSchedule CloneForPreview(NotificationSchedule schedule, DateTime lastExecutedUtc)
    {
        // Just used for preview - we only need the next calculation
        return schedule;
    }
}
