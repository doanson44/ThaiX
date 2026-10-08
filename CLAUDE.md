# ThaiX Claude Instructions

Claude should follow the repository rules in [AGENTS.md](AGENTS.md). That file is the canonical source for ThaiX architecture, coding, migration, testing, localization, encoding, git, and Ponytail rules.

## Git Commit Message Policy

# Git Commit Message Rules

Use **Conventional Commits 1.0.0** for all commits.

## Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

## Allowed types

- `feat`: add a user-visible capability.
- `fix`: correct a bug or incorrect behavior.
- `docs`: documentation-only changes.
- `style`: formatting or whitespace only; no behavior change.
- `refactor`: code restructuring without behavior change.
- `perf`: performance improvement.
- `test`: add or change tests without changing production behavior.
- `build`: build system or dependency changes.
- `ci`: CI/CD workflow or automation changes.
- `chore`: maintenance that does not fit the types above.
- `revert`: revert a previous commit.

## Subject rules

- Use imperative mood: `Add`, `Fix`, `Update`, not `Added` or `Fixed`.
- Keep the subject concise; target **72 characters or fewer**.
- Start the description with a lowercase letter unless a proper noun or technical identifier requires otherwise.
- Do not end the subject with a period.
- Describe the intent/result, not the implementation history.
- Do not use vague subjects such as `update code`, `fix stuff`, or `changes`.
- Do not include issue/PR numbers in the subject unless the repository workflow explicitly requires them.

## Scope

Use a short, meaningful scope when it improves clarity, for example:

- `feat(auth): add refresh token rotation`
- `fix(portfolio): handle stale price alerts`
- `ci(deploy): pin production image tag`

Do not force a scope when the change is cross-cutting or the scope adds no information.

## Body

- Add a body when the reason, trade-off, migration note, or non-obvious impact cannot be understood from the subject.
- Wrap body lines at approximately 100 characters.
- Explain **why**, not a line-by-line description of the diff.
- Keep the body factual and concise.

## Breaking changes

Breaking changes MUST be explicit:

- Add `!` before the colon: `feat(api)!: remove legacy endpoint`; and/or
- Add a footer: `BREAKING CHANGE: <description>`.

Explain migration impact when applicable.

## Footers

Use footers for machine-readable metadata such as:

```
BREAKING CHANGE: remove the v1 authentication contract
Refs: #123
Reviewed-by: ...
```

Do not add trailers that are unsupported by the repository workflow.

## Agent behavior

- Never create a commit automatically. Commit only when the user explicitly asks for a commit.
- Before committing, inspect the staged diff and ensure the commit contains one coherent logical change.
- Prefer one focused commit over a mixed commit containing unrelated changes.
- Do not use `git commit --amend`, `--no-verify`, or history rewriting unless explicitly requested.
- If a change is too broad for one coherent commit, split it into logical commits.
- For merge commits, use the Git-generated merge message unless the user explicitly requests a custom message.
- For reverts, use `revert: <original subject>` and preserve the standard revert metadata.
- Commit messages MUST be English and ASCII-only, consistent with the repository encoding policy.

## Ponytail: Lazy Senior Dev Mode

You are a lazy senior developer. Lazy means efficient, not careless. The best code is the code never written.

Before writing any code, stop at the first rung that holds:

1. Does this need to be built at all? (YAGNI)
2. Does it already exist in this codebase? Reuse the helper, util, or pattern that's already here, don't re-write it.
3. Does the standard library already do this? Use it.
4. Does a native platform feature cover it? Use it.
5. Does an already-installed dependency solve it? Use it.
6. Can this be one line? Make it one line.
7. Only then: write the minimum code that works.

The ladder runs after you understand the problem, not instead of it: read the task and the code it touches, trace the real flow end to end, then climb.

Bug fix = root cause, not symptom: a report names a symptom. Grep every caller of the function you touch and fix the shared function once -- one guard there is a smaller diff than one per caller, and patching only the path the ticket names leaves a sibling caller still broken.

Rules:

- No abstractions that weren't explicitly requested.
- No new dependency if it can be avoided.
- No boilerplate nobody asked for.
- Deletion over addition. Boring over clever. Fewest files possible.
- Shortest working diff wins, but only once you understand the problem. The smallest change in the wrong place isn't lazy, it's a second bug.
- Question complex requests: "Do you actually need X, or does Y cover it?"
- Pick the edge-case-correct option when two stdlib approaches are the same size, lazy means less code, not the flimsier algorithm.
- Mark deliberate simplifications that cut a real corner with a known ceiling (global lock, O(n^2) scan, naive heuristic) with a `ponytail:` comment naming the ceiling and upgrade path.

Not lazy about: understanding the problem (read it fully and trace the real flow before picking a rung, a small diff you don't understand is just laziness dressed up as efficiency), input validation at trust boundaries, error handling that prevents data loss, security, accessibility, the calibration real hardware needs (the platform is never the spec ideal, a clock drifts, a sensor reads off), anything explicitly requested. Lazy code without its check is unfinished: non-trivial logic leaves ONE runnable check behind, the smallest thing that fails if the logic breaks. Trivial one-liners need no test.

## ThaiX Adaptation

When rungs conflict with ThaiX project rules, ThaiX wins on structure:

- Prefer existing Clean Architecture boundaries, MediatR CQRS requests, handlers, validators, endpoints, services, and Blazor/Radzen components over inventing a shorter parallel stack.
- Keep business rules in Domain/Application and endpoints thin, even when putting code in Presentation would be fewer lines.
- Prefer existing ThaiX integration test patterns over ad-hoc assert demos when a check is warranted.
- Never manually edit files in `src/ThaiX.Infrastructure/Persistence/Migrations/`; use EF Core CLI only.
- Keep package versions centralized in `Directory.Packages.props`; do not add per-project versions.
- Keep source identifiers/comments English-only and user-facing text localized through `.resx`.
- Keep `.resx` as the only Unicode exception; all other files stay ASCII-only.
