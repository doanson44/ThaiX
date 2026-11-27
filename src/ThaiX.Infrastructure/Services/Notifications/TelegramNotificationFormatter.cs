using System.Globalization;
using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class TelegramNotificationFormatter : INotificationFormatter
{
    public NotificationChannel Channel => NotificationChannel.Telegram;

    public string Format(NotificationCard card) => Format(card, null);

    public string Format(NotificationCard card, NotificationKind? kind)
    {
        ArgumentNullException.ThrowIfNull(card);

        var builder = new StringBuilder();
        var category = kind.HasValue ? NotificationTemplateResources.KindLabel(kind.Value) : card.Category;
        var severityLabel = NotificationTemplateResources.SeverityLabel(card.Severity);
        var severityIcon = NotificationTemplateResources.SeverityIcon(card.Severity);

        builder.Append($"<b>{EscapeHtml(NotificationTemplateResources.Get("BrandFooter"))}</b>\n");
        builder.Append($"{severityIcon} <b>{EscapeHtml(category)}</b> | <b>{EscapeHtml(severityLabel)}</b>\n");
        builder.Append($"<b>{EscapeHtml(card.Title)}</b>\n\n");

        if (card.Metrics.Count > 0)
        {
            builder.Append($"<b>{EscapeHtml(NotificationTemplateResources.CardTypeLabel(card.Type))}</b>\n");
            foreach (var metric in card.Metrics)
            {
                builder.Append(
                    $"<b>{EscapeHtml(metric.Label.Trim())}</b>: <code>{EscapeHtml(FormatMetricValue(metric.Value))}</code>\n");
            }

            builder.Append('\n');
        }

        foreach (var section in card.Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Title))
            {
                builder.Append($"<b>{EscapeHtml(section.Title.Trim())}</b>\n");
            }

            if (!string.IsNullOrWhiteSpace(section.Text))
            {
                builder.Append($"{EscapeHtml(section.Text.Trim())}\n");
            }

            builder.Append('\n');
        }

        var safeActions = card.Actions.Where(action => IsSafeLink(action.Url)).ToArray();
        foreach (var action in safeActions)
        {
            builder.Append(
                $"<a href=\"{EscapeHtml(action.Url.Trim())}\">{EscapeHtml(action.Label.Trim())}</a>\n");
        }

        return builder.ToString().Trim();
    }

    private static string FormatMetricValue(string value)
    {
        if (value.EndsWith('%'))
        {
            var numeric = value.AsSpan()[..^1];
            if (decimal.TryParse(numeric, NumberStyles.Any, CultureInfo.InvariantCulture, out var pct))
            {
                return pct.ToString("+0.##;-0.##", CultureInfo.InvariantCulture) + "%";
            }
        }

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
        {
            return num.ToString("#,##0.######", CultureInfo.InvariantCulture);
        }

        return value;
    }

    private static bool IsSafeLink(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}