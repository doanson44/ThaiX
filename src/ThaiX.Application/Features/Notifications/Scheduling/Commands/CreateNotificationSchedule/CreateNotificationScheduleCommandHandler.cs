using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;
using ThaiX.Domain.Aggregates.Notifications.ValueObjects;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.CreateNotificationSchedule;

public sealed class CreateNotificationScheduleCommandHandler
    : IRequestHandler<CreateNotificationScheduleCommand, NotificationScheduleDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ISchedulingCalculator _calculator;
    private readonly IDateTimeProvider _dateTime;

    public CreateNotificationScheduleCommandHandler(
        IApplicationDbContext db,
        ISchedulingCalculator calculator,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _calculator = calculator;
        _dateTime = dateTime;
    }

    public async Task<NotificationScheduleDto> Handle(
        CreateNotificationScheduleCommand cmd, CancellationToken ct)
    {
        if (cmd.Type == Domain.Aggregates.Notifications.Enums.ScheduleType.OneTime && cmd.OneTimeAtLocal is not null)
        {
            var hasDuplicateOneTime = await _db.NotificationSchedules
                .AnyAsync(s => s.Status != Domain.Aggregates.Notifications.Enums.ScheduleStatus.Deleted
                               && s.TemplateKey == cmd.TemplateKey
                               && s.TimeZoneId == cmd.TimeZoneId
                               && s.OneTimeAtLocal == cmd.OneTimeAtLocal,
                    ct);

            if (hasDuplicateOneTime)
            {
                throw new InvalidOperationException("A one-time schedule with the same template and occurrence time already exists.");
            }
        }

        var recurrence = BuildRecurrence(cmd);

        var schedule = Domain.Aggregates.Notifications.NotificationSchedule.Create(
            cmd.Name,
            cmd.TemplateKey,
            cmd.Subject,
            cmd.Body,
            recurrence,
            cmd.TimeZoneId,
            cmd.ExecuteTimeLocal,
            cmd.OneTimeAtLocal,
            cmd.StartDateLocal,
            cmd.MaxConsecutiveFailures);

        var startAtUtc = _calculator.CalculateStartAtUtc(schedule);
        var nextAtUtc = _calculator.CalculateNextExecuteAtUtc(schedule, _dateTime.UtcNow);

        if (nextAtUtc is null)
            throw new InvalidOperationException("Failed to calculate first execution time.");

        schedule.Activate(startAtUtc, nextAtUtc.Value);

        _db.NotificationSchedules.Add(schedule);
        await _db.SaveChangesAsync(ct);

        return Map(schedule);
    }

    private static ScheduleRecurrence BuildRecurrence(CreateNotificationScheduleCommand cmd) => cmd.Type switch
    {
        Domain.Aggregates.Notifications.Enums.ScheduleType.OneTime => ScheduleRecurrence.OneTime(cmd.MisfirePolicy),
        Domain.Aggregates.Notifications.Enums.ScheduleType.Weekly => ScheduleRecurrence.Weekly(cmd.DayOfWeek ?? DayOfWeek.Monday, cmd.MisfirePolicy),
        Domain.Aggregates.Notifications.Enums.ScheduleType.Monthly => ScheduleRecurrence.Monthly(cmd.DayOfMonth ?? 1, cmd.MonthlyOverflowPolicy, cmd.MisfirePolicy),
        _ => ScheduleRecurrence.EveryXDays(cmd.IntervalDays ?? 1, cmd.MisfirePolicy)
    };

    private static NotificationScheduleDto Map(Domain.Aggregates.Notifications.NotificationSchedule s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        TemplateKey = s.TemplateKey,
        Subject = s.Subject,
        Body = s.Body,
        Recurrence = s.Recurrence,
        Status = s.Status,
        TimeZoneId = s.TimeZoneId,
        ExecuteTimeLocal = s.ExecuteTimeLocal,
        OneTimeAtLocal = s.OneTimeAtLocal,
        StartDateLocal = s.StartDateLocal,
        StartAtUtc = s.StartAtUtc,
        NextExecuteAtUtc = s.NextExecuteAtUtc,
        FailureCount = s.FailureCount,
        ExecutionCount = s.ExecutionCount,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt ?? s.CreatedAt
    };
}
