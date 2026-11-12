# .NET 11 SDK Toolchain

## Repository state

- Root `global.json` pins .NET SDK `11.0.100` and uses `rollForward: latestMinor`.
- All current ThaiX application and test projects target `net10.0`.
- The current project target therefore remains .NET 10; this branch does not migrate application
  runtime or framework references to .NET 11.
- The .NET 11 SDK is the build toolchain. It can target earlier .NET TFMs.
- `Directory.Packages.props` remains on the ASP.NET Core / EF Core 10 package line. Do not
  upgrade package major versions solely because the SDK changed.

## Support lifecycle

As of 2026-11-12, Microsoft classifies .NET 10 as LTS and .NET 11 as STS. The current support
policy lists November 14, 2028 as the end-of-support date for both releases. The choice here is
therefore driven by database-provider compatibility and production stability, not by claiming
that .NET 10 has a later support date than .NET 11.

## .NET 11 application migration decision

Do not retarget the solution to `net11.0` yet. The application uses Oracle's
`MySql.EntityFrameworkCore` provider through `UseMySQL()` for its primary MariaDB/MySQL database.
As of 2026-11-12, EF Core 11 is stable, but Oracle's `MySql.EntityFrameworkCore` 11.0.0 package is
still published as `11.0.0-preview`; Oracle's Connector/NET 26.10 documentation describes EF Core
11 support as preview/RC. This is a production database-provider dependency, so the current
prerelease provider is not an acceptable basis for a framework migration.

Keep the application and test projects on `net10.0`, keep the Microsoft EF Core / ASP.NET Core
packages on the 10.0.x line, and retain the .NET 10 runtime image until Oracle publishes a stable
provider version that explicitly supports EF Core 11 and .NET 11. Do not mix EF Core 11 with the
current EF Core 10 provider or force an unsupported package graph.

Re-open the migration when all of these are true:

1. Oracle publishes a stable (non-prerelease) `MySql.EntityFrameworkCore` release compatible with
   EF Core 11 and .NET 11.
2. The provider's documented Connector/NET and MariaDB/MySQL compatibility covers ThaiX's actual
   database server and integration-test setup.
3. All Microsoft ASP.NET Core, EF Core, and test-host packages can be moved to compatible stable
   11.0.x versions.
4. The .NET 11 breaking-change review is complete; unit, integration, browser, and publish tests
   pass using the .NET 11 runtime and container images.

The SDK-only update to 11.0.100 is valid and does not require changing project TFMs. See the
[.NET 11 breaking changes](https://learn.microsoft.com/dotnet/core/compatibility/11) before the
future migration.

## C# language version

The compiler selects the default language version from the target framework, not simply from the
installed SDK:

| Target framework | Default C# language version |
|---|---|
| `net10.0` | C# 14 |
| `net11.0` | C# 15 |

ThaiX projects currently use `net10.0`, so their default remains C# 14. Do not use C# 15-only
features or set `LangVersion` to `latest` as part of this SDK-only update. A framework and
language migration requires a separate compatibility review.

## Local developer setup

Install both:

1. .NET SDK 11.0.100 (selected by root `global.json`).
2. .NET 10 runtime (required to execute `net10.0` applications and tests).

The CI pipelines install the .NET 10 SDK side-by-side to supply the .NET 10 runtime. The root
`global.json` continues to select SDK 11. Verify the selected toolchain from the repository root:

```bash
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
```

Expected selected SDK: `11.0.100`. Confirm a .NET 10 runtime is listed before running tests.

## CI and container alignment

- GitHub Actions CI, production, and client visual-test workflows install SDK 11 and SDK 10
  side-by-side. SDK 11 is
  selected by `global.json`; SDK 10 provides the runtime used by `net10.0` test hosts.
- Azure Pipelines installs the same SDK/runtime combination.
- The main Presentation Dockerfile copies root `global.json`, so its build stage uses
  `mcr.microsoft.com/dotnet/sdk:11.0`. Its final stage remains
  `mcr.microsoft.com/dotnet/aspnet:10.0`, matching the application's `net10.0` target.
- The visual-test Dockerfile is deliberately isolated: it does not copy root `global.json` and
  continues to use SDK 10 so the `net10.0` Blazor client can run inside that same test container.

Do not change the ASP.NET runtime image to 11.0 unless the project target framework is migrated
and the runtime/deployment compatibility has been verified.

## Upgrade checklist

When changing the pinned SDK again, review all of the following together:

- `global.json` and the SDK installed by local setup instructions.
- GitHub Actions CI, production, and visual-test workflows.
- Azure Pipelines SDK installation.
- Main and test Dockerfiles, including intentional runtime-image separation.
- README prerequisites, `AGENTS.md`, Copilot instructions, and EF migration skill mirrors.
- Build, test, publish, and migration commands.
- The actual project TFMs, default C# versions, and centrally managed package versions.

Run build and tests with the pinned SDK and the required target runtime before claiming the
toolchain migration is verified.

## Official references

- [.NET 11 downloads and SDK 11.0.100](https://dotnet.microsoft.com/en-us/download/dotnet/11.0)
- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [C# language versioning and TFM defaults](https://learn.microsoft.com/dotnet/csharp/language-reference/language-versioning)
- [.NET SDK and Visual Studio versioning](https://learn.microsoft.com/dotnet/core/porting/versioning-sdk-msbuild-vs)
