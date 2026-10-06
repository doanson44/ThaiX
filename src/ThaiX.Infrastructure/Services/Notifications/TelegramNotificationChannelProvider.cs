using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class TelegramNotificationChannelProvider : INotificationChannelProvider
{
    private readonly ITelegramNotificationService _telegramNotificationService;

    public TelegramNotificationChannelProvider(ITelegramNotificationService telegramNotificationService)
    {
        _telegramNotificationService = telegramNotificationService ?? throw new ArgumentNullException(nameof(telegramNotificationService));
    }

    public NotificationChannel Channel => NotificationChannel.Telegram;

    public string Provider => "Telegram";

    public async Task<NotificationSendResult> SendAsync(
        NotificationRenderedMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await _telegramNotificationService.SendRenderedToChatAsync(
                message.Destination,
                message.Text,
                cancellationToken);

            return NotificationSendResult.Sent(providerMessageId: null);
        }
        catch (Exception ex)
        {
            return NotificationSendResult.Failed(ex.GetType().Name, ex.Message);
        }
    }
}
