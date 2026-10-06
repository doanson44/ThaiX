using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Infrastructure.Services;

public sealed class NotificationRouter : INotificationRouter
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationRouter> _logger;

    public NotificationRouter(
        INotificationService notificationService,
        ILogger<NotificationRouter> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Guid> DispatchAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation(
            "Creating notification {Kind} with {ChannelCount} requested channels",
            message.Kind,
            message.Channels.Count);

        return _notificationService.CreateAsync(
            new CreateNotificationRequest
            {
                Kind = message.Kind,
                Severity = message.Severity,
                Text = message.Text,
                Title = message.Title,
                TemplateKey = message.TemplateKey,
                RecipientUserId = message.RecipientUserId,
                DataJson = message.DataJson,
                SourceEventId = message.SourceEventId,
                DeduplicationKey = message.DeduplicationKey,
                ScheduledAtUtc = message.ScheduledAtUtc,
                Channels = message.Channels,
                Card = message.Card
            },
            cancellationToken);
    }
}
