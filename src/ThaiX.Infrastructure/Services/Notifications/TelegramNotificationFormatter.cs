using System.Globalization;
using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class TelegramNotificationFormatter : INotificationFormatter
{
    public NotificationChannel Channel => NotificationChannel.Telegram;

    public string Format(NotificationCard card)
    {
        var builder = new StringBuilder();

        // Header: severity icon + TITLE, category as subtitle
        builder.Append($"{NotificationBrand.SeverityIcon(card.Severity)} <b>{EscapeHtml(card.Title)}</b>\n");
        builder.Append($"<i>{EscapeHtml(card.Category)}</i>\n\n");

        // Metrics — values in monospace to align with the app's numeric convention
        if (card.Metrics.Count > 0)
        {
            var maxLabelLen = card.Metrics.Max(m => m.Label.Length);
            foreach (var metric in card.Metrics)
            {
                builder.Append(
                    $"{EscapeHtml(metric.Label.PadRight(maxLabelLen))}  <code>{EscapeHtml(FormatMetricValue(metric.Value))}</code>\n");
            }

            builder.Append('\n');
        }

        // Sections
        foreach (var section in card.Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Title))
            {
                builder.Append($"<b>{EscapeHtml(section.Title.Trim())}</b>\n");
            }

            builder.Append($"{EscapeHtml(section.Text.Trim())}\n\n");
        }

        // Actions — real hyperlinks instead of raw "Label: Url" text
        foreach (var action in card.Actions)
        {
            builder.Append($"🔗 <a href=\"{EscapeHtml(action.Url.Trim())}\">{EscapeHtml(action.Label.Trim())}</a>\n");
        }

        builder.Append("\n<i>— ThaiX</i>");

        return builder.ToString().Trim();
    }

    private static string FormatMetricValue(string value)
    {
        // Percentage: e.g. "-0.0022%" → "-0.00%"
        if (value.EndsWith('%'))
        {
            var numeric = value.AsSpan()[..^1];
            if (decimal.TryParse(numeric, NumberStyles.Any, CultureInfo.InvariantCulture, out var pct))
            {
                return pct.ToString("+0.##;-0.##", CultureInfo.InvariantCulture) + "%";
            }
        }

        // Numeric: apply thousands separator, trim insignificant decimals
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
        {
            return num.ToString("#,##0.######", CultureInfo.InvariantCulture);
        }

        return value;
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}
