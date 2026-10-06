using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Notifications.Scheduling.Commands.ProcessDueSchedules;

[InvalidateCache(CacheGroups.NotificationSchedules)]
public sealed record ProcessDueSchedulesCommand : IAppCommand<int>
{
}
