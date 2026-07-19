# PayrollMS — Documentation Home

This is the Obsidian vault root (or open just `docs/` as the vault — see setup notes below).

## Map of Content

- [[00-prd/README|PRD]] — product requirements, module by module
- [[01-erd/erd-explained|ERD — entities explained]] ([[01-erd/erd|raw diagram]])
- [[02-diagrams/diagrams-explained|State machines & sequence diagrams — explained]]
  - State machines:
    - [[02-diagrams/state-machines/payroll-run|PayrollRun lifecycle]]
    - [[02-diagrams/state-machines/dispute-ticket|DisputeTicket lifecycle]]
    - [[02-diagrams/state-machines/loan-and-disbursement|Loan / Disbursement / BankTransfer]]
  - Sequence diagrams:
    - [[02-diagrams/sequence-diagrams/01-external-sync|01 — External ERP sync]]
    - [[02-diagrams/sequence-diagrams/02-payroll-generation|02 — Payroll generation]]
    - [[02-diagrams/sequence-diagrams/03-approval-workflow|03 — Approval workflow]]
    - [[02-diagrams/sequence-diagrams/04-salary-slip-delivery|04 — Salary slip delivery]]
    - [[02-diagrams/sequence-diagrams/05-dispute-correction|05 — Dispute correction]]
    - [[02-diagrams/sequence-diagrams/06-disbursement|06 — Disbursement]]

## How this folder is used by three different tools

| Tool | What it reads | Where |
|---|---|---|
| **Obsidian** | every `.md` and `.mermaid` file, rendered with wikilinks + graph view | this whole `docs/` folder as the vault |
| **Claude Code** | `CLAUDE.md` at repo root, plus anything it's told to `view`/`grep` under `docs/` | repo root |
| **Google Antigravity** | `AGENTS.md` at repo root, plus `.agents/rules/*.md` | repo root |

Nothing here is tool-specific markup — it's plain Markdown and Mermaid, so all three read the
*same files*. See the root `CLAUDE.md` / `AGENTS.md` for how each agent is pointed at this folder.
