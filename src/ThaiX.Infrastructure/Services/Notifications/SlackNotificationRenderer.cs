using System.Text;
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

        var text = BuildText(message);
        var attachment = message.Card is not null ? _formatter.BuildAttachment(message.Card) : null;
        var attachmentsJson = attachment is null ? null : JsonSerializer.Serialize(new[] { attachment });

        var payloadJson = JsonSerializer.Serialize(new
        {
            channel = route.Destination,
            text,
            attachments = attachment is null ? null : new[] { attachment },
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

    private string BuildText(NotificationMessage message)
    {
        if (message.Card is not null)
        {
            return _formatter.Format(message.Card);
        }

        // Legacy path: build from plain text fields
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(message.Title))
        {
            builder.Append(message.Title.Trim()).Append('\n');
        }

        builder.Append($"Severity: {message.Severity}\n");

        if (builder.Length > 0)
        {
            builder.Append('\n');
        }

        builder.Append(message.Text.Trim()).Append('\n');

        foreach (var section in message.Sections)
        {
            builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(section.Title))
            {
                builder.Append(section.Title.Trim()).Append('\n');
            }
            builder.Append(section.Text.Trim()).Append('\n');
        }

        foreach (var action in message.Actions)
        {
            builder.Append('\n');
            builder.Append($"{action.Label.Trim()}: {action.Url.Trim()}\n");
        }

        return builder.ToString().Trim();
    }
}
