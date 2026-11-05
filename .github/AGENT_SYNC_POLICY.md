# Agent Asset Sync Policy

This repository uses canonical shared assets plus mirrors for the agent runtimes present in the checkout:

- `.github/` for GitHub Copilot (VS Code)
- `.codex/` / `.agents/` for OpenAI Codex and shared agent assets
- `.claude/` for Claude skills and mirrored agent assets
- `.cursor/` for Cursor IDE
- `.trae/` for Trae IDE
- `.continue/` for Continue.dev (VS Code + JetBrains)

## Runtime Conventions (as of June 2026)

### GitHub Copilot (`.github/`)

```
.github/
├── copilot-instructions.md          # Always-on, plain markdown (no YAML)
├── instructions/
│   └── *.instructions.md            # File-based, YAML: description, applyTo
├── prompts/
│   └── *.prompt.md                  # YAML: description, agent, argument-hint
├── skills/
│   └── */SKILL.md                   # YAML: name, description
└── agents/
    └── *.agent.md
```

Ref: https://code.visualstudio.com/docs/copilot/customization/custom-instructions

### Codex (`.codex/`)

```
<repo root>/
├── AGENTS.md                        # Plain markdown, hierarchical scoping
├── .agents/
│   ├── rules/
│   │   └── *.md                     # Mirrored shared rules
│   └── skills/
│       └── */SKILL.md               # YAML: name, description
└── .codex/
    ├── config.toml                  # Project config (sandbox, MCP, hooks, model)
    └── agents/
        └── *.toml                   # name, description, developer_instructions
```

Ref: https://github.com/openai/codex (AGENTS.md + .agents/skills convention)

### Claude (`.claude/`)

```
<repo root>/
├── CLAUDE.md                         # Claude always-on project instructions
└── .claude/
    ├── rules/
    │   └── *.md                      # Mirrored shared rules
    └── skills/
        └── */SKILL.md                # YAML: name, description
```

### Cursor (`.cursor/`)

```
.cursor/
├── rules/
│   └── *.mdc                        # YAML: alwaysApply, globs, description
├── prompts/
│   └── *.prompt.md                  # Shared prompt files
└── skills/
    └── */SKILL.md                   # YAML: name, description
```

Ref: https://cursor.com/docs/rules

### Trae (`.trae/`)

```
.trae/
├── rules/
│   └── *.md                         # YAML: alwaysApply, scene (optional)
└── skills/
    └── */SKILL.md                   # YAML: name, description (open agent skills std)
```

Ref: https://www.trae.ai/blog/trae_tutorial_0825 (Rules) + https://www.trae.ai/blog/trae_tutorial_0115 (Skills)

### Continue.dev (`.continue/`)

```
.continue/
├── agents/
│   └── *.yaml                       # name, version, schema, systemMessage, models
├── skills/
│   └── */SKILL.md                   # Repository-local mirror for shared skill prompts
├── rules/
│   └── *.md                         # YAML: globs, description, alwaysApply (optional)
├── prompts/
│   └── *.prompt.md                  # YAML: description, agent, argument-hint
├── models/
│   └── *.yaml                       # Model block definitions
├── mcpServers/
│   └── *.yaml                       # MCP server configurations
├── docs/
│   └── *.yaml                       # Documentation context blocks
└── context/
    └── *.yaml                       # Custom context providers
```

Ref: https://docs.continue.dev/reference

## Shared Assets

The canonical shared source is `.agents/`. At the time of this policy update, the repository contains `.agents/`, `.github/`, and `.cursor/` agent assets. `.claude/`, `.trae/`, and `.continue/` are supported destinations only when those directories are actually present; do not create empty runtime trees solely to satisfy this policy.

- **Skills**: Shared across all runtimes. Canonical source: `.agents/skills/` (Codex
  convention, also compatible with Copilot). Sync to `.github/skills/`,
  `.cursor/skills/` and any other runtime skill directory that exists in the checkout.
- **Rules**: Shared always-on rules should be mirrored to `.agents/rules/`,
  `.cursor/rules/` and any other runtime rules directory that exists in the checkout
  using each runtime's native frontmatter.
- **AGENTS.md**: Root-level file used by Codex and also recognized by Copilot
  (as alternative to `copilot-instructions.md`). Single source of truth for
  repository-level guidelines.
- **CLAUDE.md**: Root-level file used by Claude. Keep it aligned with `AGENTS.md`
  for global repository rules.

## Sync Workflow

1. When changing a skill, update `.agents/skills/` first, then synchronize all matching skill mirrors present in `.github/skills/` and `.cursor/skills/`. Synchronize any additional runtime mirror only if it exists.
2. When changing layer-specific rules, update each existing runtime's native format separately
   (they use different YAML frontmatter conventions).
3. When changing global architecture rules, update `AGENTS.md`, `CLAUDE.md`, and `.github/copilot-instructions.md`.
4. Preserve each runtime's YAML frontmatter conventions -- do not cross-contaminate.
5. When changing shared prompts, propagate to `.github/prompts/` and `.cursor/prompts/`, plus other prompt directories only when present.
