using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Configuration;
using ThaiX.Application.Features.Notifications.Scheduling.Commands.ProcessDueSchedules;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class DueSchedulePoller
{
    private readonly IMediator _mediator;
    private readonly ILogger<DueSchedulePoller> _logger;
    private readonly NotificationSchedulingSettings _settings;

    public DueSchedulePoller(
        IMediator mediator,
        ILogger<DueSchedulePoller> logger,
        IOptions<NotificationSchedulingSettings> settings)
    {
        _mediator = mediator;
        _logger = logger;
        _settings = settings.Value;
    }

    [Hangfire.AutomaticRetry(Attempts = 0)]
    [Hangfire.DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            var processed = await _mediator.Send(new ProcessDueSchedulesCommand(), ct);
            if (processed > 0)
            {
                _logger.LogInformation("Processed {Count} due notification schedules", processed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process due notification schedules");
        }
    }
}
