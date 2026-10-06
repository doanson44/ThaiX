using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Notifications.Enums;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.DeleteNotificationSchedule;

public sealed class DeleteNotificationScheduleCommandHandler
    : IRequestHandler<DeleteNotificationScheduleCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public DeleteNotificationScheduleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteNotificationScheduleCommand cmd, CancellationToken ct)
    {
        var schedule = await _db.NotificationSchedules
            .FirstOrDefaultAsync(s => s.Id == cmd.Id && s.Status != ScheduleStatus.Deleted, ct)
            ?? throw new InvalidOperationException($"Schedule {cmd.Id} not found.");

        schedule.Delete();
        await _db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
