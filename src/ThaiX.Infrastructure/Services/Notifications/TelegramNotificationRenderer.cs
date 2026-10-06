using System.Text;
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

        var text = BuildText(message);
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
            builder.Append($"<b>{EscapeHtml(message.Title.Trim())}</b>\n");
        }

        builder.Append($"Severity: {EscapeHtml(message.Severity.ToString())}\n");

        if (builder.Length > 0)
        {
            builder.Append('\n');
        }

        builder.Append($"{EscapeHtml(message.Text.Trim())}\n");

        foreach (var section in message.Sections)
        {
            builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(section.Title))
            {
                builder.Append($"<b>{EscapeHtml(section.Title.Trim())}</b>\n");
            }
            builder.Append($"{EscapeHtml(section.Text.Trim())}\n");
        }

        foreach (var action in message.Actions)
        {
            builder.Append('\n');
            builder.Append($"{EscapeHtml(action.Label.Trim())}: {EscapeHtml(action.Url.Trim())}\n");
        }

        return builder.ToString().Trim();
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
