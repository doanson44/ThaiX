using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Common.Interfaces;

public interface INotificationRouteResolver
{
    Task<IReadOnlyList<NotificationRoute>> ResolveAsync(
        NotificationRouteRequest request,
        CancellationToken cancellationToken);
}
