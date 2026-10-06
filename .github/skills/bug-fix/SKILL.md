---
name: bug-fix
description: "Diagnose and fix a bug with minimal changes. Reproduce, identify root cause, implement fix, and verify with tests. Use when debugging errors, exceptions, or unexpected behavior."
argument-hint: "Describe the bug: error message, stack trace, steps to reproduce"
---

# Bug Fix Flow

Systematic bug diagnosis and minimal fix.

## Procedure

### Step 1 -- Reproduce
- Gather: error message, stack trace, steps to reproduce
- Identify the affected layer (Domain/Application/Infrastructure/Presentation/Client)

### Step 2 -- Diagnose
- Read relevant files
- Identify root cause
- Explain the issue clearly

### Step 3 -- Fix
- Implement minimal fix
- Preserve existing behavior for unrelated code
- No unrelated refactoring
- Run `dotnet build ThaiX.slnx` to verify compilation

### Step 4 -- Verify
- Run `dotnet test tests/ThaiX.Application.UnitTests`
- If tests fail, iterate on the fix
- Report status -- DO NOT auto-commit
