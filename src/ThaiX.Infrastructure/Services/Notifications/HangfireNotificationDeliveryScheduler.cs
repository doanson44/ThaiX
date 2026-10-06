using Hangfire;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.BackgroundJobs;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class HangfireNotificationDeliveryScheduler : INotificationDeliveryScheduler
{
    private readonly IBackgroundJobClient _jobClient;

    public HangfireNotificationDeliveryScheduler(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
    }

    public Task ScheduleNotificationAsync(
        Guid notificationId,
        DateTime? scheduledAtUtc,
        CancellationToken cancellationToken)
    {
        var delay = scheduledAtUtc is null
            ? TimeSpan.Zero
            : scheduledAtUtc.Value - DateTime.UtcNow;

        if (delay > TimeSpan.Zero)
        {
            _jobClient.Schedule<NotificationDeliveryJob>(
                job => job.DispatchNotificationAsync(notificationId, CancellationToken.None),
                delay);
            return Task.CompletedTask;
        }

        _jobClient.Enqueue<NotificationDeliveryJob>(
            job => job.DispatchNotificationAsync(notificationId, CancellationToken.None));
        return Task.CompletedTask;
    }
}
