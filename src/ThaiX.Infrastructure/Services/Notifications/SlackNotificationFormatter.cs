using System.Globalization;
using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class SlackNotificationFormatter : INotificationFormatter
{
    private const int MaxFieldsPerBlock = 10;

    public NotificationChannel Channel => NotificationChannel.Slack;

    /// <summary>
    /// Plain mrkdwn fallback — shown in push-notification previews and by clients
    /// that don't render Block Kit. The real presentation is <see cref="BuildAttachment"/>.
    /// </summary>
    public string Format(NotificationCard card)
    {
        var builder = new StringBuilder();

        builder.Append($"{NotificationBrand.SeverityIcon(card.Severity)} *{card.Title}*  |  _{card.Category}_\n\n");

        if (card.Metrics.Count > 0)
        {
            var maxLabelLen = card.Metrics.Max(m => m.Label.Length);
            foreach (var metric in card.Metrics)
            {
                builder.Append($"{metric.Label.PadRight(maxLabelLen)}  `{FormatMetricValue(metric.Value)}`\n");
            }

            builder.Append('\n');
        }

        foreach (var section in card.Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Title))
            {
                builder.Append($"*{section.Title.Trim()}*\n");
            }

            builder.Append($"{section.Text.Trim()}\n\n");
        }

        foreach (var action in card.Actions)
        {
            builder.Append($"<{action.Url.Trim()}|{action.Label.Trim()}>\n");
        }

        return builder.ToString().Trim();
    }

    /// <summary>
    /// Builds a Block Kit "attachment" (header + fields + sections + link buttons) wrapped
    /// in a severity-colored side bar — the only way Slack still supports a brand color strip.
    /// </summary>
    public object BuildAttachment(NotificationCard card)
    {
        var blocks = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["type"] = "header",
                ["text"] = new Dictionary<string, object?>
                {
                    ["type"] = "plain_text",
                    ["text"] = Truncate(card.Title, 150),
                    ["emoji"] = true
                }
            },
            new Dictionary<string, object?>
            {
                ["type"] = "context",
                ["elements"] = new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["type"] = "mrkdwn",
                        ["text"] = $"*{card.Category}*  ·  {NotificationBrand.SeverityIcon(card.Severity)} {card.Severity}"
                    }
                }
            }
        };

        if (card.Metrics.Count > 0)
        {
            blocks.Add(new Dictionary<string, object?> { ["type"] = "divider" });

            foreach (var chunk in Chunk(card.Metrics, MaxFieldsPerBlock))
            {
                blocks.Add(new Dictionary<string, object?>
                {
                    ["type"] = "section",
                    ["fields"] = chunk
                        .Select(metric => (object)new Dictionary<string, object?>
                        {
                            ["type"] = "mrkdwn",
                            ["text"] = $"*{metric.Label}*\n`{FormatMetricValue(metric.Value)}`"
                        })
                        .ToArray()
                });
            }
        }

        foreach (var section in card.Sections)
        {
            blocks.Add(new Dictionary<string, object?> { ["type"] = "divider" });
            var text = string.IsNullOrWhiteSpace(section.Title)
                ? section.Text.Trim()
                : $"*{section.Title.Trim()}*\n{section.Text.Trim()}";

            blocks.Add(new Dictionary<string, object?>
            {
                ["type"] = "section",
                ["text"] = new Dictionary<string, object?>
                {
                    ["type"] = "mrkdwn",
                    ["text"] = Truncate(text, 3000)
                }
            });
        }

        if (card.Actions.Count > 0)
        {
            blocks.Add(new Dictionary<string, object?> { ["type"] = "divider" });
            blocks.Add(new Dictionary<string, object?>
            {
                ["type"] = "actions",
                ["elements"] = card.Actions
                    .Select(action => (object)new Dictionary<string, object?>
                    {
                        ["type"] = "button",
                        ["text"] = new Dictionary<string, object?>
                        {
                            ["type"] = "plain_text",
                            ["text"] = Truncate(action.Label, 75),
                            ["emoji"] = true
                        },
                        ["url"] = action.Url,
                        ["style"] = "primary"
                    })
                    .ToArray()
            });
        }

        blocks.Add(new Dictionary<string, object?>
        {
            ["type"] = "context",
            ["elements"] = new object[]
            {
                new Dictionary<string, object?> { ["type"] = "mrkdwn", ["text"] = "ThaiX" }
            }
        });

        return new Dictionary<string, object?>
        {
            ["color"] = NotificationBrand.SeverityColorHex(card.Severity),
            ["blocks"] = blocks
        };
    }

    private static IEnumerable<IReadOnlyList<T>> Chunk<T>(IReadOnlyList<T> source, int size)
    {
        for (var i = 0; i < source.Count; i += size)
        {
            yield return source.Skip(i).Take(size).ToList();
        }
    }

    private static string Truncate(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength - 1), "…");
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
}
