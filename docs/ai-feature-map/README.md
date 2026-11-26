# ThaiX AI Feature Map

Maintained navigation pack for AI agents and engineering tools working in ThaiX.

## Read order
1. PROJECT-OVERVIEW.md - product, architecture, runtime and infrastructure.
2. FEATURE-CATALOG.md - business capabilities grouped by domain.
3. FEATURE-TRACEABILITY.md - Client, API, Application, Domain and Infrastructure paths.
4. AI-AGENT-GUIDE.md - source discovery, task routing, constraints and verification.
5. SOURCE-INDEX.md - canonical entry points, endpoint registrations and tests.

## Evidence labels
- Verified: confirmed in current source or maintained repository documentation.
- Structural: folder, page or class presence is confirmed; behavior still requires inspection.
- Needs tracing: likely relationship is known, but the complete call chain or invariant is not verified.

Do not convert a structural observation into a business rule. Source code and tests remain authoritative.

## Maintenance
Update this map when features, routes, permissions, aggregates, providers, jobs or cross-feature dependencies change. Keep non-resource documentation ASCII-only as required by AGENTS.md.

Repository: https://github.com/doanson44/ThaiX
Default integration branch: master
SDK: .NET 11.0.100
Target framework: net11.0
Language: C# 15
Source projects: 5
Test projects: 4
