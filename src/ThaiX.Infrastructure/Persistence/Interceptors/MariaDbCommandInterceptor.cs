using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ThaiX.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Intercepts EF Core DbCommands to fix MariaDB/MySQL SQL dialect issues.
/// Specifically, MySql.EntityFrameworkCore generates invalid syntax for ExecuteDelete:
/// "DELETE FROM table AS t WHERE ..." which fails in MariaDB/MySQL with a syntax error.
/// MariaDB/MySQL requires "DELETE t FROM table AS t WHERE ...".
/// </summary>
public sealed partial class MariaDbCommandInterceptor : DbCommandInterceptor
{
    [GeneratedRegex(@"^\s*DELETE\s+FROM\s+([`\w\.]+)\s+AS\s+([`\w]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex DeleteFromAliasRegex();

    [GeneratedRegex(@"\bESCAPE\s+'\\'", RegexOptions.IgnoreCase)]
    private static partial Regex EscapeBackslashRegex();

    private static void FixCommand(DbCommand command)
    {
        if (string.IsNullOrEmpty(command.CommandText))
        {
            return;
        }

        if (command.CommandText.Contains("ESCAPE", StringComparison.OrdinalIgnoreCase))
        {
            command.CommandText = EscapeBackslashRegex().Replace(command.CommandText, "ESCAPE '\\\\'");
        }

        if (command.CommandText.TrimStart().StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
        {
            command.CommandText = DeleteFromAliasRegex().Replace(command.CommandText, "DELETE $2 FROM $1 AS $2");
        }
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        FixCommand(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        FixCommand(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        FixCommand(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        FixCommand(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }
}
