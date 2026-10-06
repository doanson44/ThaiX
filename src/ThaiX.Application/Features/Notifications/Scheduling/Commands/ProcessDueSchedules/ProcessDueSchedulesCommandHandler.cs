using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.ProcessDueSchedules;

public sealed class ProcessDueSchedulesCommandHandler
    : IRequestHandler<ProcessDueSchedulesCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ISchedulingCalculator _calculator;
    private readonly IDateTimeProvider _dateTime;

    public ProcessDueSchedulesCommandHandler(
        IApplicationDbContext db,
        ISchedulingCalculator calculator,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _calculator = calculator;
        _dateTime = dateTime;
    }

    public async Task<int> Handle(ProcessDueSchedulesCommand cmd, CancellationToken ct)
    {
        var utcNow = _dateTime.UtcNow;
        var dueSchedules = await _db.NotificationSchedules
            .Where(s => s.Status == ScheduleStatus.Active
                        && s.NextExecuteAtUtc <= utcNow
                        && (s.EndAtUtc == null || s.EndAtUtc > utcNow))
            .OrderBy(s => s.NextExecuteAtUtc)
            .Take(50)
            .ToListAsync(ct);

        var processed = 0;
        foreach (var schedule in dueSchedules)
        {
            ct.ThrowIfCancellationRequested();

            var occurrenceTimeUtc = schedule.NextExecuteAtUtc ?? utcNow;
            var occKey = $"{schedule.Id}:{occurrenceTimeUtc:O}";
            var execution = NotificationScheduleExecution.Create(
                schedule.Id, occurrenceTimeUtc, schedule.ExecutionCount + 1);
            execution.MarkRunning();
            _db.NotificationScheduleExecutions.Add(execution);
            Notification? notification = null;

            try
            {
                notification = await _db.Notifications
                    .FirstOrDefaultAsync(x => x.DeduplicationKey == occKey, ct);

                if (notification is null)
                {
                    notification = Notification.Create(
                        NotificationKind.SystemAlert,
                        NotificationSeverity.Info,
                        schedule.TemplateKey,
                        schedule.Subject,
                        schedule.Body,
                        schedule.DataJson,
                        null,
                        null,
                        occKey,
                        null);

                    _db.Notifications.Add(notification);
                    await _db.SaveChangesAsync(ct);
                }

                var nextAtUtc = _calculator.CalculateNextExecuteAtUtc(schedule, utcNow);
                schedule.RecordExecution(utcNow, nextAtUtc ?? utcNow.AddYears(100));
                schedule.RecordSuccess(utcNow);

                if (schedule.Recurrence.Type == ScheduleType.OneTime)
                {
                    schedule.Complete();
                }

                execution.MarkSucceeded(notification.Id, utcNow);
                processed++;
            }
            catch (DbUpdateException ex) when (IsNotificationDeduplicationConflict(ex))
            {
                if (notification is not null)
                {
                    _db.Notifications.Remove(notification);
                }

                var existingNotification = await _db.Notifications
                    .FirstOrDefaultAsync(x => x.DeduplicationKey == occKey, ct);

                var nextAtUtc = _calculator.CalculateNextExecuteAtUtc(schedule, utcNow);
                schedule.RecordExecution(utcNow, nextAtUtc ?? utcNow.AddYears(100));
                schedule.RecordSuccess(utcNow);

                if (schedule.Recurrence.Type == ScheduleType.OneTime)
                {
                    schedule.Complete();
                }

                if (existingNotification is not null)
                {
                    execution.MarkSucceeded(existingNotification.Id, utcNow);
                    processed++;
                }
                else
                {
                    execution.MarkSkipped("Notification already exists for this occurrence.", utcNow);
                }
            }
            catch (Exception ex)
            {
                schedule.RecordFailure();
                execution.MarkFailed("PROCESS_FAILED", ex.Message, utcNow);
            }

            await _db.SaveChangesAsync(ct);
        }

        return processed;
    }

    private static bool IsNotificationDeduplicationConflict(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("Cannot insert duplicate key row", StringComparison.OrdinalIgnoreCase)
               && message.Contains("IX_Notifications_DeduplicationKey", StringComparison.Ordinal);
    }
}
