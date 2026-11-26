---
name: db-query
description: "Query ThaiX application or Hangfire databases for diagnostics/reporting when requested. Read-only by default."
argument-hint: "SQL query or data request, e.g. 'Top 20 newest contacts'"
---

# DB Query

Connect only to the database explicitly in scope. ThaiX uses two separate database providers:

- `DefaultConnection` -- application and Identity data. Oracle `MySql.EntityFrameworkCore`
  via `UseMySQL()`; the configured application database is MariaDB/MySQL.
- `HangfireConnection` -- Hangfire job storage. `Hangfire.SqlServer` via
  `UseSqlServerStorage()`; this is SQL Server, not MySQL.

There is no fallback from `HangfireConnection` to `DefaultConnection`. When Hangfire is enabled,
a missing `HangfireConnection` causes startup/configuration failure. Ask which database applies
when the request is ambiguous.

## Connection Resolution Priority

1. .NET user-secrets for the `ThaiX.Presentation` project.
2. Environment variables:
   `ConnectionStrings__DefaultConnection` or `ConnectionStrings__HangfireConnection`.
3. `src/ThaiX.Presentation/appsettings.Development.json`.
4. `src/ThaiX.Presentation/appsettings.json` only when the value is intentional and not a placeholder.

Inspect the current configuration before connecting. Mask passwords and other credentials in all
output. Never copy connection strings into logs, commits, or generated reports.

## Procedure

### 1. Resolve and Validate Connection

- Confirm whether the target is the application database or Hangfire storage.
- Confirm the provider and database name from the selected connection.
- Use a client/driver compatible with that provider. Do not use MySqlConnector for Hangfire SQL
  Server storage or assume that a MySQL client can query both databases.

### 2. Execute Query

- Default behavior is read-only SQL (`SELECT` only).
- Use provider-appropriate SQL syntax: `LIMIT` for MariaDB/MySQL and `TOP` for SQL Server.
- Prefer narrow predicates, date ranges, and indexed columns over full scans.

### 3. Return Result

- Return the SQL executed, row count, and a compact preview (first N rows).
- Mask secrets and omit unnecessary PII.
- State which connection/provider was queried.

## Safety Rules

- Do not execute `INSERT`, `UPDATE`, `DELETE`, `MERGE`, `REPLACE`, `TRUNCATE`,
  `DROP`, `ALTER`, or `CREATE` unless explicitly requested.
- Before destructive statements, identify the exact affected tables/rows and obtain confirmation.
- Avoid long-running full scans when a scoped query is possible.
- Do not infer the provider of one connection from the other.
