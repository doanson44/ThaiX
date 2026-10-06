---
description: "Run a sprint-start health check: build status, test results, known issues, and architecture review."
agent: "agent"
---

Perform sprint-start health check:

## Step 1 -- Build Health
- Run `dotnet build ThaiX.slnx` and check for warnings
- Run `dotnet test tests/ThaiX.Application.UnitTests` and check for failures

## Step 2 -- Architecture Review
- Check `knowledge-docs/11-claude-corrections-log.md` for recurring issues
- Suggest instruction improvements if patterns repeat (5+ entries)

## Step 3 -- Summary
- Build status (pass/fail/warnings)
- Test results (pass/fail count)
- Known issues and priority items
