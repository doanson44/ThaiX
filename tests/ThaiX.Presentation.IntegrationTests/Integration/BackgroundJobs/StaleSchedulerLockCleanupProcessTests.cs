using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MySqlConnector;
using ThaiX.Infrastructure.BackgroundJobs;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Integration.BackgroundJobs;

[Collection(IntegrationTestCollection.Name)]
public sealed class StaleSchedulerLockCleanupProcessTests : IntegrationTestBase
{
    private const string LockTable = "Hangfire_DistributedLock";
    private const string JobLock = "lock:ThaiX.Infrastructure.BackgroundJobs.OutboxProcessorJob:ProcessPendingMessages";
    private const string StateLock = "job:42:state-lock";

    public StaleSchedulerLockCleanupProcessTests(ThaiXWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public void RemoveStaleSchedulerLocks_ShouldRemoveOnlyStaleSchedulerLocks()
    {
        // Arrange
        Execute($"CREATE TABLE IF NOT EXISTS `{LockTable}` (`Resource` varchar(100) NOT NULL, `CreatedAt` datetime(6) NOT NULL)");
        Execute($"DELETE FROM `{LockTable}`");

        var now = DateTime.UtcNow;
        var stale = now.AddMinutes(-30);
        Insert("recurring-jobs:lock", stale);
        Insert("locks:schedulepoller", stale);
        Insert("lock:recurring-job:ProcessOutbox", stale);
        Insert("recurring-jobs:lock", now);
        Insert(JobLock, stale);
        Insert(StateLock, stale);

        var sut = new StaleSchedulerLockCleanupProcess(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = Factory.ConnectionString
                })
                .Build(),
            NullLogger<StaleSchedulerLockCleanupProcess>.Instance);

        // Act
        var removed = sut.RemoveStaleSchedulerLocks();

        // Assert
        removed.Should().Be(3);
        var remaining = ReadLockResources();
        remaining.Should().BeEquivalentTo(["recurring-jobs:lock", JobLock, StateLock]);
        CountRows("recurring-jobs:lock").Should().Be(1, "the lock held right now must be kept");
    }

    private void Execute(string sql)
    {
        using var connection = new MySqlConnection(Factory.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private void Insert(string resource, DateTime createdAt)
    {
        using var connection = new MySqlConnection(Factory.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO `{LockTable}` (Resource, CreatedAt) VALUES (@resource, @createdAt)";
        command.Parameters.AddWithValue("@resource", resource);
        command.Parameters.AddWithValue("@createdAt", createdAt);
        command.ExecuteNonQuery();
    }

    private List<string> ReadLockResources()
    {
        using var connection = new MySqlConnection(Factory.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT Resource FROM `{LockTable}`";
        using var reader = command.ExecuteReader();

        var resources = new List<string>();
        while (reader.Read())
        {
            resources.Add(reader.GetString(0));
        }

        return resources;
    }

    private int CountRows(string resource)
    {
        using var connection = new MySqlConnection(Factory.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM `{LockTable}` WHERE Resource = @resource";
        command.Parameters.AddWithValue("@resource", resource);
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
