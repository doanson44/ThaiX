using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Ensures the Hangfire_* tables have an index on their ExpireAt column.
/// Hangfire.MySqlStorage's install script does not create these indexes, so the ExpirationManager
/// cleanup DELETE performs a full table scan and can block on row locks until the client command
/// timeout fires. Runs once at startup against the Hangfire connection and is idempotent, so a
/// fresh database gets the indexes automatically.
/// </summary>
public sealed class HangfireStorageIndexInitializer
{
    private static readonly string[] TablesWithExpireAt =
    [
        "Hangfire_AggregatedCounter",
        "Hangfire_Counter",
        "Hangfire_Hash",
        "Hangfire_Job",
        "Hangfire_List",
        "Hangfire_Set"
    ];

    private readonly string _connectionString;
    private readonly ILogger<HangfireStorageIndexInitializer> _logger;

    public HangfireStorageIndexInitializer(
        IConfiguration configuration,
        ILogger<HangfireStorageIndexInitializer> logger)
    {
        _connectionString = configuration.GetConnectionString("HangfireConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'HangfireConnection' or 'DefaultConnection' not found.");
        _logger = logger;
    }

    /// <summary>
    /// Creates the missing ExpireAt indexes on the Hangfire_* tables. Never throws; a locked table
    /// (for example one held by a stale transaction) is logged and skipped so startup is not blocked.
    /// </summary>
    public void EnsureExpireAtIndexes()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            if (!TableExists(connection, "Hangfire_Job"))
            {
                _logger.LogWarning("Hangfire schema not created yet; skipping ExpireAt index creation");
                return;
            }

            foreach (var table in TablesWithExpireAt)
            {
                EnsureIndex(connection, table);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ensure Hangfire ExpireAt indexes");
        }
    }

    private static bool TableExists(MySqlConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM information_schema.TABLES " +
            "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table";
        command.Parameters.AddWithValue("@table", tableName);
        command.CommandTimeout = 30;
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static bool ExpireAtIndexExists(MySqlConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM information_schema.STATISTICS " +
            "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table AND COLUMN_NAME = 'ExpireAt'";
        command.Parameters.AddWithValue("@table", tableName);
        command.CommandTimeout = 30;
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private void EnsureIndex(MySqlConnection connection, string table)
    {
        try
        {
            if (ExpireAtIndexExists(connection, table))
            {
                return;
            }

            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE INDEX IX_{table}_ExpireAt ON `{table}` (ExpireAt)";
            // Wait out innodb_lock_wait_timeout (50s) so a blocked DDL surfaces the server's lock-wait
            // error instead of a misleading client-side "Command Timeout", while a large-table build can finish.
            command.CommandTimeout = 90;
            command.ExecuteNonQuery();
            _logger.LogInformation("Created ExpireAt index on Hangfire table {Table}", table);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create ExpireAt index on Hangfire table {Table}", table);
        }
    }
}
