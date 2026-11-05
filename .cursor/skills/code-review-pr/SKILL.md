---
name: code-review-pr
description: "Review code changes against ThaiX conventions. Check architecture, data access, caching, security, and permissions for all changed files. Score X/10. Use when reviewing a PR or staged changes."
argument-hint: "Optionally specify branch or 'staged' for staged changes"
---

# Code Review PR

Review all changed files against ThaiX conventions.

## Procedure

### Step 1 -- Gather Changes

- Determine the PR base branch from the actual repository configuration; ThaiX currently targets `master`.
- Run `git diff --staged` for staged changes. For a PR, compare against `master` (for example, `git diff origin/master...HEAD`) after confirming the base branch exists.
- List all changed files

### Step 2 -- Review Each File
For each changed file, check:
- Architecture layer correctness
- `AsNoTracking()` on queries
- `Select()` projection (no entity exposure)
- `[InvalidateCache(CacheGroups.X)]` on commands
- `ICacheableQuery` on queries
- `Permissions.*` constants (not hardcoded strings)
- `CancellationToken` propagated
- No business logic in endpoints
- Proper error handling
- `ResourceKeys` for localization (no raw strings)

### Step 3 -- Report
Format findings as:
- **CRITICAL**: Must fix before merge
- **WARNING**: Should fix
- **SUGGESTION**: Nice to have
- **GOOD**: Patterns done correctly

Overall score: X/10
Actionable recommendations with file/line references and minimal patches. Report only findings supported by the diff and repository source.
