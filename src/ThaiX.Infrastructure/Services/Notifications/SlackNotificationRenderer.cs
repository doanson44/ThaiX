using System.Text.Json;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class SlackNotificationRenderer : INotificationRenderer
{
    private readonly SlackNotificationFormatter _formatter;

    public SlackNotificationRenderer(SlackNotificationFormatter formatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    public NotificationChannel Channel => NotificationChannel.Slack;

    public NotificationRenderedMessage Render(NotificationMessage message, NotificationRoute route, string idempotencyKey)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        var card = message.Card ?? NotificationCardFactory.FromMessage(message);
        var text = _formatter.Format(card, message.Kind);
        var attachment = _formatter.BuildAttachment(card, message.Kind);
        var attachmentsJson = JsonSerializer.Serialize(new[] { attachment });

        var payloadJson = JsonSerializer.Serialize(new
        {
            channel = route.Destination,
            text,
            attachments = new[] { attachment },
            idempotencyKey
        });

        return new NotificationRenderedMessage
        {
            Channel = route.Channel,
            Provider = route.Provider,
            Destination = route.Destination,
            Text = text,
            PayloadJson = payloadJson,
            IdempotencyKey = idempotencyKey,
            AttachmentsJson = attachmentsJson
        };
    }
}