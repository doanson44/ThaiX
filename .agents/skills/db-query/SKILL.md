---
name: db-query
description: "Query SQL Server data directly for diagnostics/reporting when requested. Prioritize connection from appsettings.Development.json, then fallback sources. Read-only by default."
argument-hint: "SQL query or data request, e.g. 'Top 20 newest contacts'"
---

# DB Query

Connect to the project database and execute query requests safely.

## Connection Resolution Priority

1. .NET user-secrets for the `ThaiX.Presentation` project (`dotnet user-secrets list`)
2. Environment variable `ConnectionStrings__DefaultConnection`
3. `src/ThaiX.Presentation/appsettings.json` -> `ConnectionStrings:DefaultConnection` (placeholder by default)

## Procedure

### 1. Resolve and Validate Connection
- Read connection string using the priority above
- Mask secrets when reporting connection details
- Confirm target database name from the connection string

### 2. Execute Query
- Prefer `sqlcmd` when available
- Fallback to `Invoke-Sqlcmd` when available
- Default behavior is read-only SQL (`SELECT` only)

### 3. Return Result
- Return SQL executed
- Return row count
- Return a compact preview (first N rows)
- For large datasets, use `TOP`, pagination, or date filters

## Safety Rules

- Do not execute mutation statements unless user explicitly requests:
- `INSERT`, `UPDATE`, `DELETE`, `MERGE`, `TRUNCATE`, `DROP`, `ALTER`, `CREATE`
- Avoid long-running full scans when a scoped query is possible
- Never print full secrets in output
