# ThaiX

Personal finance and market intelligence platform built with Clean Architecture, DDD and CQRS on .NET 10, with a Blazor WebAssembly front end.

ThaiX aggregates market data (crypto, stocks, bank rates), tracks portfolios and price alerts, and automates notifications (Slack/Telegram/Email), all behind a permission-based Minimal API and an admin-style Blazor UI.

[![CI/CD](https://github.com/doanson44/ThaiX/actions/workflows/ci.yml/badge.svg)](https://github.com/doanson44/ThaiX/actions/workflows/ci.yml)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4)](global.json)

## Features

- **Market data dashboard**: crypto (MEXC, Binance, Bybit, CoinGecko), stocks (TCBS Top 10, VNDirect, Dragon Capital, ChainBroker, CafeF, VnExpress, 24hMoney, Yahoo Finance), bank interest rates (Sacombank and others).
- **Portfolio management**: positions, valuation, performance tracking, market scanner, price alerts with automated checks.
- **CRM**: contacts and notes.
- **Expense tracker** and **lottery** (Ket Qua Dien Toan) modules.
- **Notifications**: scheduled and on-demand dispatch via Slack, Telegram, and email, backed by Hangfire + Outbox pattern.
- **Security**: ASP.NET Core Identity, JWT auth, API client credentials, claim-based permissions.
- **Master data**: countries, cities, districts, banks.
- **Admin tooling**: users, Slack command testing, AI tester.
- **Localization**: English (default) and Vietnamese, resource-driven at the boundary layers.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, C# 14, ASP.NET Core Minimal API |
| Frontend | Blazor WebAssembly, Radzen Blazor, Bootstrap 5.3 (dark mode) |
| Data | EF Core 10, SQL Server / MariaDB |
| CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| Background jobs | Hangfire (Outbox processing, scheduled checks) |
| Logging | Serilog (structured logging) |
| Auth | ASP.NET Core Identity + JWT |
| Packages | Centralized via `Directory.Packages.props` |

## Architecture

Clean Architecture + Domain-Driven Design + CQRS, with strict one-way dependencies:

```
ThaiX.Domain         -> no dependencies
ThaiX.Application    -> depends on Domain
ThaiX.Infrastructure -> depends on Application + Domain
ThaiX.Presentation   -> depends on Application + Infrastructure
ThaiX.Client         -> Blazor WASM, consumes the Presentation API
```

- **Domain**: aggregates, entities, value objects, domain events. No outward dependencies.
- **Application**: commands, queries, handlers, validators, DTOs (manual LINQ projection, no AutoMapper).
- **Infrastructure**: EF Core configurations/migrations, external API providers, Hangfire jobs, caching, identity.
- **Presentation**: thin Minimal API endpoints, permission-enforced, Swagger-documented.
- **Client**: Blazor WebAssembly admin UI (Radzen + Bootstrap), organized by menu group (Account, Administration, CRM, Market Data, Market Research, Master Data, Notification, Portfolio, Security).

## Project Structure

```
src/
  ThaiX.Domain/          Aggregates, value objects, domain events
  ThaiX.Application/     CQRS commands/queries/handlers/validators
  ThaiX.Infrastructure/  EF Core, Hangfire, external API providers, identity
  ThaiX.Presentation/    Minimal API host
  ThaiX.Client/          Blazor WebAssembly front end
tests/
  ThaiX.Application.UnitTests/
  ThaiX.Presentation.IntegrationTests/
  ThaiX.Client.VisualTests/
docs/                    Project/product documentation
deploy/, cloudflare/      Deployment and edge configuration
```

## Getting Started

### Prerequisites

- .NET 10 SDK (see [global.json](global.json))
- SQL Server or MariaDB (see [docker-compose.yml](docker-compose.yml) for a local MariaDB container)
- Node-free: the Blazor client uses LibMan for client-side libraries (`dotnet tool restore` + `dotnet libman restore`)

### Run with Docker Compose

```bash
docker compose up -d
```

This starts the ThaiX API (`http://localhost:5005`) and a MariaDB database.

### Run locally

```bash
dotnet restore ThaiX.slnx
dotnet build ThaiX.slnx -c Debug --no-restore
cd src/ThaiX.Presentation
dotnet watch run --non-interactive
```

The Presentation host serves both the API and the Blazor WebAssembly client.

### Run tests

```bash
dotnet test ThaiX.slnx --no-restore
```

## CI/CD

GitHub Actions workflow ([.github/workflows/ci.yml](.github/workflows/ci.yml)) runs on push/PR to `main`, `master`, `develop`, and `release/*`:

1. Restore, build, run unit + integration tests, enforce a minimum code coverage threshold.
2. Publish a self-contained `win-x86` package of `ThaiX.Presentation` as a build artifact.

An equivalent Azure DevOps pipeline is available at [azure-pipelines.yml](azure-pipelines.yml).

## Contributing

See [AGENTS.md](AGENTS.md) for repository conventions (architecture rules, coding style, encoding policy, migrations, and commit policy) used by both humans and AI coding agents.

## License

Licensed under the [Apache License 2.0](LICENSE).