---
name: db-query
description: "Run direct MariaDB/MySQL queries for diagnostics and reporting with safe, read-first behavior. Use when the user asks to inspect data or validate database state."
argument-hint: "SQL query or data request, e.g. 'Top 20 newest contacts'"
---

# DB Query

Connect to the project database and execute query requests safely. ThaiX runs on **MariaDB/MySQL**, not SQL Server, and has two separate connections:

- `DefaultConnection` -- EF Core + Identity data, via Oracle's `MySql.EntityFrameworkCore` (host `nas.phitv.me`)
- `HangfireConnection` -- Hangfire job storage only, via `MySqlConnector` (tables prefixed `Hangfire_`); falls back to `DefaultConnection` when unset

Ask the user which connection applies when the request is ambiguous.

## Connection Resolution Priority

1. .NET user-secrets for the `ThaiX.Presentation` project (`dotnet user-secrets list`)
2. Environment variable `ConnectionStrings__DefaultConnection` / `ConnectionStrings__HangfireConnection`
3. `src/ThaiX.Presentation/appsettings.Development.json` -> `ConnectionStrings:DefaultConnection` / `HangfireConnection`
4. `src/ThaiX.Presentation/appsettings.json` (placeholder by default -- do not use for real queries)

## Procedure

### 1. Resolve and Validate Connection
- Read connection string using the priority above
- Mask the password when reporting connection details
- Confirm target database name/host from the connection string

### 2. Execute Query
No `mysql` CLI or `sqlcmd` is installed on this machine. Use PowerShell with the `MySqlConnector` assembly already restored in the NuGet cache:

1. Resolve the cache path once: `dotnet nuget locals global-packages --list`
2. Match the version to `Directory.Packages.props` (`MySqlConnector`), then: `Add-Type -Path "<global-packages>\mysqlconnector\<version>\lib\net8.0\MySqlConnector.dll"`
3. Query via `[MySqlConnector.MySqlConnection]::new($connStr)`, `.Open()`, `MySqlCommand`, `ExecuteReader()`
4. Avoid `dotnet-script` (.csx) for this -- top-level `using var` declarations are unreliable in Roslyn scripting; if scripting is used anyway, dispose explicitly with `try/finally`

Default behavior is read-only SQL (`SELECT` only).

### 3. Return Result
- Return SQL executed
- Return row count
- Return a compact preview (first N rows)
- For large datasets, use `LIMIT`, pagination, or date filters

## Safety Rules

- Do not execute mutation statements unless user explicitly requests:
- `INSERT`, `UPDATE`, `DELETE`, `REPLACE`, `TRUNCATE`, `DROP`, `ALTER`, `CREATE`
- Before any `DROP`/`TRUNCATE`/`DELETE`, list the exact tables/rows affected and get confirmation
- Avoid long-running full scans when a scoped query is possible
- Never print full secrets (password) in output
- Columns use `utf8mb4` -- verify console encoding (`UNICODE(SUBSTRING(...))`) when displaying Vietnamese text instead of trusting the console display
