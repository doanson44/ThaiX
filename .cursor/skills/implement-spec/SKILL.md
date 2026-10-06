---
name: implement-spec
description: "Writes an implementation spec before coding and pauses for review. Invoke when user requests a feature, change, or refactor that should be reviewed before implementation."
argument-hint: "Feature request, change request, or problem statement"
---

# Implement Spec First

Use this skill when the user asks for a new feature, enhancement, refactor, or non-trivial change and wants the implementation plan reviewed before coding.

## Goal

Produce a concrete implementation specification first, save it as a Markdown file under `docs/implement-plans/`, derive an actionable implementation todo list from that spec, then stop and wait for review. Do not start implementation until the user approves or revises the spec.

## Mandatory Workflow

### Phase 1 -- Understand the Request
- Read the relevant code, rules, and surrounding architecture.
- Clarify the feature goal, constraints, assumptions, and affected areas.
- Ask focused questions only when the ambiguity materially changes the implementation plan.

### Phase 2 -- Write the Implement Spec
- Produce a reviewable implementation spec instead of code changes.
- Keep the spec specific to the current codebase. Do not write generic advice.
- Create the directory `docs/implement-plans/` if it does not already exist.
- Save the spec to a Markdown file in `docs/implement-plans/`.
- Break the planned implementation into an ordered todo list of small, execution-ready tasks.
- Use a clear, kebab-case file name when possible, for example:
  - `docs/implement-plans/asset-import.md`
  - `docs/implement-plans/portfolio-alert-refactor.md`
- If the user provides a source document or feature name, derive the file name from it unless the user specifies a different path.
- Preferred file naming convention:
  - convert to lowercase
  - replace spaces and underscores with hyphens
  - remove characters that are not letters, numbers, or hyphens
  - keep meaningful prefixes such as `uc4`
  - examples:
    - `UC4_ASSET_IMPORT.md` -> `docs/implement-plans/uc4-asset-import.md`
    - `Portfolio Alert Refactor` -> `docs/implement-plans/portfolio-alert-refactor.md`
- Include only the sections that add value, but prefer this structure:
  - Summary
  - Goals
  - Non-goals
  - Current state
  - Proposed approach
  - Data model or contract changes
  - Backend changes
  - Frontend changes
  - API or integration changes
  - Validation and error handling
  - Security and permissions
  - Performance or caching considerations
  - Files likely to change
  - Test plan
  - Implementation todo list
  - Risks
  - Open questions

### Phase 3 -- Prepare Todo List And Stop for Review
- Create or update the workspace todo list after saving the spec.
- Mirror the implementation todo list from the spec into the workspace todo list using concise, ordered tasks.
- Keep every todo item in `pending` status while waiting for user approval.
- Do not mark implementation work as `in_progress` until the user explicitly approves the spec or asks to start a specific todo.
- Explicitly ask the user to review the spec.
- Include the saved file path in the response.
- Mention that the todo list is ready for step-by-step execution after approval.
- Do not edit application code yet.
- Do not create migrations, endpoints, UI, or tests in this phase unless the user explicitly asks for a draft artifact as part of the spec review.

### Phase 4 -- Revise the Spec
- Incorporate the user's feedback into the spec.
- Track decisions and resolved open questions.
- If the requested direction conflicts with repository rules, call that out and propose a compliant alternative.

### Phase 5 -- Implement Only After Approval
- Start implementation only after the user clearly approves the spec.
- Follow the approved plan closely.
- If implementation reveals a necessary deviation, pause and surface the change for confirmation when it is material.

## Output Requirements

When writing the implementation spec:
- Reference concrete files, classes, handlers, endpoints, pages, or services whenever possible.
- Mention impacted layers explicitly: Domain, Application, Infrastructure, Presentation, Client, Tests.
- Call out dependencies, permissions, cache invalidation, validation, localization, and responsive UI requirements when relevant.
- Prefer actionable bullet points over long prose.
- Separate confirmed decisions from assumptions.
- Include an `Implementation Todo List` section with ordered tasks that can be executed one by one later.
- Persist the spec as a `.md` file under `docs/implement-plans/` before responding to the user.
- Persist the same implementation tasks into the workspace todo list so the user can later ask for execution step by step.
- In the response, summarize the spec briefly and point the user to the saved Markdown file for review.
- In the response, state that the workspace todo list has been prepared from the spec.
- When the request is based on an existing document, mention both the source document path and the generated plan path in the response.

## Guardrails

- Never jump straight to coding for non-trivial feature work.
- Never present a vague plan as if it were implementation-ready.
- Never hide uncertainty. Capture it in open questions or assumptions.
- Never bypass architectural rules in order to make the spec shorter.
- Never keep the spec only in chat when this skill is used. The spec must also be written to `docs/implement-plans/`.
- Never create oversized todo items that combine unrelated work. Split them into clear, sequential tasks.
- Never start executing todo items before the user approves the spec or requests implementation.

## Spec Template

```md
## Summary
- Short description of the requested change.

## Goals
- What the change must achieve.

## Non-goals
- What is intentionally excluded.

## Current State
- Relevant existing flow, files, and constraints.

## Proposed Changes
- Domain:
- Application:
- Infrastructure:
- Presentation:
- Client:

## Data / Contract Changes
- DTOs, entities, API contracts, migrations, config, external providers.

## Validation / Security / Permissions
- Validators, auth policies, business rules, cache invalidation.

## Test Plan
- Unit, integration, UI, and manual verification.

## Implementation Todo List
- [ ] Task 1 written as a small, concrete implementation step.
- [ ] Task 2 written as the next dependency-aware step.

## Risks / Open Questions
- Unknowns, trade-offs, and decisions needed from the user.
```

## Example Invocation

- "Add bulk approve for invoices, but write the implementation spec first."
- "Before coding this portfolio alert feature, create an implement spec and let me review it."
- "Plan the refactor for market data sync first, then wait for approval before implementing."
