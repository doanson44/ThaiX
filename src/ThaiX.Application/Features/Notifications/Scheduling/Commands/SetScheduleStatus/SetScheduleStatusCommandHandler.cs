using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.SetScheduleStatus;

public sealed class SetScheduleStatusCommandHandler
    : IRequestHandler<SetScheduleStatusCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ISchedulingCalculator _calculator;
    private readonly IDateTimeProvider _dateTime;

    public SetScheduleStatusCommandHandler(
        IApplicationDbContext db,
        ISchedulingCalculator calculator,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _calculator = calculator;
        _dateTime = dateTime;
    }

    public async Task<Unit> Handle(SetScheduleStatusCommand cmd, CancellationToken ct)
    {
        var schedule = await _db.NotificationSchedules
            .FirstOrDefaultAsync(s => s.Id == cmd.Id && s.Status != ScheduleStatus.Deleted, ct)
            ?? throw new InvalidOperationException($"Schedule {cmd.Id} not found.");

        switch (cmd.Status)
        {
            case ScheduleStatus.Active:
                var startAtUtc = _calculator.CalculateStartAtUtc(schedule);
                var nextAtUtc = _calculator.CalculateNextExecuteAtUtc(schedule, _dateTime.UtcNow);
                if (nextAtUtc is null)
                    throw new InvalidOperationException("Cannot activate: invalid recurrence.");
                schedule.Activate(startAtUtc, nextAtUtc.Value);
                break;
            case ScheduleStatus.Paused:
                schedule.Pause();
                break;
            case ScheduleStatus.Disabled:
                schedule.Disable();
                break;
            default:
                throw new InvalidOperationException($"Cannot set status to {cmd.Status}.");
        }

        await _db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
