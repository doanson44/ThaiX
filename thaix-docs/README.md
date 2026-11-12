# ThaiX Documentation Refresh

This directory contains the source audit and refreshed guidance for ThaiX.

- [Audit report](AUDIT-REPORT.md)
- [Verified runtime facts](VERIFIED-RUNTIME-FACTS.md)
- [.NET 11 SDK toolchain](SDK-11-TOOLCHAIN.md)
- [Application layer rules](instructions/application-layer.md)
- [Security and authentication](instructions/security-auth.md)
- [Testing rules](instructions/testing.md)
- [Database query workflow](skills/db-query.md)
- [New module workflow](skills/new-module.md)
- [Integration test workflow](skills/add-integration-test.md)
- [Widget workflow](skills/add-widget.md)

## Maintenance policy

Application source and tests are authoritative. When documentation disagrees with implementation,
inspect the relevant source and tests before changing either. Do not assume database providers,
token lifetimes, claims, API response shapes, or helper APIs from old examples.

Audit baseline: `master` at `db9eb84c2747fef7aa8216eb4c3e8923f731b7d0`.
Reviewed: 2026-11-12.
