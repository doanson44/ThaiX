---
description: "Generate xUnit tests with Moq and FluentAssertions. Covers happy path, validation failure, not found, and edge cases."
agent: "agent"
argument-hint: "Paste class or handler to test"
---

Generate tests for the provided code:

## Requirements
- xUnit + Moq + FluentAssertions
- `Mock<IApplicationDbContext>` (no in-memory database)
- AAA pattern with `// Arrange`, `// Act`, `// Assert` comments
- Naming: `MethodName_Condition_ExpectedResult`

## Coverage
1. Happy path (valid input, correct result)
2. Validation failure (invalid input, expected error)
3. Not found (missing entity, expected exception/null)
4. Edge cases (empty collections, null values, boundary conditions)
5. Business rules (domain invariants enforced)

## Constraints
- No shared mutable state between tests
- Keep tests deterministic and isolated
- Follow existing test patterns in `tests/ThaiX.Application.UnitTests/`
