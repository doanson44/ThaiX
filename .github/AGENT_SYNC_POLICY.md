# Agent Asset Sync Policy

This repository keeps agent assets in five runtime folders, each following
the IDE's native convention:

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

- **Skills**: Shared across all runtimes. Canonical source: `.agents/skills/` (Codex
  convention, also compatible with Copilot). Sync to `.github/skills/`,
  `.claude/skills/`, `.cursor/skills/`, `.trae/skills/`, and `.continue/skills/` on change.
- **Rules**: Shared always-on rules should be mirrored to `.agents/rules/`,
  `.claude/rules/`, `.cursor/rules/`, `.trae/rules/`, and `.continue/rules/`
  using each runtime's native frontmatter.
- **AGENTS.md**: Root-level file used by Codex and also recognized by Copilot
  (as alternative to `copilot-instructions.md`). Single source of truth for
  repository-level guidelines.
- **CLAUDE.md**: Root-level file used by Claude. Keep it aligned with `AGENTS.md`
  for global repository rules.

## Sync Workflow

1. When changing a skill, update `.agents/skills/` first, then propagate to `.github/skills/`, `.claude/skills/`, `.cursor/skills/`, `.trae/skills/`, and `.continue/skills/`.
2. When changing layer-specific rules, update each runtime's native format separately
   (they use different YAML frontmatter conventions).
3. When changing global architecture rules, update both `AGENTS.md` (root) and
   `CLAUDE.md` (root) and `copilot-instructions.md` (`.github/`).
4. Preserve each runtime's YAML frontmatter conventions -- do not cross-contaminate.
5. When changing shared prompts, propagate to `.github/prompts/`, `.cursor/prompts/`,
   `.trae/prompts/`, and `.continue/prompts/`.
