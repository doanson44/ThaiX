using System.Text;
using ThaiX.Application.Common.Emails;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class EmailNotificationFormatter : INotificationFormatter
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public string Format(NotificationCard card)
    {
        var body = new StringBuilder();

        body.Append(
            $"""<p style="margin:0 0 18px;font-size:13px;color:{NotificationBrand.TextFaintHex};">{Encode(card.Category)} &middot; {NotificationBrand.SeverityIcon(card.Severity)} {card.Severity}</p>""");

        if (card.Metrics.Count > 0)
        {
            body.Append("""<table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:18px;">""");
            foreach (var metric in card.Metrics)
            {
                body.Append(
                    $"""
                    <tr>
                      <td style="padding:6px 0;font-size:13px;color:{NotificationBrand.TextFaintHex};border-bottom:1px solid {NotificationBrand.SurfaceBorderHex};">{Encode(metric.Label)}</td>
                      <td style="padding:6px 0;font-size:13px;font-family:'Consolas','Courier New',monospace;color:{NotificationBrand.TextPrimaryHex};text-align:right;border-bottom:1px solid {NotificationBrand.SurfaceBorderHex};">{Encode(metric.Value)}</td>
                    </tr>
                    """);
            }

            body.Append("</table>");
        }

        foreach (var section in card.Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Title))
            {
                body.Append(
                    $"""<h4 style="margin:16px 0 6px;font-size:14px;color:{NotificationBrand.TextPrimaryHex};">{Encode(section.Title.Trim())}</h4>""");
            }

            body.Append($"""<p style="margin:0 0 8px;">{Encode(section.Text.Trim())}</p>""");
        }

        foreach (var action in card.Actions)
        {
            body.Append(
                $"""<p style="margin:14px 0 0;"><a href="{Encode(action.Url.Trim())}" style="color:{NotificationBrand.AccentHex};text-decoration:none;font-weight:600;">🔗 {Encode(action.Label.Trim())}</a></p>""");
        }

        return EmailTemplateBuilder.Build(Encode(card.Title), body.ToString());
    }

    private static string Encode(string text) => System.Net.WebUtility.HtmlEncode(text);
}
