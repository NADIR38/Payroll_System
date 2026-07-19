# PayrollMS

Enterprise multi-tenant payroll microservice. ASP.NET Core 9 (Clean Architecture, CQRS+MediatR,
EF Core 9) + Next.js 15 frontend. Full design context lives in `docs/` — **read the relevant
doc before touching a module**, don't guess at business rules from the code alone.

## Where the design docs are (read these, don't re-derive them)

- `docs/00-prd/` — the PRD. One file per module (`docs/00-prd/modules/NN-*.md`). This is the
  source of truth for business rules — if code and PRD disagree, ask before "fixing" either.
- `docs/01-erd/erd.mermaid` + `erd-explained.md` — full schema, and *why* each table/column
  exists. Check `erd-explained.md` before adding/renaming a column — there's usually a stated
  reason (snapshotting, tenant isolation, immutability) for why a field looks "redundant."
- `docs/02-diagrams/state-machines/*.mermaid` — the only valid status transitions for
  `PayrollRun`, `DisputeTicket`, `LoanInstallment`, `DisbursementBatch`, `BankTransferRecord`.
  Any code that changes a `.Status` field must match one of these edges exactly.
- `docs/02-diagrams/sequence-diagrams/*.mermaid` — the required order of operations for the six
  multi-step flows (sync, payroll generation, approval, slip delivery, dispute correction,
  disbursement). Don't reorder steps (e.g. don't fire the webhook before the DB commit).

## Core invariants (violating these is a bug, not a style choice)

1. **Every tenant table carries `company_id`.** RLS enforces isolation at the DB layer —
   never write a query that could cross tenants even if RLS would also catch it.
2. **Snapshot, don't reference, once money is calculated.** `PayrollEntry`,
   `PayrollEntryComponent`, `SalarySlip`, and approval records copy the facts they depended on
   (name, IBAN, formula text) instead of joining live to `EmployeePayrollProfile` /
   `SalaryStructure`. Never "fix" this by replacing a snapshot column with a join.
3. **Nothing about money is hard-deleted or edited in place post-approval.** Corrections are new
   versioned rows (`PayrollRun.Version`, `SalarySlip.Version`, `*History` tables), and
   `AuditLog` gets a row for every mutation (via the MediatR audit behavior — don't call it
   manually per-handler).
4. **Formulas run through NCalc only**, whitelisted functions
   (`Abs, Round, Floor, Ceiling, Min, Max, If`), no reflection/file IO. Never add a formula
   function without updating the whitelist *and* `docs/00-prd/modules/05-formula-engine.md`.

## Build & test

```
dotnet build PayrollMS.sln
dotnet test
cd payrollms-web && npm run dev      # frontend
```
<!-- fill in real lint/test commands once the solution exists; Claude should not guess these -->

## Conventions

- Backend: Clean Architecture layering (`Domain` → `Application` → `Infrastructure` → `API`) —
  don't put EF Core or HTTP concerns in `Domain`/`Application`.
- New endpoints go under `/api/v1/`; breaking changes get `/api/v2/`, they don't mutate v1.
- Every new command needs a FluentValidation validator picked up by `ValidationBehavior` —
  don't hand-roll validation inside a handler.

## When you're unsure

Prefer re-reading `docs/00-prd/modules/<module>.md` and `docs/01-erd/erd-explained.md` over
guessing. If the PRD and a request from me conflict, say so before implementing.
