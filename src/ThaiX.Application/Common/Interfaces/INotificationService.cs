using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Common.Interfaces;

public interface INotificationService
{
    Task<Guid> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken);
}
