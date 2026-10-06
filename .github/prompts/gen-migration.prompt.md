---
description: "Generate an EF Core migration command and preview expected SQL changes."
agent: "agent"
argument-hint: "Describe the entity or schema changes"
---

Generate migration for the described changes:

1. **Analyze** entity changes in the codebase
2. **Generate** migration command:
   ```
   dotnet ef migrations add [Name] --project src/ThaiX.Infrastructure --startup-project src/ThaiX.Presentation
   ```
3. **Preview** expected SQL (CREATE TABLE, ALTER, INDEX, etc.)
4. **Verify**: RowVersion configured? Soft delete filter applied? Indexes appropriate?
