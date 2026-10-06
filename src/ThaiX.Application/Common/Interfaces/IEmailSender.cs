namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Abstraction for sending emails.
/// Implementation lives in Infrastructure layer.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
