using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Notifications.Queries.GetUserNotificationPreferences;

public sealed class GetUserNotificationPreferencesQueryHandler
    : IRequestHandler<GetUserNotificationPreferencesQuery, IReadOnlyList<UserNotificationPreferenceDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserNotificationPreferencesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<UserNotificationPreferenceDto>> Handle(
        GetUserNotificationPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserNotificationPreferences
            .AsNoTracking()
            .Where(preference => preference.UserId == request.UserId)
            .OrderBy(preference => preference.Kind)
            .ThenBy(preference => preference.Channel)
            .Select(preference => new UserNotificationPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                Kind = preference.Kind,
                Channel = preference.Channel,
                Enabled = preference.Enabled,
                Destination = preference.Destination,
                MinimumSeverity = preference.MinimumSeverity,
                QuietHoursStart = preference.QuietHoursStart,
                QuietHoursEnd = preference.QuietHoursEnd,
                TimeZoneId = preference.TimeZoneId,
                BatchingMode = preference.BatchingMode
            })
            .ToListAsync(cancellationToken);
    }
}
