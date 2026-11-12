# .NET 11 SDK and Runtime

## Repository state

- Root `global.json` pins .NET SDK `11.0.100` with `rollForward: latestMinor`.
- All application and test projects target `net11.0`.
- The default language version is C# 15 for `net11.0`.
- Microsoft ASP.NET Core and EF Core packages use the `11.0.0` release line.
- Oracle `MySql.EntityFrameworkCore` is pinned to `11.0.0-preview` because no stable 11.x provider was available at migration time.

## Database provider compatibility warning

The application registers Oracle's `MySql.EntityFrameworkCore` provider through `UseMySQL()` for its primary MariaDB/MySQL database. The solution now targets EF Core 11 while using Oracle's `11.0.0-preview` provider package.

This is an intentional prerelease dependency selected to complete the .NET 11 migration. It is not evidence that production compatibility is established. Before deploying:

1. Run the full MariaDB 11.4 integration suite.
2. Verify EF migrations, generated SQL, transaction behavior, concurrency, and provider-specific query translation against the real database engine.
3. Verify the provider's support for the exact MariaDB/MySQL server version used in each environment.
4. Replace the preview package with Oracle's stable EF Core 11 provider when available, then rerun the full suite.

Do not apply database migrations as part of this framework upgrade. No migration files or database schema were changed.

## Package alignment

- Microsoft EF Core, ASP.NET Core, and Microsoft.Extensions packages are aligned to `11.0.0`.
- `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.InMemory`, and `Microsoft.AspNetCore.Mvc.Testing` are also aligned to `11.0.0`.
- The Oracle provider is `11.0.0-preview`.
- Non-framework dependencies retain their existing centrally managed versions unless independently required by restore/build compatibility.

## SDK, CI, and containers

- GitHub Actions and Azure Pipelines use the .NET 11 SDK.
- The main Presentation Dockerfile builds with `mcr.microsoft.com/dotnet/sdk:11.0` and runs with `mcr.microsoft.com/dotnet/aspnet:11.0`.
- The visual-test Dockerfile uses the .NET 11 SDK/runtime and targets `net11.0`.
- The SDK installation includes the matching .NET 11 runtime; no side-by-side .NET 10 runtime is required for these projects.

## Local verification

From the repository root:

```bash
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
dotnet restore ThaiX.slnx
dotnet build ThaiX.slnx -c Release --no-restore
dotnet test ThaiX.slnx --no-restore
```

Expected selected SDK: `11.0.100`. Run the MariaDB-backed integration tests with Docker available.

## Breaking changes

Review the [.NET 11 breaking changes](https://learn.microsoft.com/dotnet/core/compatibility/11) and [C# 15 language version](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15) before adopting new runtime or language behavior.
