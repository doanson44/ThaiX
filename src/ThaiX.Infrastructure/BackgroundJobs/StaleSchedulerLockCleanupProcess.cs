using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Removes distributed lock rows left behind by Hangfire's own schedulers (recurring job poller, delayed job
/// poller and the per-recurring-job enqueue guard).
/// Hangfire.MySqlStorage creates lock rows without an expiration and releases them only through an explicit
/// delete, so a lock owned by a killed or hung scheduler pass stays forever. Worse, on acquisition timeout the
/// storage throws its own exception type instead of Hangfire's DistributedLockTimeoutException, so the failure
/// escapes the scheduler instead of being retried on the next pass. Both resources are only held for the
/// duration of a scheduler pass (Hangfire itself treats a lock older than one minute as timed out), so stale
/// rows are removed well after that. Locks held by running jobs are never touched.
/// </summary>
public sealed class StaleSchedulerLockCleanupProcess : IBackgroundProcess
{
    private const string LockTableName = "Hangfire_DistributedLock";

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(2);

    private readonly string _connectionString;
    private readonly ILogger<StaleSchedulerLockCleanupProcess> _logger;

    public StaleSchedulerLockCleanupProcess(
        IConfiguration configuration,
        ILogger<StaleSchedulerLockCleanupProcess> logger)
    {
        _connectionString = configuration.GetConnectionString("HangfireConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'HangfireConnection' or 'DefaultConnection' not found.");
        _logger = logger;
    }

    public void Execute(BackgroundProcessContext context)
    {
        try
        {
            RemoveStaleSchedulerLocks();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stale Hangfire scheduler lock cleanup failed");
        }

        context.Wait(CheckInterval);
    }

    /// <summary>
    /// Deletes scheduler-owned lock rows that are older than <see cref="StaleAfter"/> and returns the number of
    /// rows removed.
    /// </summary>
    public int RemoveStaleSchedulerLocks()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"DELETE FROM `{LockTableName}` " +
            "WHERE CreatedAt < @cutoff " +
            "AND (Resource = @recurringJobsLock OR Resource = @delayedJobsLock OR Resource LIKE @recurringJobLock)";
        command.Parameters.AddWithValue("@cutoff", DateTime.UtcNow.Subtract(StaleAfter));
        command.Parameters.AddWithValue("@recurringJobsLock", "recurring-jobs:lock");
        command.Parameters.AddWithValue("@delayedJobsLock", "locks:schedulepoller");
        command.Parameters.AddWithValue("@recurringJobLock", "lock:recurring-job:%");

        var removed = command.ExecuteNonQuery();
        if (removed > 0)
        {
            _logger.LogWarning(
                "Removed {Count} stale Hangfire scheduler lock(s) that were blocking job scheduling",
                removed);
        }

        return removed;
    }
}
