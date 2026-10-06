using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class SlackNotificationChannelProvider : INotificationChannelProvider
{
    private readonly ISlackNotificationService _slackNotificationService;

    public SlackNotificationChannelProvider(ISlackNotificationService slackNotificationService)
    {
        _slackNotificationService = slackNotificationService ?? throw new ArgumentNullException(nameof(slackNotificationService));
    }

    public NotificationChannel Channel => NotificationChannel.Slack;

    public string Provider => "Slack";

    public async Task<NotificationSendResult> SendAsync(
        NotificationRenderedMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await _slackNotificationService.SendToChannelAsync(
                message.Destination,
                message.Text,
                threadTs: null,
                cancellationToken,
                message.AttachmentsJson);

            return NotificationSendResult.Sent(providerMessageId: null);
        }
        catch (Exception ex)
        {
            return NotificationSendResult.Failed(ex.GetType().Name, ex.Message);
        }
    }
}
