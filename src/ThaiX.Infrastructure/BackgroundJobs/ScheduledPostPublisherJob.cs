using Hangfire;
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
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class ScheduledPostPublisherJob(
    IApplicationDbContext context,
    IHangfireJobState hangfireJobState,
    ILogger<ScheduledPostPublisherJob> logger) : HangfireJobBase(hangfireJobState, logger)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.ScheduledPostPublisherJob)) return;

        var now = DateTime.UtcNow;

        var duePosts = await context.Posts
            .Where(p => p.Status == PostStatus.Scheduled && p.ScheduledAt <= now && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        if (duePosts.Count == 0)
        {
            logger.LogDebug("ScheduledPostPublisherJob: no due posts, skipping");
            return;
        }

        foreach (var post in duePosts)
        {
            post.Publish();
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("ScheduledPostPublisherJob: published {Count} post(s)", duePosts.Count);
    }
}
