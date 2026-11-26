using System.Globalization;
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

        Assert.Contains("<b>\U0001F3AF PRICE ALERT</b>", result);
        Assert.Contains("<b>\U0001F6A8 ALERT DETAILS</b>", result);
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
        Assert.Contains("\U0001F6A8 SYSTEM ALERT", json);
        Assert.Contains("12,500", json);
        Assert.Contains("Upstream &lt;service&gt; failed.", fallback);
        Assert.Contains("\U0001F6AB INCIDENT DETAILS", json);
        Assert.Contains("https://example.com/logs", json);
        Assert.DoesNotContain("javascript:", json);
        Assert.Contains("\U0001F6A8 CRITICAL", fallback);
        Assert.Contains("\U0001F6AB INCIDENT DETAILS", fallback);
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

        Assert.Contains("\U0001F504 DATA OPERATIONS", result.Text);
        Assert.Contains("Details", result.Text);
        Assert.NotNull(result.AttachmentsJson);
        Assert.Contains("\U0001F504 DATA OPERATIONS", result.AttachmentsJson);
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

        Assert.Contains("<b>\U0001F6A8 SYSTEM ALERT</b>", result.Text);
        Assert.Contains("Provider returned &lt;HTTP 503&gt;.", result.Text);
        Assert.Contains("parse_mode", result.PayloadJson);
        Assert.Contains("FINANCIAL INTELLIGENCE", result.Text);
    }

    [Fact]
    public void Formatters_UseVietnameseResourcesWhenCurrentUiCultureIsVietnamese()
    {
        var originalCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("vi-VN");
            var card = new NotificationCard
            {
                Category = "Notification",
                Title = "Th\u1ECB tr\u01B0\u1EDDng h\u00F4m nay",
                Type = NotificationCardType.Metric,
                Severity = NotificationSeverity.Warning,
                Metrics = [new NotificationMetric { Label = "Gi\u00E1", Value = "12500" }]
            };

            var telegram = new TelegramNotificationFormatter().Format(card, NotificationKind.MarketOverview);
            var slack = new SlackNotificationFormatter().Format(card, NotificationKind.MarketOverview);
            var subject = new DefaultNotificationTemplateProvider()
                .GetTemplate(NotificationKind.MarketOverview, null).DefaultSubject;

            Assert.Contains("\U0001F4CA T\u1ED4NG QUAN TH\u1EAC TR\u01AF\u1EDCNG", telegram);
            Assert.Contains("\u26A0\uFE0F C\u1EA2NH B\u00C1O", telegram);
            Assert.Contains("\U0001F4CA D\u1EEE LI\u1EC6U TH\u1EAC TR\u01AF\u1EDCNG", telegram);
            Assert.Contains("TR\u00CD TU\u1EC6 T\u00C0I CH\u00CDNH", telegram);
            Assert.Contains("\U0001F4CA T\u1ED4NG QUAN TH\u1EAC TR\u01AF\u1EDCNG", slack);
            Assert.Contains("\u26A0\uFE0F C\u1EA2NH B\u00C1O", slack);
            Assert.Contains("T\u1ED5ng quan th\u1ECB tr\u01B0\u1EDDng", subject);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Theory]
    [InlineData(NotificationKind.MarketOverview, "MARKET OVERVIEW")]
    [InlineData(NotificationKind.PriceUpdate, "PRICE UPDATE")]
    [InlineData(NotificationKind.CryptoSpike, "MARKET MOVEMENT")]
    [InlineData(NotificationKind.GoodEntryCrypto, "CRYPTO RESEARCH")]
    [InlineData(NotificationKind.GoodEntryStocks, "EQUITY RESEARCH")]
    [InlineData(NotificationKind.SystemAlert, "SYSTEM ALERT")]
    [InlineData(NotificationKind.DataSync, "DATA OPERATIONS")]
    [InlineData(NotificationKind.MarketScanner, "MARKET SCANNER")]
    [InlineData(NotificationKind.PriceAlert, "PRICE ALERT")]
    [InlineData(NotificationKind.PortfolioUpdate, "PORTFOLIO")]
    [InlineData(NotificationKind.TradeSignal, "TRADE SIGNAL")]
    public void Formatters_UseConsistentPresentationLabelsForEveryNotificationKind(
        NotificationKind kind,
        string expectedLabel)
    {
        var card = new NotificationCard
        {
            Category = "Notification",
            Title = "Test event",
            Type = NotificationCardType.Summary
        };

        var telegram = new TelegramNotificationFormatter().Format(card, kind);
        var slack = new SlackNotificationFormatter().Format(card, kind);

        Assert.Contains(expectedLabel, telegram);
        Assert.Contains(expectedLabel, slack);
    }
}
