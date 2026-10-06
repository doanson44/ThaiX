using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.RunScheduleNow;

public sealed class RunScheduleNowCommandHandler
    : IRequestHandler<RunScheduleNowCommand, ScheduleExecutionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ISchedulingCalculator _calculator;
    private readonly IDateTimeProvider _dateTime;

    public RunScheduleNowCommandHandler(
        IApplicationDbContext db,
        ISchedulingCalculator calculator,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _calculator = calculator;
        _dateTime = dateTime;
    }

    public async Task<ScheduleExecutionDto> Handle(RunScheduleNowCommand cmd, CancellationToken ct)
    {
        var schedule = await _db.NotificationSchedules
            .FirstOrDefaultAsync(s => s.Id == cmd.Id && s.Status != ScheduleStatus.Deleted, ct)
            ?? throw new InvalidOperationException($"Schedule {cmd.Id} not found.");

        var utcNow = _dateTime.UtcNow;
        var execution = NotificationScheduleExecution.Create(
            schedule.Id, utcNow, schedule.ExecutionCount + 1);

        try
        {
            execution.MarkRunning();
            _db.NotificationScheduleExecutions.Add(execution);

            var notification = Notification.Create(
                NotificationKind.SystemAlert,
                NotificationSeverity.Info,
                schedule.TemplateKey,
                schedule.Subject,
                schedule.Body,
                schedule.DataJson,
                null,
                null,
                Guid.NewGuid().ToString("N"),
                null);

            var nextAtUtc = _calculator.CalculateNextExecuteAtUtc(schedule, utcNow);
            schedule.RecordExecution(utcNow, nextAtUtc ?? utcNow.AddYears(100));
            schedule.RecordSuccess(utcNow);

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(ct);

            execution.MarkSucceeded(notification.Id, utcNow);
            await _db.SaveChangesAsync(ct);

            return Map(execution);
        }
        catch (Exception ex)
        {
            execution.MarkFailed("RUN_NOW_FAILED", ex.Message, utcNow);
            _db.NotificationScheduleExecutions.Add(execution);
            schedule.RecordFailure();
            await _db.SaveChangesAsync(ct);
            throw;
        }
    }

    private static ScheduleExecutionDto Map(NotificationScheduleExecution e) => new()
    {
        Id = e.Id,
        ScheduleId = e.ScheduleId,
        Status = e.Status,
        OccurrenceTimeUtc = e.OccurrenceTimeUtc,
        OccurrenceOrdinal = e.OccurrenceOrdinal,
        TriggeredAtUtc = e.TriggeredAtUtc,
        CompletedAtUtc = e.CompletedAtUtc,
        ErrorMessage = e.ErrorMessage
    };
}
