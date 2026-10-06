---
name: remove-page
description: "Remove a Blazor page and all related artifacts across all layers. Also use this when the same page functionality is merged into another page. Invoke when user wants to delete a page or merge duplicate pages."
---

# Remove Page

Remove a Blazor page and all related code across all layers (Client, Application, Presentation, Infrastructure).

## Procedure

### Step 1 -- Identify the Page

Locate the `.razor` page to be removed in `src/ThaiX.Client/Pages/`.

### Step 2 -- Trace All References

Run these searches in parallel to find every artifact tied to the page. Use class names, route segments, service names, and namespace paths as search terms:

```bash
# Page route path (e.g. "slack-notify")
# Page class name (e.g. "SlackNotify")
# Client DTO/contract class names (e.g. "SendSlackNotificationRequest")
# Client service interface class name (e.g. "ISlackService")
# CQRS command class name (e.g. "SendSlackNotificationCommand")
# API endpoint route (e.g. "/api/slack/notify")
```

For each search term, find every file that references it.

### Step 3 -- Cleanup Checklist

Work through each layer systematically. Only remove items that are used **exclusively** by the page being deleted.

#### 3a. Client UI

| Check | Where |
|-------|-------|
| Page file | `src/ThaiX.Client/Pages/**/<Page>.razor` |
| Route constant | `src/ThaiX.Client/Constants/RouteSegments.cs` |
| Menu item | `src/ThaiX.Client/Layout/NavMenu.razor` |
| Breadcrumb mapping | `src/ThaiX.Client/Layout/MainLayout.razor` |

#### 3b. Client Services & Models

| Check | Where |
|-------|-------|
| Client DTOs/contracts | `src/ThaiX.Client/Models/**/` |
| Service interface | `src/ThaiX.Client/Services/**/I*Service.cs` (remove only the method, not the whole interface unless all methods belong to this page) |
| Service implementation | `src/ThaiX.Client/Services/**/*Service.cs` (remove only the method) |
| DI registration | `src/ThaiX.Client/Program.cs` (remove only if removing the entire service) |

#### 3c. Application Layer (CQRS)

| Check | Where |
|-------|-------|
| Command/Query + Handler + Validator | `src/ThaiX.Application/Features/**/` |
| DTOs/models used only by this feature | `src/ThaiX.Application/Common/Models/` or within feature folder |

#### 3d. Presentation Layer (API)

| Check | Where |
|-------|-------|
| Endpoint mapping | `src/ThaiX.Presentation/Endpoints/` |
| Remove only the specific `MapPost`/`MapGet` block, not the entire endpoint group if other endpoints remain |

#### 3e. Infrastructure

| Check | Where |
|-------|-------|
| DI registration of removed services | `src/ThaiX.Infrastructure/DependencyInjection.cs` |

#### 3f. Localization

| Check | Where |
|-------|-------|
| ResourceKeys constant | `src/ThaiX.Client/Constants/ResourceKeys.cs` |
| English `.resx` | `src/ThaiX.Client/Resources/Localization.SharedResource.resx` |
| Vietnamese `.resx` | `src/ThaiX.Client/Resources/Localization.SharedResource.vi.resx` |

#### 3g. Tests

| Check | Where |
|-------|-------|
| Integration/unit tests for removed endpoints/features | `tests/` |

### Step 4 -- Verify No Leftover References

After all deletions, run a final search for the removed class names, route strings, and resource keys across the entire repo. Confirm zero matches remain.

### Step 5 -- Build

```bash
dotnet build ThaiX.slnx -c Debug --no-restore
```

Fix any compilation errors from missed references.

## Important Rules

- **Do NOT remove infrastructure services used by background jobs.** If `ISlackMessageDispatcher` or `ISlackNotificationService` is used by Hangfire jobs (`src/ThaiX.Infrastructure/BackgroundJobs/`), keep them.
- When removing a method from a service interface/class that has other methods still in use, only delete the specific method — not the entire file.
- When removing a single endpoint from an endpoint group file, only delete that `Map*()` block — not the entire file.
- Never delete `.resx` keys that are still referenced by other components. Verify with grep across the entire `src/` directory.
