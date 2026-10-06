using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

/// <summary>
/// Fake email sender for integration tests.
/// Records sent emails instead of actually sending them.
/// </summary>
public sealed class FakeEmailSender : IEmailSender
{
    private readonly List<SentEmail> _sentEmails = [];

    public IReadOnlyList<SentEmail> SentEmails => _sentEmails.AsReadOnly();

    public Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        _sentEmails.Add(new SentEmail(toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }

    public void Clear() => _sentEmails.Clear();
}

public sealed record SentEmail(string To, string Subject, string HtmlBody);
