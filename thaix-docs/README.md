# ThaiX Documentation Refresh

This directory contains the source audit and refreshed guidance for ThaiX. It is intentionally
flat: every document lives directly in `thaix-docs/` so the folder can be uploaded as a single
reference set to ChatGPT or other AI agents.

- [Audit report](AUDIT-REPORT.md)
- [Verified runtime facts](VERIFIED-RUNTIME-FACTS.md)
- [.NET 11 SDK toolchain](SDK-11-TOOLCHAIN.md)
- [Application layer rules](instruction-application-layer.md)
- [Security and authentication](instruction-security-auth.md)
- [Testing rules](instruction-testing.md)
- [Database query workflow](skill-db-query.md)
- [New module workflow](skill-new-module.md)
- [Integration test workflow](skill-add-integration-test.md)
- [Widget workflow](skill-add-widget.md)

## Maintenance policy

Application source and tests are authoritative. When documentation disagrees with implementation,
inspect the relevant source and tests before changing either. Do not assume database providers,
token lifetimes, claims, API response shapes, or helper APIs from old examples.

Audit baseline: `master` at `db9eb84c2747fef7aa8216eb4c3e8923f731b7d0`.
Reviewed: 2026-11-12.
