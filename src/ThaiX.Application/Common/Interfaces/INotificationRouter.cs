using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Common.Interfaces;

public interface INotificationRouter
{
    Task<Guid> DispatchAsync(NotificationMessage message, CancellationToken cancellationToken);
}
