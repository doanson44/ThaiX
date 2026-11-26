using System.Text.Json;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.Services.Notifications;

namespace ThaiX.Presentation.IntegrationTests.Notifications;

public sealed class NotificationFormatterTests
{
    [Fact]
    public void TelegramFormatter_FormatsMetricsAsMobileRowsAndEscapesContent()
    {
        var formatter = new TelegramNotificationFormatter();
        var card = new NotificationCard
        {
            Category = "Price Alert",
            Title = "BTC <USDT>",
            Type = NotificationCardType.Alert,
            Severity = NotificationSeverity.Warning,
            Metrics =
            [
                new NotificationMetric { Label = "Last & Price", Value = "68420.5" },
                new NotificationMetric { Label = "Change", Value = "3.42%" }
            ],
            Actions =
            [
                new NotificationAction { Label = "Open market", Url = "https://example.com/market?a=1&b=2" },
                new NotificationAction { Label = "Unsafe", Url = "javascript:alert(1)" }
            ]
        };

        var result = formatter.Format(card, NotificationKind.PriceAlert);

        Assert.Contains("<b>PRICE ALERT</b>", result);
        Assert.Contains("BTC &lt;USDT&gt;", result);
        Assert.Contains("<b>Last &amp; Price</b>: <code>68,420.5</code>", result);
        Assert.Contains("<code>+3.42%</code>", result);
        Assert.Contains("href=\"https://example.com/market?a=1&amp;b=2\"", result);
        Assert.DoesNotContain("javascript:", result);
        Assert.DoesNotContain("PadRight", result);
        Assert.Contains("ThaiX | FINANCIAL INTELLIGENCE", result);
    }

    [Fact]
    public void SlackFormatter_BuildsPremiumBlocksAndEscapesDynamicMrkdwn()
    {
        var formatter = new SlackNotificationFormatter();
        var card = new NotificationCard
        {
            Category = "System Alert",
            Title = "Provider <unavailable>",
            Type = NotificationCardType.Error,
            Severity = NotificationSeverity.Critical,
            Metrics = [new NotificationMetric { Label = "Records", Value = "12500" }],
            Sections =
            [
                new NotificationSection { Title = "Failure *details*", Text = "Upstream <service> failed." }
            ],
            Actions =
            [
                new NotificationAction { Label = "Open logs", Url = "https://example.com/logs" },
                new NotificationAction { Label = "Unsafe", Url = "javascript:alert(1)" }
            ]
        };

        var result = formatter.BuildAttachment(card, NotificationKind.SystemAlert);
        var json = JsonSerializer.Serialize(result);
        var fallback = formatter.Format(card, NotificationKind.SystemAlert);

        Assert.Contains(NotificationBrand.NegativeHex, json);
        Assert.Contains("SYSTEM ALERT", json);
        Assert.Contains("12,500", json);
        Assert.Contains("Upstream &lt;service&gt; failed.", json);
        Assert.Contains("https://example.com/logs", json);
        Assert.DoesNotContain("javascript:", json);
        Assert.Contains("CRITICAL", fallback);
        Assert.Contains("ThaiX | FINANCIAL INTELLIGENCE", fallback);
    }

    [Fact]
    public void SlackRenderer_UsesSharedPremiumLayoutForLegacyMessages()
    {
        var renderer = new SlackNotificationRenderer(new SlackNotificationFormatter());
        var message = new NotificationMessage
        {
            Kind = NotificationKind.DataSync,
            Severity = NotificationSeverity.Error,
            Title = "Market data sync",
            Text = "Provider returned HTTP 503.",
            Actions = [new NotificationAction { Label = "View status", Url = "https://example.com/status" }]
        };

        var result = renderer.Render(
            message,
            new NotificationRoute
            {
                Channel = NotificationChannel.Slack,
                Provider = "Slack",
                Destination = "C123"
            },
            "notification-1");

        Assert.Contains("DATA OPERATIONS", result.Text);
        Assert.Contains("Details", result.Text);
        Assert.NotNull(result.AttachmentsJson);
        Assert.Contains("DATA OPERATIONS", result.AttachmentsJson);
    }

    [Fact]
    public void TelegramRenderer_UsesSharedPremiumLayoutForLegacyMessages()
    {
        var renderer = new TelegramNotificationRenderer(new TelegramNotificationFormatter());
        var message = new NotificationMessage
        {
            Kind = NotificationKind.SystemAlert,
            Severity = NotificationSeverity.Critical,
            Title = "Sync failed",
            Text = "Provider returned <HTTP 503>."
        };

        var result = renderer.Render(
            message,
            new NotificationRoute
            {
                Channel = NotificationChannel.Telegram,
                Provider = "Telegram",
                Destination = "123456"
            },
            "notification-2");

        Assert.Contains("<b>SYSTEM ALERT</b>", result.Text);
        Assert.Contains("Provider returned &lt;HTTP 503&gt;.", result.Text);
        Assert.Contains("parse_mode", result.PayloadJson);
        Assert.Contains("FINANCIAL INTELLIGENCE", result.Text);
    }
}
