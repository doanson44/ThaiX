using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Features.Notifications.Commands.UpsertUserNotificationPreference;

public sealed class UpsertUserNotificationPreferenceCommandHandler
    : IRequestHandler<UpsertUserNotificationPreferenceCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public UpsertUserNotificationPreferenceCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Guid> Handle(
        UpsertUserNotificationPreferenceCommand request,
        CancellationToken cancellationToken)
    {
        var preference = await _dbContext.UserNotificationPreferences
            .FirstOrDefaultAsync(
                x => x.UserId == request.UserId
                     && x.Kind == request.Kind
                     && x.Channel == request.Channel,
                cancellationToken);

        if (preference is null)
        {
            preference = UserNotificationPreference.Create(
                request.UserId,
                request.Kind,
                request.Channel,
                request.Destination,
                request.MinimumSeverity,
                request.TimeZoneId,
                request.BatchingMode);

            preference.Update(
                request.Enabled,
                request.Destination,
                request.MinimumSeverity,
                request.QuietHoursStart,
                request.QuietHoursEnd,
                request.TimeZoneId,
                request.BatchingMode);

            _dbContext.UserNotificationPreferences.Add(preference);
        }
        else
        {
            preference.Update(
                request.Enabled,
                request.Destination,
                request.MinimumSeverity,
                request.QuietHoursStart,
                request.QuietHoursEnd,
                request.TimeZoneId,
                request.BatchingMode);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return preference.Id;
    }
}
