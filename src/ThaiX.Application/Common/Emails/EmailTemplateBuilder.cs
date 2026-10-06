using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Common.Emails;

/// <summary>
/// Wraps transactional email content in the "dark-finance" brand shell (see
/// docs/implement-plans/audit-checklist-dark-finance.md) so every ThaiX email looks like
/// one product instead of ad-hoc inline HTML per call site. Uses table-based layout and
/// web-safe fonts only, since most mail clients strip &lt;style&gt; blocks and won't load
/// custom web fonts.
/// </summary>
public static class EmailTemplateBuilder
{
    public sealed record CallToAction(string Label, string Url);

    public static string Build(string heading, string bodyHtml, CallToAction? cta = null, string? footNote = null)
    {
        var ctaHtml = cta is null
            ? string.Empty
            : $"""
               <div style="margin-top:28px;text-align:center;">
                 <a href="{cta.Url}" style="display:inline-block;padding:12px 32px;background:{NotificationBrand.AccentHex};color:#1a1207;font-weight:600;font-size:14px;text-decoration:none;border-radius:8px;font-family:'Segoe UI',Arial,sans-serif;">{cta.Label}</a>
               </div>
               """;

        var footNoteHtml = string.IsNullOrWhiteSpace(footNote)
            ? string.Empty
            : $"""<p style="margin:20px 0 0;font-size:13px;line-height:1.6;color:{NotificationBrand.TextFaintHex};">{footNote}</p>""";

        return $"""
            <!doctype html>
            <html>
            <body style="margin:0;padding:40px 16px;background:#05070b;font-family:'Segoe UI',Arial,sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:480px;margin:0 auto;">
                <tr>
                  <td style="padding-bottom:24px;text-align:center;">
                    <span style="font-family:Georgia,'Times New Roman',serif;font-size:22px;font-weight:700;color:{NotificationBrand.AccentHex};letter-spacing:0.5px;">ThaiX</span>
                  </td>
                </tr>
                <tr>
                  <td style="background:{NotificationBrand.SurfaceHex};border:1px solid {NotificationBrand.SurfaceBorderHex};border-radius:12px;padding:32px 28px;">
                    <h1 style="margin:0 0 14px;font-family:Georgia,'Times New Roman',serif;font-size:20px;font-weight:700;color:{NotificationBrand.TextPrimaryHex};">{heading}</h1>
                    <div style="font-size:14px;line-height:1.7;color:{NotificationBrand.TextMutedHex};">{bodyHtml}</div>
                    {ctaHtml}
                    {footNoteHtml}
                  </td>
                </tr>
                <tr>
                  <td style="padding-top:20px;text-align:center;font-size:12px;color:{NotificationBrand.TextFaintHex};">
                    This email was sent by ThaiX. If you weren't expecting it, you can safely ignore it.
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
