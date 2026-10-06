using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Notifications.Enums;
using ThaiX.Domain.Aggregates.Notifications.ValueObjects;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.UpdateNotificationSchedule;

public sealed class UpdateNotificationScheduleCommandHandler
    : IRequestHandler<UpdateNotificationScheduleCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ISchedulingCalculator _calculator;
    private readonly IDateTimeProvider _dateTime;

    public UpdateNotificationScheduleCommandHandler(
        IApplicationDbContext db,
        ISchedulingCalculator calculator,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _calculator = calculator;
        _dateTime = dateTime;
    }

    public async Task<Unit> Handle(UpdateNotificationScheduleCommand cmd, CancellationToken ct)
    {
        if (cmd.Type == ScheduleType.OneTime && cmd.OneTimeAtLocal is not null)
        {
            var hasDuplicateOneTime = await _db.NotificationSchedules
                .AnyAsync(s => s.Id != cmd.Id
                               && s.Status != ScheduleStatus.Deleted
                               && s.TemplateKey == cmd.TemplateKey
                               && s.OneTimeAtLocal == cmd.OneTimeAtLocal,
                    ct);

            if (hasDuplicateOneTime)
            {
                throw new InvalidOperationException("A one-time schedule with the same template and occurrence time already exists.");
            }
        }

        var schedule = await _db.NotificationSchedules
            .FirstOrDefaultAsync(s => s.Id == cmd.Id && s.Status != ScheduleStatus.Deleted, ct)
            ?? throw new InvalidOperationException($"Schedule {cmd.Id} not found.");

        var recurrence = BuildRecurrence(cmd);

        schedule.Update(
            cmd.Name, cmd.Description, cmd.TemplateKey, cmd.Subject, cmd.Body,
            cmd.DataJson, recurrence, cmd.ExecuteTimeLocal,
            cmd.OneTimeAtLocal, cmd.StartDateLocal, cmd.EndAtUtc,
            cmd.MaxConsecutiveFailures);

        var nextAtUtc = _calculator.CalculateNextExecuteAtUtc(schedule, _dateTime.UtcNow);
        if (schedule.Status == ScheduleStatus.Active && nextAtUtc is not null)
        {
            var startAtUtc = _calculator.CalculateStartAtUtc(schedule);
            schedule.Activate(startAtUtc, nextAtUtc.Value);
        }

        await _db.SaveChangesAsync(ct);

        return Unit.Value;
    }

    private static ScheduleRecurrence BuildRecurrence(UpdateNotificationScheduleCommand cmd) => cmd.Type switch
    {
        ScheduleType.OneTime => ScheduleRecurrence.OneTime(cmd.MisfirePolicy),
        ScheduleType.Weekly => ScheduleRecurrence.Weekly(cmd.DayOfWeek ?? DayOfWeek.Monday, cmd.MisfirePolicy),
        ScheduleType.Monthly => ScheduleRecurrence.Monthly(cmd.DayOfMonth ?? 1, cmd.MonthlyOverflowPolicy, cmd.MisfirePolicy),
        _ => ScheduleRecurrence.EveryXDays(cmd.IntervalDays ?? 1, cmd.MisfirePolicy)
    };
}
