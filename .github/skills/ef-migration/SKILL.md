---
name: ef-migration
description: "Manage EF Core migrations: add a new migration, revert the latest applied migration from the database, or remove the latest unapplied migration file. Use when schema changes are needed or a migration must be rolled back."
argument-hint: "Action + description, e.g. 'add AddProducts', 'revert', or 'remove'"
---

# EF Core Migration Skill

Manage EF Core migrations for ThaiX. All commands target:
- **Project:** `src/ThaiX.Infrastructure`
- **Startup project:** `src/ThaiX.Presentation`
- **Migrations folder:** `src/ThaiX.Infrastructure/Persistence/Migrations/`
- **Connection string source:** .NET user-secrets for `ThaiX.Presentation` (`dotnet user-secrets list`) or environment variable `ConnectionStrings__DefaultConnection`

---

## Action A -- Add New Migration

Use when a domain entity or EF configuration has changed and a new migration is needed.

### Step 1 -- Verify Entity and Configuration Changes
- Confirm the entity (`Domain/`) and its `IEntityTypeConfiguration` (`Infrastructure/Persistence/Configurations/`) are both saved.
- Check `IApplicationDbContext` and `ApplicationDbContext` have the correct `DbSet<T>`.

### Step 2 -- Naming Convention
- PascalCase, descriptive of the schema change.
- Examples: `Add_Products_Table`, `Rename_Contact_Phone_To_PhoneNumber`, `Add_UserProfile_BirthDate`
- No spaces, no version numbers, no generic names like `Update`.

### Step 3 -- Run Add Command
```powershell
dotnet ef migrations add {MigrationName} `
  --project src/ThaiX.Infrastructure `
  --startup-project src/ThaiX.Presentation `
  --output-dir Persistence/Migrations
```

> PowerShell note: use `Get-ChildItem -Path 'src\ThaiX.Infrastructure\Persistence\Migrations' -Name | Sort-Object`
> instead of `dir /b ... | sort` to avoid PowerShell pipe parsing issues.

### Step 4 -- Review Generated Migration
Open the new `{Timestamp}_{MigrationName}.cs` and verify:
- [ ] `Up()` contains the expected DDL (CREATE TABLE, ADD COLUMN, etc.)
- [ ] `Down()` correctly reverses `Up()`
- [ ] No unintended table drops or column renames
- [ ] `ApplicationDbContextModelSnapshot.cs` has been updated

### Step 5 -- Apply to Database (optional, only when instructed)
```powershell
dotnet ef database update `
  --project src/ThaiX.Infrastructure `
  --startup-project src/ThaiX.Presentation
```

> **Never apply migrations automatically.** Only run `database update` when explicitly requested by the user.

---

## Action B -- Revert Latest Applied Migration (DB rollback)

Use when the latest migration **has already been applied** to the database and must be rolled back.

### Step 1 -- Identify the Previous Migration
List migrations to find the migration immediately before the one to revert:
```powershell
dotnet ef migrations list `
  --project src/ThaiX.Infrastructure `
  --startup-project src/ThaiX.Presentation
```
The output lists migrations in chronological order. Note the name of the **second-to-last** entry -- this is the rollback target.

### Step 2 -- Revert Database to Previous Migration
```powershell
dotnet ef database update {PreviousMigrationName} `
  --project src/ThaiX.Infrastructure `
  --startup-project src/ThaiX.Presentation
```
This runs the `Down()` method of the latest migration, undoing the schema changes in the database.

### Step 3 -- Remove the Migration File (optional)
If the migration file should also be deleted after reverting:
```powershell
dotnet ef migrations remove `
  --project src/ThaiX.Infrastructure `
  --startup-project src/ThaiX.Presentation
```
> EF Core will refuse `remove` if the migration is still applied to the database. **Always revert the DB first (Step 2) before running `remove`.**

### Step 4 -- Verify
```powershell
dotnet ef migrations list `
  --project src/ThaiX.Infrastructure `
  --startup-project src/ThaiX.Presentation
```
Confirm the reverted migration is no longer listed (if removed) or shows as `(Pending)` (if only DB was rolled back).

---

## Action C -- Remove Latest Unapplied Migration

Use when the latest migration **has NOT been applied** to the database and should be discarded (e.g., generated with wrong entity state, wrong name).

### Step 1 -- Confirm Migration is Unapplied
```powershell
dotnet ef migrations list `
  --project src/ThaiX.Infrastructure `
  --startup-project src/ThaiX.Presentation
```
The latest migration must show as `(Pending)`. If it does not show `(Pending)`, use **Action B** first.

### Step 2 -- Remove
```powershell
dotnet ef migrations remove `
  --project src/ThaiX.Infrastructure `
  --startup-project src/ThaiX.Presentation
```
This deletes:
- `Persistence/Migrations/{Timestamp}_{Name}.cs`
- `Persistence/Migrations/{Timestamp}_{Name}.Designer.cs`
- Reverts `ApplicationDbContextModelSnapshot.cs` to the previous state

### Step 3 -- Verify
Check that `Persistence/Migrations/` no longer contains the removed files and the snapshot is consistent.

---

## Common Errors

| Error | Cause | Fix |
|---|---|---|
| `Unable to create an object of type 'ApplicationDbContext'` | Startup project not set or connection string missing | Ensure `--startup-project src/ThaiX.Presentation` and `appsettings.Development.json` is present |
| `The migration '...' has already been applied` | Trying to `remove` an applied migration | Run `database update {previous}` first, then `remove` |
| `No migrations configuration type was found` | Wrong `--project` path | Verify project path points to `ThaiX.Infrastructure` |
| `Build failed` | Compilation error in Infrastructure or Presentation | Run `dotnet build ThaiX.slnx` and fix errors first |

## Rules

- **NEVER auto-apply** `database update` without explicit user instruction.
- **NEVER drop tables** unless the migration file explicitly generates a `DROP TABLE` from a proper entity deletion.
- Build the solution before adding a migration: `dotnet build ThaiX.slnx`
- Review `Down()` to ensure it safely reverts `Up()`.
- Do NOT modify auto-generated migration files manually.
- Do NOT auto-commit after migration operations.
