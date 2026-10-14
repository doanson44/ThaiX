using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MySqlConnector;
using ThaiX.Infrastructure.BackgroundJobs;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Integration.BackgroundJobs;

[Collection(IntegrationTestCollection.Name)]
public sealed class HangfireStorageIndexInitializerTests : IntegrationTestBase
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

    public HangfireStorageIndexInitializerTests(ThaiXWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task EnsureExpireAtIndexes_WithFreshHangfireTables_ShouldCreateIndexesIdempotently()
    {
        // Arrange - create minimal Hangfire_* tables on the Testcontainers MariaDB
        using var connection = new MySqlConnection(Factory.ConnectionString);
        await connection.OpenAsync();

        foreach (var table in TablesWithExpireAt)
        {
            using var create = connection.CreateCommand();
            create.CommandText = $"CREATE TABLE `{table}` (Id BIGINT NOT NULL PRIMARY KEY, ExpireAt DATETIME NULL)";
            await create.ExecuteNonQueryAsync();
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HangfireConnection"] = Factory.ConnectionString
            })
            .Build();

        var initializer = new HangfireStorageIndexInitializer(
            configuration,
            NullLogger<HangfireStorageIndexInitializer>.Instance);

        // Act - run twice to prove idempotency
        initializer.EnsureExpireAtIndexes();
        initializer.EnsureExpireAtIndexes();

        // Assert
        using var verify = connection.CreateCommand();
        verify.CommandText =
            "SELECT COUNT(*) FROM information_schema.STATISTICS " +
            "WHERE TABLE_SCHEMA = DATABASE() AND COLUMN_NAME = 'ExpireAt' " +
            "AND TABLE_NAME IN ('Hangfire_AggregatedCounter','Hangfire_Counter','Hangfire_Hash','Hangfire_Job','Hangfire_List','Hangfire_Set')";
        var count = Convert.ToInt64(await verify.ExecuteScalarAsync());

        count.Should().Be(6);
    }
}
