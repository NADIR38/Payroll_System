# AGENTS.md — PayrollMS

## Project
Multi-tenant payroll microservice. ASP.NET Core 9 (Clean Architecture, CQRS+MediatR, EF Core 9,
PostgreSQL/Supabase) + Next.js 15 frontend. This file is the cross-tool twin of `CLAUDE.md` —
keep the two in sync; don't let them diverge on business rules, only on tool-specific syntax.

## Before planning any task
Read, in this order:
1. `docs/00-prd/modules/<relevant-module>.md` — the business rules for whatever you're touching
2. `docs/01-erd/erd.mermaid` + `erd-explained.md` — schema and *why* it looks the way it does
3. `docs/02-diagrams/state-machines/*.mermaid` — legal status transitions
4. `docs/02-diagrams/sequence-diagrams/*.mermaid` — required step order for multi-actor flows

Do not start writing code from a task description alone if a matching module doc exists —
summarize what you read from the doc back to the user as part of your plan.

## Hard rules (deny-by-default — confirm before doing any of these)
- Never hard-delete a row from a financial table (`payroll_*`, `disbursement_*`, `loan_*`,
  `advance_*`, `bonus_*`, `audit_logs`). Soft delete / new version only.
- Never edit `PayrollEntry` / `PayrollEntryComponent` once the parent `PayrollRun.Status` is
  past `GENERATED` — that requires a Correction run (`docs/02-diagrams/sequence-diagrams/05-dispute-correction.mermaid`).
  Never modify a Postgres migration file without explicit confirmation.
- Never add a formula-evaluation function outside the NCalc whitelist
  (`Abs, Round, Floor, Ceiling, Min, Max, If`).
- Never move `company_id` scoping out of a query "for simplicity."

## Conventions
- Endpoints under `/api/v1/`; new breaking versions get `/api/v2/`, never mutate v1 in place.
- Clean Architecture layering: `Domain` has no EF Core/HTTP references.
- One FluentValidation validator per command (picked up by `ValidationBehavior`).

## Tech stack
- Backend: ASP.NET Core 9, EF Core 9 + Npgsql, MediatR, FluentValidation, NCalc2, QuestPDF,
  Hangfire (Postgres storage), MailKit/Resend, ClosedXML, Serilog.
- Frontend: Next.js 15 (App Router), TypeScript, Tailwind, TanStack Query/Table, React Hook
  Form + Zod, Recharts, react-pdf.
- Infra: Supabase Postgres + Storage, Railway (API), Vercel (frontend).

## Build & verify
```
dotnet build PayrollMS.sln && dotnet test
cd payrollms-web && npm run build
```
<!-- replace with real commands once the solution/repo exists -->
