using ThaiX.Application.Common.Models;

namespace ThaiX.Infrastructure.Services.Notifications;

public interface ITelegramNotificationService
{
    Task SendAsync(NotificationMessage message, CancellationToken ct);

    Task SendToChatAsync(string chatId, string text, CancellationToken ct);

    Task SendRenderedToChatAsync(string chatId, string htmlText, CancellationToken ct);
}
