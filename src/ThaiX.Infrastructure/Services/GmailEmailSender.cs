using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.Services;

/// <summary>
/// Email sender implementation using Gmail SMTP.
/// Configured via <see cref="EmailSettings"/> options bound from appsettings.
/// </summary>
public sealed class GmailEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<GmailEmailSender> _logger;

    public GmailEmailSender(
        IOptions<EmailSettings> options,
        ILogger<GmailEmailSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword),
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {ToEmail}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}: {Subject}", toEmail, subject);
            throw;
        }
    }
}
