using ThaiX.Application.Common.Models;
using ThaiX.Infrastructure.Services.Notifications;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

public sealed class FakeTelegramNotificationService : ITelegramNotificationService
{
    private readonly List<NotificationMessage> _messages = [];

    public IReadOnlyList<NotificationMessage> Messages => _messages.AsReadOnly();
    public List<string> ChatMessages { get; } = [];
    public List<string> RenderedChatMessages { get; } = [];

    public Task SendAsync(NotificationMessage message, CancellationToken ct)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    public Task SendToChatAsync(string chatId, string text, CancellationToken ct)
    {
        ChatMessages.Add(text);
        return Task.CompletedTask;
    }

    public Task SendRenderedToChatAsync(string chatId, string htmlText, CancellationToken ct)
    {
        RenderedChatMessages.Add(htmlText);
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _messages.Clear();
        ChatMessages.Clear();
        RenderedChatMessages.Clear();
    }
}
