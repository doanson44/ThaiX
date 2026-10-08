namespace ThaiX.Infrastructure.Configuration;

public sealed class HangfireConfiguration
{
    public const string SectionName = "Hangfire";
    public bool Enabled { get; set; } = true;
    public string SchemaName { get; set; } = "hangfire";
    public int RetryAttempts { get; set; } = 2;
    public HangfireServerConfiguration Server { get; set; } = new();
    public HangfireStorageConfiguration Storage { get; set; } = new();
    public HangfireDashboardConfiguration Dashboard { get; set; } = new();
    public Dictionary<string, RecurringJobConfiguration> RecurringJobs { get; set; } = new();
}

public sealed class HangfireServerConfiguration
{
    public bool Enabled { get; set; } = true;
    public int WorkerCount { get; set; } = 5;
    public string[] Queues { get; set; } = ["default"];
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(15);
}

public sealed class HangfireStorageConfiguration
{
    public TimeSpan QueuePollInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan JobExpirationCheckInterval { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan CountersAggregateInterval { get; set; } = TimeSpan.FromMinutes(5);
    public bool PrepareSchemaIfNecessary { get; set; } = true;
    public int DashboardJobListLimit { get; set; } = 10000;
    public TimeSpan TransactionTimeout { get; set; } = TimeSpan.FromMinutes(1);
}

public sealed class HangfireDashboardConfiguration
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "/hangfire";
    public bool ReadOnly { get; set; } = false;
    public string RequiredPermission { get; set; } = "System.Admin";
    public string AppPath { get; set; } = "/";
    public int StatsPollingInterval { get; set; } = 2000;
    public bool DisplayStorageConnectionString { get; set; } = false;
}

public sealed class RecurringJobConfiguration
{
    public bool Enabled { get; set; } = true;
    public string CronExpression { get; set; } = string.Empty;
    public string Queue { get; set; } = "default";
    public string TimeZone { get; set; } = "UTC";
}
