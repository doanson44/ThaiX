using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace ThaiX.Infrastructure.Persistence;

/// <summary>
/// Custom HistoryRepository for MariaDB and MySQL that fixes an issue in MySql.EntityFrameworkCore
/// where AcquireDatabaseLockAsync executes SELECT GET_LOCK(..., -1).
/// In MariaDB, negative timeouts are treated as invalid and return NULL (DBNull),
/// causing an InvalidCastException when casting to long.
/// Using a positive timeout (60 seconds) is supported by both MySQL and MariaDB.
/// </summary>
public class MariaDbHistoryRepository : HistoryRepository
{
    private const string LockName = "__EFMigrationsLock";
    private readonly IRelationalCommand _getLockCommand;
    private readonly IRelationalCommand _releaseLockCommand;

    public MariaDbHistoryRepository(HistoryRepositoryDependencies dependencies)
        : base(dependencies)
    {
        _getLockCommand = dependencies.RawSqlCommandBuilder
            .Build($"SELECT GET_LOCK('{LockName}', 60);");
        _releaseLockCommand = dependencies.RawSqlCommandBuilder
            .Build($"SELECT RELEASE_LOCK('{LockName}');");
    }

    public override LockReleaseBehavior LockReleaseBehavior => LockReleaseBehavior.Connection;

    protected override string ExistsSql
    {
        get
        {
            var stringMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
            var schema = TableSchema ?? Dependencies.Connection.DbConnection.Database;
            return new StringBuilder()
                .Append("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE ")
                .Append("TABLE_SCHEMA=")
                .Append(stringMapping.GenerateSqlLiteral(schema))
                .Append(" AND TABLE_NAME=")
                .Append(stringMapping.GenerateSqlLiteral(TableName))
                .Append(';')
                .ToString();
        }
    }

    protected override bool InterpretExistsResult(object? value)
        => value is not null && value is not DBNull;

    public override string GetCreateIfNotExistsScript()
    {
        var script = GetCreateScript();
        var index = script.IndexOf("CREATE TABLE", StringComparison.OrdinalIgnoreCase);
        return index >= 0
            ? script.Insert(index + "CREATE TABLE".Length, " IF NOT EXISTS")
            : script;
    }

    public override string GetBeginIfNotExistsScript(string migrationId)
    {
        var stringMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
        return new StringBuilder()
            .Append("IF NOT EXISTS(SELECT * FROM ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append(" WHERE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(" = ")
            .Append(stringMapping.GenerateSqlLiteral(migrationId))
            .AppendLine(")")
            .AppendLine("BEGIN")
            .ToString();
    }

    public override string GetBeginIfExistsScript(string migrationId)
    {
        var stringMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
        return new StringBuilder()
            .Append("IF EXISTS(SELECT * FROM ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append(" WHERE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(" = ")
            .Append(stringMapping.GenerateSqlLiteral(migrationId))
            .AppendLine(")")
            .AppendLine("BEGIN")
            .ToString();
    }

    public override string GetEndIfScript() => "END;\n";

    public override IMigrationsDatabaseLock AcquireDatabaseLock()
    {
        var lockObj = new MariaDbMigrationDatabaseLock(
            this,
            Dependencies.Connection,
            _releaseLockCommand,
            Dependencies.CommandLogger);

        var result = _getLockCommand.ExecuteScalar(CreateRelationalCommandParameters());
        if (result is null || Convert.ToInt64(result) != 1)
        {
            lockObj.Dispose();
            throw new InvalidOperationException("Could not acquire migrations database lock.");
        }

        return lockObj;
    }

    public override async Task<IMigrationsDatabaseLock> AcquireDatabaseLockAsync(CancellationToken cancellationToken = default)
    {
        var lockObj = new MariaDbMigrationDatabaseLock(
            this,
            Dependencies.Connection,
            _releaseLockCommand,
            Dependencies.CommandLogger);

        var result = await _getLockCommand.ExecuteScalarAsync(CreateRelationalCommandParameters(), cancellationToken);
        if (result is null || Convert.ToInt64(result) != 1)
        {
            await lockObj.DisposeAsync();
            throw new InvalidOperationException("Could not acquire migrations database lock.");
        }

        return lockObj;
    }

    private RelationalCommandParameterObject CreateRelationalCommandParameters()
        => new(
            Dependencies.Connection,
            null,
            null,
            Dependencies.CurrentContext.Context,
            Dependencies.CommandLogger,
            CommandSource.Migrations);

    private sealed class MariaDbMigrationDatabaseLock(
        IHistoryRepository historyRepository,
        IRelationalConnection connection,
        IRelationalCommand releaseCommand,
        IRelationalCommandDiagnosticsLogger logger) : IMigrationsDatabaseLock
    {
        private bool _disposed;

        public IHistoryRepository HistoryRepository => historyRepository;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                releaseCommand.ExecuteScalar(new RelationalCommandParameterObject(
                    connection, null, null, null, logger, CommandSource.Migrations));
            }
            catch
            {
                // Lock release is best effort; connection close will release advisory locks
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                await releaseCommand.ExecuteScalarAsync(new RelationalCommandParameterObject(
                    connection, null, null, null, logger, CommandSource.Migrations));
            }
            catch
            {
                // Lock release is best effort; connection close will release advisory locks
            }
        }
    }
}
