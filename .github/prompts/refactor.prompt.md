---
description: "Refactor code for readability, performance, or clean architecture. No behavior change. Respects ThaiX layer boundaries."
agent: "agent"
argument-hint: "Paste code and specify target: clean-architecture, performance, or readability"
---

Refactor the provided code:

**Targets** (specify one): `clean-architecture`, `performance`, `readability`

Constraints:
- No behavior change
- Respect layer boundaries (Domain -> Application -> Infrastructure -> Presentation)
- Keep business logic in Domain/Application, not endpoints
- Follow ThaiX conventions (no AutoMapper, no Repository, `Select()` projection, etc.)
- Extract helper methods only if they improve clarity
- Return refactored code only
