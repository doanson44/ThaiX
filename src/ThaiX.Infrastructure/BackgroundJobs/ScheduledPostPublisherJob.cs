using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Blog;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that publishes blog posts whose scheduled publish time has arrived.
/// Structurally identical to PriceAlertCheckerJob's check-then-act shape, just without the
/// market-data fetch — no notification dispatch for MVP (subscriber notifications are deferred).
/// </summary>
public sealed class ScheduledPostPublisherJob(
    IApplicationDbContext context,
    IHangfireJobState hangfireJobState,
    ILogger<ScheduledPostPublisherJob> logger) : HangfireJobBase(hangfireJobState, logger)
{
    private readonly ILogger<ScheduledPostPublisherJob> _logger = logger;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.ScheduledPostPublisherJob)) return;

        using var _ = await AcquireJobLockAsync(HangfireJobIds.ScheduledPostPublisherJob, cancellationToken);

        var now = DateTime.UtcNow;

        var duePosts = await context.Posts
            .Where(p => p.Status == PostStatus.Scheduled && p.ScheduledAt <= now && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        if (duePosts.Count == 0)
        {
            _logger.LogDebug("ScheduledPostPublisherJob: no due posts, skipping");
            return;
        }

        foreach (var post in duePosts)
        {
            post.Publish();
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ScheduledPostPublisherJob: published {Count} post(s)", duePosts.Count);
    }
}
