using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hard-deletes JsonBins whose <c>ExpiredAtUtc</c> has passed (including soft-deleted rows).
/// Recurring Hangfire job.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class JsonBinExpiredCleanupJob : HangfireJobBase
{
    /// <summary>Max rows hard-deleted per loop iteration (large Content blobs).</summary>
    public const int BatchSize = 200;

    private readonly IApplicationDbContext _context;
    private readonly ILogger<JsonBinExpiredCleanupJob> _logger;

    public JsonBinExpiredCleanupJob(
        IApplicationDbContext context,
        IHangfireJobState hangfireJobState,
        ILogger<JsonBinExpiredCleanupJob> logger)
        : base(hangfireJobState, logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.JsonBinExpiredCleanupJob))
            return;

        var nowUtc = DateTime.UtcNow;
        var totalDeleted = 0;

        while (true)
        {
            var ids = await _context.JsonBins
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ExpiredAtUtc != null && x.ExpiredAtUtc <= nowUtc)
                .OrderBy(x => x.ExpiredAtUtc)
                .Select(x => x.Id)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (ids.Count == 0)
                break;

            var deleted = await _context.JsonBins
                .IgnoreQueryFilters()
                .Where(x => ids.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);

            totalDeleted += deleted;

            _logger.LogDebug(
                "JsonBinExpiredCleanupJob: batch hard-deleted {BatchCount} row(s)",
                deleted);

            if (ids.Count < BatchSize)
                break;
        }

        if (totalDeleted == 0)
        {
            _logger.LogDebug("JsonBinExpiredCleanupJob: nothing to clean");
            return;
        }

        _logger.LogInformation(
            "JsonBinExpiredCleanupJob: hard-deleted {Count} expired JsonBin row(s)",
            totalDeleted);
    }
}
