using Microsoft.Playwright;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ThaiX.Application.Common.Emails;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.Services.Notifications;
using Xunit;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Renders the *real* Telegram/Slack/Email formatter output (no hand-copied markup) into a
/// single static preview page mocking each channel's native chrome, then screenshots it with
/// Playwright. Telegram/Slack are text+JSON APIs with no browser surface of their own, so this
/// is how the redesigned "dark-finance" notification templates get visually verified without a
/// live bot token or Slack workspace.
/// </summary>
public sealed class NotificationDesignPreviewTests
{
    private static readonly string PreviewDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "NotificationPreview");

    private static readonly string ScreenshotDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TestResults", "NotificationPreviews");

    [Fact]
    public async Task NotificationPreview_RendersAndCapturesScreenshot()
    {
        Directory.CreateDirectory(PreviewDir);
        Directory.CreateDirectory(ScreenshotDir);

        var htmlPath = Path.Combine(PreviewDir, "preview.generated.html");
        await File.WriteAllTextAsync(htmlPath, BuildPreviewPage());

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1180, Height = 1000 }
        });

        await page.GotoAsync(new Uri(Path.GetFullPath(htmlPath)).AbsoluteUri);
        await page.WaitForTimeoutAsync(300);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(ScreenshotDir, "notification-design-preview.png"),
            FullPage = true
        });
    }

    // ===== Sample data — mirrors what the real background jobs build =====

    private static readonly NotificationCard PriceAlertCard = new()
    {
        Category = "Price Alert",
        Title = "BTCUSDT crossed above target",
        Type = NotificationCardType.Alert,
        Severity = NotificationSeverity.Warning,
        Metrics =
        [
            new NotificationMetric { Label = "Condition", Value = "Price > 68000" },
            new NotificationMetric { Label = "Current", Value = "68420.5" },
            new NotificationMetric { Label = "Change 24h", Value = "3.42%" },
            new NotificationMetric { Label = "Status", Value = "Triggered" }
        ],
        Actions = [new NotificationAction { Label = "Open MEXC", Url = "https://www.mexc.com/exchange/BTC_USDT" }]
    };

    private static readonly NotificationCard WeeklySuggestionCard = new()
    {
        Category = "Weekly Suggestion",
        Title = "MEXC Spot Crypto Suggestions",
        Type = NotificationCardType.Signal,
        Severity = NotificationSeverity.Info,
        Metrics =
        [
            new NotificationMetric { Label = "Entry", Value = "2.145" },
            new NotificationMetric { Label = "Stop Loss", Value = "1.980" },
            new NotificationMetric { Label = "TP1", Value = "2.350" },
            new NotificationMetric { Label = "TP2", Value = "2.610" },
            new NotificationMetric { Label = "Score", Value = "8.4" }
        ],
        Sections =
        [
            new NotificationSection
            {
                Title = "🟢 SUIUSDT — Long",
                Text = "Breakout above the weekly resistance with rising volume; momentum favors continuation into the next range."
            }
        ],
        Actions = [new NotificationAction { Label = "View Full Report", Url = "https://ThaiX.app/market-research/weekly-suggestions" }]
    };

    private static readonly NotificationCard SystemAlertCard = new()
    {
        Category = "System Alert",
        Title = "ChainBroker sync failed",
        Type = NotificationCardType.Error,
        Severity = NotificationSeverity.Critical,
        Sections =
        [
            new NotificationSection { Text = "Unlock schedule sync failed after 3 attempts: upstream API returned HTTP 503." }
        ]
    };

    private static readonly (string Heading, string Body, string CtaLabel, string Url, string? FootNote)[] AuthEmails =
    [
        ("Welcome to ThaiX!", "<p>Please confirm your email address to activate your account.</p>", "Confirm Email",
            "https://ThaiX.app/confirm-email?userId=…&token=…", "If you did not create an account, you can safely ignore this email."),
        ("Reset your password", "<p>Click the button below to choose a new password.</p>", "Reset Password",
            "https://ThaiX.app/reset-password?userId=…&token=…", "If you did not request this, you can safely ignore this email.")
    ];

    // ===== Page assembly =====

    private static string BuildPreviewPage()
    {
        var telegramFormatter = new TelegramNotificationFormatter();
        var slackFormatter = new SlackNotificationFormatter();
        var emailFormatter = new EmailNotificationFormatter();

        var cards = new[] { PriceAlertCard, WeeklySuggestionCard, SystemAlertCard };

        var rows = new StringBuilder();
        foreach (var card in cards)
        {
            var telegramHtml = telegramFormatter.Format(card).Replace("\n", "<br/>");
            var slackAttachment = (Dictionary<string, object?>)slackFormatter.BuildAttachment(card);

            rows.Append(
                $"""
                <div class="channel-row">
                  <div class="channel-col">
                    <div class="chrome-label">Telegram</div>
                    {RenderTelegramMock(telegramHtml)}
                  </div>
                  <div class="channel-col">
                    <div class="chrome-label">Slack</div>
                    {RenderSlackMock(slackAttachment)}
                  </div>
                </div>
                """);
        }

        var emailCardsHtml = string.Join("\n", cards.Select(card =>
            $"""<iframe class="email-frame" srcdoc="{WebUtility.HtmlEncode(emailFormatter.Format(card))}"></iframe>"""));

        var authEmailsHtml = string.Join("\n", AuthEmails.Select(email =>
        {
            var html = EmailTemplateBuilder.Build(
                email.Heading, email.Body, new EmailTemplateBuilder.CallToAction(email.CtaLabel, email.Url), email.FootNote);
            return $"""<iframe class="email-frame" srcdoc="{WebUtility.HtmlEncode(html)}"></iframe>""";
        }));

        return $$"""
            <!doctype html>
            <html>
            <head>
            <meta charset="utf-8"/>
            <title>ThaiX Notification Design Preview</title>
            <style>
                body { margin:0; padding:32px; background:#05070b; font-family:'Segoe UI',Arial,sans-serif; color:#e6e8ec; }
                h1 { font-family:Georgia,serif; font-size:26px; color:#f59e0b; margin:0 0 4px; }
                h2 { font-size:14px; text-transform:uppercase; letter-spacing:1px; color:#5b6272; margin:40px 0 16px; font-weight:600; }
                .channel-row { display:flex; gap:20px; margin-bottom:28px; }
                .channel-col { flex:1; min-width:0; }
                .chrome-label { font-size:11px; text-transform:uppercase; letter-spacing:1px; color:#5b6272; margin-bottom:8px; }

                /* Telegram mock chrome */
                .tg-app { background:#0e1621; border-radius:12px; padding:16px; }
                .tg-bubble { background:#182533; border-radius:10px; padding:12px 14px; color:#e9edf1; font-size:14px; line-height:1.5; max-width:100%; }
                .tg-bot { color:#8fceff; font-weight:600; font-size:12.5px; margin-bottom:6px; }
                .tg-bubble b { color:#ffffff; }
                .tg-bubble i { color:#a9b4bf; }
                .tg-bubble code { background:#0e1621; border-radius:4px; padding:1px 5px; font-family:'Consolas',monospace; color:#7fd8a3; }
                .tg-bubble a { color:#6cb8ff; }

                /* Slack mock chrome */
                .sl-app { background:#1a1d21; border-radius:12px; padding:16px; }
                .sl-msg-head { display:flex; align-items:center; gap:8px; margin-bottom:6px; }
                .sl-avatar { width:28px; height:28px; border-radius:6px; background:#f59e0b; color:#1a1207; font-weight:700; display:flex; align-items:center; justify-content:center; font-size:13px; }
                .sl-sender { font-weight:700; font-size:13.5px; color:#fff; }
                .sl-badge { font-size:10px; background:#2b2f36; color:#9a9ea6; padding:1px 5px; border-radius:3px; }
                .sl-attachment { border-left:4px solid #5b7c99; background:#22252b; border-radius:0 6px 6px 0; padding:10px 14px; }
                .sl-header { font-weight:700; font-size:15px; color:#fff; margin-bottom:4px; }
                .sl-context { font-size:12px; color:#9a9ea6; margin:2px 0; }
                .sl-divider { border:none; border-top:1px solid #34383f; margin:8px 0; }
                .sl-fields { display:grid; grid-template-columns:1fr 1fr; gap:8px 16px; margin:6px 0; }
                .sl-field { font-size:13px; color:#d1d3d6; }
                .sl-section { font-size:13px; color:#d1d3d6; margin:6px 0; }
                .sl-actions { margin-top:8px; }
                .sl-btn { display:inline-block; background:#2b2f36; border:1px solid #454951; color:#e9edf1; text-decoration:none; font-size:12.5px; font-weight:600; padding:6px 12px; border-radius:6px; }
                .sl-attachment strong { color:#fff; }
                .sl-attachment code { background:#0e0f12; border-radius:4px; padding:1px 5px; font-family:'Consolas',monospace; color:#7fd8a3; }

                .email-frame { width:100%; height:520px; border:1px solid #232838; border-radius:10px; margin-bottom:20px; background:#05070b; }
            </style>
            </head>
            <body>
                <h1>ThaiX Notification Design Preview</h1>
                <div style="color:#9aa1ad;font-size:13px;">Rendered directly from the production formatters — Telegram &amp; Slack mockups reproduce native chrome around real output; Email frames are the actual generated documents.</div>

                <h2>Telegram &amp; Slack</h2>
                {{rows}}

                <h2>Email — notification cards</h2>
                {{emailCardsHtml}}

                <h2>Email — account &amp; auth</h2>
                {{authEmailsHtml}}
            </body>
            </html>
            """;
    }

    private static string RenderTelegramMock(string telegramHtml)
    {
        return $"""
            <div class="tg-app">
              <div class="tg-bot">ThaiX Bot</div>
              <div class="tg-bubble">{telegramHtml}</div>
            </div>
            """;
    }

    private static string RenderSlackMock(IReadOnlyDictionary<string, object?> attachment)
    {
        var color = (string)attachment["color"]!;
        var blocks = (List<object>)attachment["blocks"]!;

        var body = new StringBuilder();
        foreach (var blockObj in blocks)
        {
            var block = (Dictionary<string, object?>)blockObj;
            var type = (string)block["type"]!;

            switch (type)
            {
                case "header":
                    var headerText = (string)((Dictionary<string, object?>)block["text"]!)["text"]!;
                    body.Append($"""<div class="sl-header">{WebUtility.HtmlEncode(headerText)}</div>""");
                    break;

                case "context":
                    foreach (var elObj in (object[])block["elements"]!)
                    {
                        var el = (Dictionary<string, object?>)elObj;
                        body.Append($"""<div class="sl-context">{MrkdwnToHtml((string)el["text"]!)}</div>""");
                    }
                    break;

                case "divider":
                    body.Append("""<hr class="sl-divider"/>""");
                    break;

                case "section" when block.TryGetValue("fields", out var fieldsObj) && fieldsObj is object[] fields:
                    body.Append("""<div class="sl-fields">""");
                    foreach (var fObj in fields)
                    {
                        var field = (Dictionary<string, object?>)fObj;
                        body.Append($"""<div class="sl-field">{MrkdwnToHtml((string)field["text"]!)}</div>""");
                    }
                    body.Append("</div>");
                    break;

                case "section":
                    var textBlock = (Dictionary<string, object?>)block["text"]!;
                    body.Append($"""<div class="sl-section">{MrkdwnToHtml((string)textBlock["text"]!)}</div>""");
                    break;

                case "actions":
                    body.Append("""<div class="sl-actions">""");
                    foreach (var aObj in (object[])block["elements"]!)
                    {
                        var action = (Dictionary<string, object?>)aObj;
                        var label = (string)((Dictionary<string, object?>)action["text"]!)["text"]!;
                        var url = (string)action["url"]!;
                        body.Append($"""<a class="sl-btn" href="{WebUtility.HtmlEncode(url)}">{WebUtility.HtmlEncode(label)}</a>""");
                    }
                    body.Append("</div>");
                    break;
            }
        }

        return $"""
            <div class="sl-app">
              <div class="sl-msg-head">
                <div class="sl-avatar">T</div>
                <div class="sl-sender">ThaiX</div>
                <div class="sl-badge">APP</div>
              </div>
              <div class="sl-attachment" style="border-left-color:{color};">{body}</div>
            </div>
            """;
    }

    private static string MrkdwnToHtml(string mrkdwn)
    {
        var text = WebUtility.HtmlEncode(mrkdwn);
        text = Regex.Replace(text, @"\*(.+?)\*", "<strong>$1</strong>");
        text = Regex.Replace(text, @"_(.+?)_", "<em>$1</em>");
        text = Regex.Replace(text, @"`(.+?)`", "<code>$1</code>");
        text = Regex.Replace(text, @"&lt;(https?://[^|&]+)\|([^&]+)&gt;", "<a href=\"$1\">$2</a>");
        return text.Replace("\n", "<br/>");
    }
}
