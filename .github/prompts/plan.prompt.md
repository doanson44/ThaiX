---
description: "Plan a task with minimal steps. Identify which layers to change and propose a concrete plan."
agent: "agent"
argument-hint: "Describe the goal and any constraints"
---

Plan the requested task:

1. **Understand**: Clarify the goal and identify affected layers
2. **Propose**: Plan with max 3 steps, specifying the layer for each step
3. **Constraints**: Minimal changes, respect layering (Domain -> Application -> Infrastructure -> Presentation)

Output format:
- Step 1: [Layer] - [What to do]
- Step 2: [Layer] - [What to do]
- Step 3: [Layer] - [What to do]
