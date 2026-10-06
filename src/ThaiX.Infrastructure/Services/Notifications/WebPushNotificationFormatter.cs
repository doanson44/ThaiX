using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class WebPushNotificationFormatter : INotificationFormatter
{
    public NotificationChannel Channel => NotificationChannel.WebPush;

    public string Format(NotificationCard card)
    {
        // Web push: title only + short body = first section text
        var body = card.Sections.Count > 0
            ? card.Sections[0].Text.Trim()
            : card.Metrics.Count > 0
                ? $"{card.Metrics[0].Label}: {card.Metrics[0].Value}"
                : string.Empty;

        return body;
    }
}
