# DB Query

Use this workflow for explicitly requested database diagnostics. Read-only by default.

## Current connection/provider facts

- `DefaultConnection`: application/Identity database. `ApplicationDbContext` uses Oracle's
  `MySql.EntityFrameworkCore` provider via `UseMySQL()` and `MariaDbHistoryRepository`.
- `HangfireConnection`: Hangfire storage currently uses SQL Server via
  `UseSqlServerStorage()` in `src/ThaiX.Infrastructure/DependencyInjection.cs`.
- When Hangfire is enabled, a missing `HangfireConnection` causes startup/configuration failure;
  do not assume a fallback to `DefaultConnection`.

Always confirm which connection is in scope and inspect current configuration before connecting.
Never infer the provider of one connection from another.

## Connection resolution

1. .NET user-secrets for `ThaiX.Presentation`, when configured.
2. Relevant environment variable:
   `ConnectionStrings__DefaultConnection` or `ConnectionStrings__HangfireConnection`.
3. `src/ThaiX.Presentation/appsettings.Development.json`.
4. `appsettings.json` only when its value is intentional and not a placeholder.

Mask credentials in all output.

## Query safety

- Read-only `SELECT` by default.
- Use a client and SQL dialect matching the verified provider.
- Use `LIMIT` for MariaDB and `TOP` for SQL Server when bounding result sets.
- Prefer narrow predicates, date ranges, and indexed columns over full scans.
- Never execute `INSERT`, `UPDATE`, `DELETE`, `MERGE`, `REPLACE`, `TRUNCATE`, `DROP`,
  `ALTER`, or `CREATE` unless explicitly requested. For destructive statements, show exact
  affected rows/tables and obtain confirmation.
- Report the query, row count, and a compact preview without exposing secrets or unnecessary PII.
