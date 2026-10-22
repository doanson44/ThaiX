using ThaiX.Application.Common.Models;
using ThaiX.Infrastructure.Services.Notifications;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

public sealed class FakeSlackNotificationService : ISlackNotificationService
{
    private readonly List<NotificationMessage> _messages = [];

    public IReadOnlyList<NotificationMessage> Messages => _messages.AsReadOnly();
    public List<string> ChannelMessages { get; } = [];
    public List<string> ResponseUrlMessages { get; } = [];

    public Task SendAsync(NotificationMessage message, CancellationToken ct)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    public Task SendToChannelAsync(string channelId, string text, string? threadTs, CancellationToken ct, string? attachmentsJson = null)
    {
        ChannelMessages.Add(text);
        return Task.CompletedTask;
    }

    public Task SendToResponseUrlAsync(string responseUrl, string text, CancellationToken ct)
    {
        ResponseUrlMessages.Add(text);
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _messages.Clear();
        ChannelMessages.Clear();
        ResponseUrlMessages.Clear();
    }
}
