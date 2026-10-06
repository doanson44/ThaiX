using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Notifications.Scheduling.Dtos;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Queries.GetNotificationSchedule;

public sealed class GetNotificationScheduleQueryHandler
    : IRequestHandler<GetNotificationScheduleQuery, NotificationScheduleDto?>
{
    private readonly IApplicationDbContext _db;

    public GetNotificationScheduleQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<NotificationScheduleDto?> Handle(
        GetNotificationScheduleQuery q, CancellationToken ct)
    {
        return await _db.NotificationSchedules
            .Where(x => x.Id == q.Id && x.Status != ScheduleStatus.Deleted)
            .Select(x => new NotificationScheduleDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                TemplateKey = x.TemplateKey,
                Subject = x.Subject,
                Body = x.Body,
                DataJson = x.DataJson,
                Recurrence = x.Recurrence,
                Status = x.Status,
                TimeZoneId = x.TimeZoneId,
                ExecuteTimeLocal = x.ExecuteTimeLocal,
                OneTimeAtLocal = x.OneTimeAtLocal,
                StartDateLocal = x.StartDateLocal,
                StartAtUtc = x.StartAtUtc,
                EndAtUtc = x.EndAtUtc,
                LastTriggeredAtUtc = x.LastTriggeredAtUtc,
                LastSuccessfulAtUtc = x.LastSuccessfulAtUtc,
                NextExecuteAtUtc = x.NextExecuteAtUtc,
                FailureCount = x.FailureCount,
                ExecutionCount = x.ExecutionCount,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt ?? x.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
