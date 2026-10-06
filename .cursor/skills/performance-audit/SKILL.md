---
name: performance-audit
description: "Audit features for performance bottlenecks including N+1 queries, index gaps, client-side filtering, over-fetching, caching misses, and pagination issues. Use when optimizing slow paths."
argument-hint: "Feature name or area to audit, e.g. 'Contact list' or 'Invoice detail'"
---

# Performance Audit

Audit a feature for data access and caching performance.

## Procedure

### Step 1 -- Identify Hot Paths
- Read handlers for the specified feature
- Check for N+1 queries, missing indexes, unnecessary Includes
- Map the data flow: endpoint -> handler -> DB queries

### Step 2 -- Data Access Review
- Verify `AsNoTracking()` usage on all read queries
- Check for client-side filtering (`AsEnumerable()`, `ToList()` before `Where()`)
- Verify projection (no over-fetching -- `Select()` with only needed fields)
- Check pagination (server-side `ToPagedListAsync()`, not manual Skip/Take)
- Verify `Expression<Func<T, bool>>` (not `Func<T, bool>`)

### Step 3 -- Caching Review
- Check `ICacheableQuery` implementation on queries
- Verify cache key determinism (uses `CacheKeys.*` constants)
- Check invalidation coverage (`[InvalidateCache]` on all related commands)
- Verify `IsVersionedList` set for list queries

### Step 4 -- Report
Findings with severity:
- **High**: Causes visible latency or scaling issues
- **Medium**: Suboptimal but functional
- **Low**: Minor improvement opportunity

Include concrete fix suggestions with code patches.
