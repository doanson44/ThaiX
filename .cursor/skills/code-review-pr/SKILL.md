---
name: code-review-pr
description: "Review code changes against ThaiX architecture, data-access, caching, security, and permission standards. Use when reviewing pull requests or staged diffs."
argument-hint: "Optionally specify branch or 'staged' for staged changes"
---

# Code Review PR

Review all changed files against ThaiX conventions.

## Procedure

### Step 1 -- Gather Changes
- Run `git diff --staged` (or `git diff main...HEAD` for PR)
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
Actionable recommendations with code patches.
