using ThaiX.Application.Common.Models;

namespace ThaiX.Infrastructure.Services.Notifications;

public interface ISlackNotificationService
{
    Task SendAsync(NotificationMessage message, CancellationToken ct);

    Task SendToChannelAsync(string channelId, string text, string? threadTs, CancellationToken ct, string? attachmentsJson = null);

    Task SendToResponseUrlAsync(string responseUrl, string text, CancellationToken ct);
}
