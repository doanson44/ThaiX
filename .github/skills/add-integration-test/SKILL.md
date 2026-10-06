---
name: add-integration-test
description: "Add integration tests for a Minimal API endpoint. Covers happy path, 401/403/404/400 cases, validation, and business rule enforcement. Use when adding or verifying endpoint test coverage."
argument-hint: "Endpoint path and HTTP method, e.g. 'POST /api/contacts' or 'GET /api/users/{id}'"
---

# Add Integration Test Skill

Add complete integration test coverage for a Minimal API endpoint in `ThaiX.Presentation.IntegrationTests`.

## Infrastructure Reference

- Factory: `ThaiXWebApplicationFactory` -- spins up a real SQL Server container via Testcontainers
- Base class: `IntegrationTestBase` (must inherit, must annotate `[Collection(IntegrationTestCollection.Name)]`)
- Test file location: `tests/ThaiX.Presentation.IntegrationTests/Endpoints/{Feature}EndpointsTests.cs`
- Pattern: xUnit `[Fact]` methods, AAA with `// Arrange`, `// Act`, `// Assert` comments
- Naming: `{Method}_{Scenario}_{ExpectedResult}` (e.g. `CreateContact_WithValidData_ShouldReturn200`)

### Available Helpers (IntegrationTestBase)

| Helper | Purpose |
|---|---|
| `CreateAndAuthenticateAdminAsync(email?)` | Creates admin with all permissions + sets Bearer token |
| `CreateTestUserAsync(email, password, confirmEmail)` | Creates a bare user (no permissions) |
| `CreateUserWithPermissionsAsync(email, pw, permissions[])` | Creates user with specific `Permissions.*` claims |
| `AuthenticateAsync(email, password)` | Logs in and sets `Client` Bearer token |
| `ReadResponseAsync<T>(response)` | Deserializes `ApiResponse<T>` envelope |
| `ReadResponseAsync(response)` | Deserializes non-generic `ApiResponse` envelope |
| `Client` | Shared `HttpClient` (reset auth per test where needed) |

## Procedure

### Step 1 -- Locate the Endpoint
- Read the endpoint file in `src/ThaiX.Presentation/Endpoints/`
- Identify: HTTP method, route, required permission (`Permissions.*` constant), request/response types

### Step 2 -- Identify Required Test Cases

Apply the following matrix based on endpoint type:

#### GET (list)
- [ ] Happy path: admin auth -> 200, non-empty result
- [ ] Filter/search: if query params exist, verify filtering works
- [ ] Unauthenticated (no token) -> 401
- [ ] No permission -> 403

#### GET (by id)
- [ ] Happy path: create resource, fetch by id -> 200, correct data
- [ ] Not found: random `Guid.NewGuid()` -> 404
- [ ] Unauthenticated -> 401
- [ ] No permission -> 403

#### POST (create)
- [ ] Happy path: valid request -> 200/201, `envelope.Success` is true
- [ ] Duplicate/conflict (if applicable) -> 400/409
- [ ] Validation failure: missing required fields -> 400, `envelope.Errors` non-empty
- [ ] Unauthenticated -> 401
- [ ] No permission -> 403

#### PUT/PATCH (update)
- [ ] Happy path: create resource, update it -> 200
- [ ] Not found: wrong id -> 404
- [ ] Validation failure: invalid data -> 400
- [ ] Unauthenticated -> 401
- [ ] No permission -> 403

#### DELETE
- [ ] Happy path: create resource, delete it -> 200
- [ ] Not found: already deleted or unknown id -> 404
- [ ] Unauthenticated -> 401
- [ ] No permission -> 403

### Step 3 -- Check Existing File
- Look for `tests/ThaiX.Presentation.IntegrationTests/Endpoints/{Feature}EndpointsTests.cs`
- If it exists: append new test methods inside the appropriate `#region`
- If it does not exist: create the file with class declaration, then add all test methods

### Step 4 -- Write Tests

#### Class Shell (new file only)
```csharp
using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class {Feature}EndpointsTests : IntegrationTestBase
{
    public {Feature}EndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }
}
```

#### Happy Path Pattern (POST)
```csharp
[Fact]
public async Task Create{Entity}_WithValidData_ShouldReturnOk()
{
    // Arrange
    await CreateAndAuthenticateAdminAsync();
    var request = new { /* required fields */ };

    // Act
    var response = await Client.PostAsJsonAsync("/api/{route}", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var envelope = await ReadResponseAsync<Guid>(response);
    envelope.Success.Should().BeTrue();
    envelope.Data.Should().NotBeEmpty();
}
```

#### 401 Pattern
```csharp
[Fact]
public async Task {Method}_{Entity}_WhenNotAuthenticated_ShouldReturnUnauthorized()
{
    // Arrange
    Client.DefaultRequestHeaders.Authorization = null;

    // Act
    var response = await Client.{Verb}Async("/api/{route}", /* body if needed */);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}
```

#### 403 Pattern
```csharp
[Fact]
public async Task {Method}_{Entity}_WithoutPermission_ShouldReturnForbidden()
{
    // Arrange
    var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
    await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
    await AuthenticateAsync(email, "Test@Pass123");

    // Act
    var response = await Client.{Verb}Async("/api/{route}", /* body if needed */);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

#### 400 Validation Pattern
```csharp
[Fact]
public async Task Create{Entity}_WithInvalidData_ShouldReturnBadRequest()
{
    // Arrange
    await CreateAndAuthenticateAdminAsync();
    var request = new { /* intentionally invalid/missing fields */ };

    // Act
    var response = await Client.PostAsJsonAsync("/api/{route}", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var envelope = await ReadResponseAsync<object>(response);
    envelope.Success.Should().BeFalse();
    envelope.Errors.Should().NotBeEmpty();
}
```

#### 404 Pattern
```csharp
[Fact]
public async Task Get{Entity}ById_WithUnknownId_ShouldReturnNotFound()
{
    // Arrange
    await CreateAndAuthenticateAdminAsync();
    var unknownId = Guid.NewGuid();

    // Act
    var response = await Client.GetAsync($"/api/{route}/{unknownId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### Step 5 -- Validation

Run tests to confirm they pass (or fail for expected reasons):
```bash
dotnet test tests/ThaiX.Presentation.IntegrationTests --filter "FullyQualifiedName~{Feature}EndpointsTests"
```

If a test fails unexpectedly, inspect the response body to diagnose the issue before modifying test assertions.

## Rules

- Each test must be independent: user emails use `Guid.NewGuid():N` to avoid collisions
- Never hardcode credential strings; use `"Test@Pass123"` or `"Admin@Pass123"` constants only
- Use `Permissions.*` constants, never raw strings
- Reset `Client.DefaultRequestHeaders.Authorization = null` for 401 tests
- Use `#region` blocks to group tests by endpoint route
- Do NOT use `[Theory]` unless the endpoint has a documented parameterized contract
- No shared mutable state between tests
- Do NOT auto-commit after writing tests
