---
description: "Fix async/await bugs including race conditions, deadlocks, missing awaits, or incorrect concurrent access."
agent: "agent"
argument-hint: "Describe the async problem and context"
---

Fix the async/await issue:

- Ensure no race conditions
- Correct async/await usage throughout
- Proper `CancellationToken` propagation
- No blocking calls (`.Result`, `.Wait()`) on async paths
- Return updated function only
