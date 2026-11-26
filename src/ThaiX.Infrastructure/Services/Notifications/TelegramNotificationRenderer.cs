using System.Text.Json;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class TelegramNotificationRenderer : INotificationRenderer
{
    private readonly TelegramNotificationFormatter _formatter;

    public TelegramNotificationRenderer(TelegramNotificationFormatter formatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    public NotificationChannel Channel => NotificationChannel.Telegram;

    public NotificationRenderedMessage Render(NotificationMessage message, NotificationRoute route, string idempotencyKey)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        var card = message.Card ?? NotificationCardFactory.FromMessage(message);
        var text = _formatter.Format(card, message.Kind);
        var payloadJson = JsonSerializer.Serialize(new
        {
            chat_id = route.Destination,
            text,
            parse_mode = "HTML",
            idempotencyKey
        });

        return new NotificationRenderedMessage
        {
            Channel = route.Channel,
            Provider = route.Provider,
            Destination = route.Destination,
            Text = text,
            PayloadJson = payloadJson,
            IdempotencyKey = idempotencyKey
        };
    }
}
