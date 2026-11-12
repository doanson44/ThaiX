# Add Integration Test

Add endpoint integration tests in `tests/ThaiX.Presentation.IntegrationTests/`.

## Current fixture

- Factory: `ThaiXWebApplicationFactory`, using MariaDB 11.4 via `Testcontainers.MariaDb`.
- Base: `IntegrationTestBase`; use `[Collection(IntegrationTestCollection.Name)]`.
- Follow existing helpers and test classes before adding new infrastructure.
- The fixture uses `UseMySQL()` and `MariaDbHistoryRepository`; do not substitute SQL Server unless
  runtime provider and test fixture are deliberately migrated together.

## Procedure

1. Read the endpoint and identify method, route, permission, request/response envelope, and behavior.
2. Inspect existing tests and shared helpers before creating a new class.
3. Cover applicable happy path, 401, 403, 400, 404, conflict, and domain-invariant cases.
4. Use unique test identities and deterministic data.
5. Use `Permissions.*` constants, not raw permission strings.
6. Follow the repository naming convention `{Method}_{Scenario}_{ExpectedResult}` and AAA comments.
7. Run the targeted test filter and report the actual command/result.

## Important

- Reset `Client.DefaultRequestHeaders.Authorization` for unauthenticated tests.
- Do not assume every endpoint needs every status-code test; test only applicable contract behavior.
- Do not share mutable state across tests.
- Do not commit unless the user requests a commit/PR workflow.
