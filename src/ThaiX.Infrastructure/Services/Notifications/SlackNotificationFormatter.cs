using System.Globalization;
using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class SlackNotificationFormatter : INotificationFormatter
{
    private const int MaxFieldsPerBlock = 10;
    private const int MaxActionsPerBlock = 25;

    public NotificationChannel Channel => NotificationChannel.Slack;

    public string Format(NotificationCard card) => Format(card, null);

    public string Format(NotificationCard card, NotificationKind? kind)
    {
        ArgumentNullException.ThrowIfNull(card);

        var builder = new StringBuilder();
        var category = GetCategory(card, kind);
        var severity = NotificationTemplateResources.SeverityLabel(card.Severity);
        var severityIcon = NotificationTemplateResources.SeverityIcon(card.Severity);

        builder.Append($"*{EscapeMrkdwn(card.Title)}*\n");
        builder.Append($"_{EscapeMrkdwn(category)}_ | {severityIcon} {severity}\n\n");

        if (card.Metrics.Count > 0)
        {
            builder.Append($"*{EscapeMrkdwn(NotificationTemplateResources.CardTypeLabel(card.Type))}*\n");
            foreach (var metric in card.Metrics)
            {
                builder.Append($"*{EscapeMrkdwn(metric.Label)}*  `{EscapeCode(FormatMetricValue(metric.Value))}`\n");
            }

            builder.Append('\n');
        }

        foreach (var section in card.Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Title))
            {
                builder.Append($"*{EscapeMrkdwn(section.Title.Trim())}*\n");
            }

            builder.Append($"{EscapeMrkdwn(section.Text.Trim())}\n\n");
        }

        foreach (var action in card.Actions.Where(action => IsSafeLink(action.Url)))
        {
            builder.Append($"<{action.Url.Trim()}|{EscapeMrkdwn(action.Label.Trim())}>\n");
        }

        builder.Append($"\n_{EscapeMrkdwn(NotificationTemplateResources.Get("BrandFooter"))}_");
        return builder.ToString().Trim();
    }

    public object BuildAttachment(NotificationCard card) => BuildAttachment(card, null);

    public object BuildAttachment(NotificationCard card, NotificationKind? kind)
    {
        ArgumentNullException.ThrowIfNull(card);

        var category = GetCategory(card, kind);
        var severity = NotificationTemplateResources.SeverityLabel(card.Severity);
        var severityIcon = NotificationTemplateResources.SeverityIcon(card.Severity);
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
                        ["text"] = $"*{EscapeMrkdwn(category)}*  |  {severityIcon} {severity}"
                    }
                }
            }
        };

        if (card.Metrics.Count > 0)
        {
            blocks.Add(new Dictionary<string, object?>
            {
                ["type"] = "section",
                ["text"] = new Dictionary<string, object?>
                {
                    ["type"] = "mrkdwn",
                    ["text"] = $"*{EscapeMrkdwn(NotificationTemplateResources.CardTypeLabel(card.Type))}*"
                }
            });
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
                            ["text"] = $"*{EscapeMrkdwn(metric.Label)}*\n`{EscapeCode(FormatMetricValue(metric.Value))}`"
                        })
                        .ToArray()
                });
            }
        }

        foreach (var section in card.Sections)
        {
            var text = string.IsNullOrWhiteSpace(section.Title)
                ? EscapeMrkdwn(section.Text.Trim())
                : $"*{EscapeMrkdwn(section.Title.Trim())}*\n{EscapeMrkdwn(section.Text.Trim())}";

            blocks.Add(new Dictionary<string, object?> { ["type"] = "divider" });
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

        var safeActions = card.Actions.Where(action => IsSafeLink(action.Url)).Take(MaxActionsPerBlock).ToArray();
        if (safeActions.Length > 0)
        {
            blocks.Add(new Dictionary<string, object?> { ["type"] = "divider" });
            blocks.Add(new Dictionary<string, object?>
            {
                ["type"] = "actions",
                ["elements"] = safeActions
                    .Select(action => (object)new Dictionary<string, object?>
                    {
                        ["type"] = "button",
                        ["text"] = new Dictionary<string, object?>
                        {
                            ["type"] = "plain_text",
                            ["text"] = Truncate(action.Label, 75),
                            ["emoji"] = true
                        },
                        ["url"] = action.Url.Trim(),
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
                new Dictionary<string, object?>
                {
                    ["type"] = "mrkdwn",
                    ["text"] = NotificationTemplateResources.Get("BrandFooter")
                }
            }
        });

        return new Dictionary<string, object?>
        {
            ["color"] = NotificationBrand.SeverityColorHex(card.Severity),
            ["blocks"] = blocks
        };
    }

    private static string GetCategory(NotificationCard card, NotificationKind? kind)
    {
        return kind.HasValue ? NotificationTemplateResources.KindLabel(kind.Value) : card.Category;
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
        return text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength - 3), "...");
    }

    private static string EscapeMrkdwn(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("*", "\\*")
            .Replace("_", "\\_")
            .Replace("~", "\\~")
            .Replace("|", "\\|");
    }

    private static string EscapeCode(string text) => text.Replace("`", "'");

    private static bool IsSafeLink(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
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
