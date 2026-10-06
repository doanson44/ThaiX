using ThaiX.Application.Common.Models;
using ThaiX.Infrastructure.Services.Notifications;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

public sealed class FakeSlackNotificationService : ISlackNotificationService
{
    private readonly List<NotificationMessage> _messages = [];

    public IReadOnlyList<NotificationMessage> Messages => _messages.AsReadOnly();

    public Task SendAsync(NotificationMessage message, CancellationToken ct)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    public Task SendToChannelAsync(string channelId, string text, string? threadTs, CancellationToken ct, string? attachmentsJson = null)
    {
        return Task.CompletedTask;
    }

    public Task SendToResponseUrlAsync(string responseUrl, string text, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
