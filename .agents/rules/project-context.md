---
name: project-context
description: Always-on project context for PayrollMS — load for every task.
---

See `/AGENTS.md` at repo root for the full rule set. This file exists only because Antigravity's
`.agents/rules/` convention is where teams put *scoped* rules once the project grows — for a
single-service repo like this, root `AGENTS.md` is enough. Split rules out here (e.g.
`payroll-engine.md`, `frontend.md`) only once different subagents need different context.
