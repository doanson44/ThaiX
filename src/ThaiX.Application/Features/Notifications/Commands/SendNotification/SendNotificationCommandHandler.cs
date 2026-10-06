using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Features.Notifications.Commands.SendNotification;

public sealed class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Guid>
{
    private readonly INotificationService _notificationService;

    public SendNotificationCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public Task<Guid> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<NotificationChannel> channels = request.Target switch
        {
            NotificationTarget.Slack => [NotificationChannel.Slack],
            NotificationTarget.Telegram => [NotificationChannel.Telegram],
            NotificationTarget.Both => new[] { NotificationChannel.Slack, NotificationChannel.Telegram },
            _ => []
        };

        return _notificationService.CreateAsync(
            new CreateNotificationRequest
            {
                Kind = request.EventType,
                Severity = request.Severity ?? NotificationSeverity.Info,
                Text = request.Text,
                Title = request.Title,
                Channels = channels
            },
            cancellationToken);
    }
}
