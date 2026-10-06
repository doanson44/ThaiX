---
description: "Review code against ThaiX conventions. Check bugs, performance, security, architecture, and maintainability. Score X/10."
agent: "agent"
argument-hint: "Paste code to review"
---

Review the provided code against ThaiX conventions:

## Checks
1. **CRITICAL**: Architecture violations, missing `AsNoTracking()`, entity exposure, direct DbContext in wrong layer, missing permissions
2. **WARNING**: Missing `CancellationToken`, hardcoded strings, missing `[InvalidateCache]`, missing `ICacheableQuery`
3. **SUGGESTION**: Performance improvements, readability, better patterns
4. **GOOD**: Patterns done correctly

## ThaiX-Specific
- `AsNoTracking()` on queries, `Select()` projection
- `[InvalidateCache(CacheGroups.X)]` on commands
- `Permissions.*` constants (not hardcoded)
- `CacheKeys.*` / `CacheGroups.*` constants
- `ResourceKeys` for localization
- Proper aggregate methods (no public setters)
- FluentValidation (no manual validation in handlers)

Give concrete fixes with code patches. Score: X/10.
