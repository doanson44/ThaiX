using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class DiscordNotificationFormatter : INotificationFormatter
{
    public NotificationChannel Channel => NotificationChannel.Discord;

    public string Format(NotificationCard card)
    {
        // Discord supports Markdown — use same format as Slack for now.
        var fmt = new SlackNotificationFormatter();
        return fmt.Format(card);
    }
}
