# PayrollMS — State Machines & Sequence Diagrams Explained

Two kinds of diagrams, six files total:

**State machines** (what statuses an entity can be in, and what's allowed to move it):
- `state_payroll_run.mermaid` — the big one, §15.3
- `state_dispute_ticket.mermaid` — §18.3
- `state_loan_and_disbursement.mermaid` — three smaller machines: LoanInstallment, DisbursementBatch, BankTransferRecord

**Sequence diagrams** (who calls whom, in what order, for one specific workflow):
- `seq_01_external_sync.mermaid` — ERP pushes employee/attendance/leave data in
- `seq_02_payroll_generation.mermaid` — the Hangfire worker that actually runs the formula engine
- `seq_03_approval_workflow.mermaid` — multi-step approve/reject, including a rejection loop
- `seq_04_salary_slip_delivery.mermaid` — PDF generation → storage → email
- `seq_05_dispute_correction.mermaid` — dispute → correction run → resolved, all three branches
- `seq_06_disbursement.mermaid` — bank file generation → manual bank upload → confirm

---

## Why state machines at all (instead of just booleans)?

The PRD itself insists on this (§15.3, §18.3): `PayrollRun.Status` and `DisputeTicket.Status` are modeled as **single enum columns with an explicit, enforced transition graph**, not as a handful of independent boolean flags (`is_approved`, `is_disbursed`, `is_disputed`...). The reason is combinatorial: with N booleans you can represent 2^N states, almost all of which are nonsensical ("disbursed=true but approved=false"). A state machine only allows the transitions someone actually designed, which is exactly why the PRD's error codes include things like `PAYROLL_FROZEN` and `APPROVAL_ALREADY_COMPLETED` — those are illegal-transition guards, not generic validation errors.

### `PayrollRun` — why each transition exists
- **DRAFT → GENERATING → GENERATED**: `GENERATING` is called out in the PRD as a *transient* state (§15.5, step 1) that isn't part of the "official" state machine list in §15.3 — it exists only so a concurrent request can tell "a job is actively running" apart from "nothing has happened yet." I included it because otherwise two people clicking "generate" at once would both see DRAFT and both queue a job.
- **GENERATED/UNDER_REVIEW → DISPUTED**: a side-state, reachable from either point, because a dispute can be raised as soon as a slip exists — it doesn't wait for the full approval chain to finish.
- **DISPUTED → GENERATED**: this is the correction path — resolving a dispute with a required recalculation produces a **new PayrollRun** (not a return to the old one's UNDER_REVIEW), which is why the diagram shows the correction run re-entering at GENERATED-equivalent state, going through approval fresh.
- **UNDER_REVIEW → GENERATED on rejection**: explicitly required by §16.5 — "rejection at any step returns the run to GENERATED status; HR is notified." This is a deliberate one-step-back design (not "go to the previous step") — a full restart of the review is required after any fix, rather than resuming mid-chain, so nothing gets silently re-approved without a human re-checking the whole thing.
- **APPROVED → DISBURSEMENT_PENDING → DISBURSED**: this is the only place a state can partially "fail" without regressing — a `PartiallyFailed` disbursement batch does *not* drag `PayrollRun` back to an earlier status; disbursement failures are absorbed entirely inside `DisbursementBatch`'s own machine (see below), keeping the payroll calculation itself untouched once it's approved.
- **Immutability boundary**: I marked GENERATED as the freeze line in the diagram notes because §2.4 is explicit — "once a PayrollRun moves past GENERATED status, its snapshots are frozen." Everything after that point (UNDER_REVIEW onward) can change *approval* state but never *calculation* state.

### `DisputeTicket` — why it branches three ways from `UNDER_REVIEW`
The PRD gives three distinct outcomes once a dispute is investigated (§18.3), and the diagram keeps them as three separate edges rather than collapsing "resolved with correction" and "resolved without correction" into one RESOLVED transition with a side-flag — because the *path* to RESOLVED matters for reporting (the Dispute Report in §23.1 needs to distinguish "resolved via correction" from "resolved via explanation" for resolution-time SLA analysis), even though the *destination* status is the same enum value.

### `LoanInstallment` / `DisbursementBatch` / `BankTransferRecord`
These three are grouped in one file because they're small, linear, mostly-one-way machines — worth noting is that **`BankTransferRecord.Status` and `PayrollEntry.Status` are deliberately decoupled** (see the note in the diagram): a bank rejecting an IBAN happens *after* the payroll entry is already Locked, and must not be able to reopen it. This is the same "don't let a downstream process corrupt an upstream frozen snapshot" principle used everywhere else in the schema.

---

## Why sequence diagrams for these six flows specifically?

I picked the six flows that the PRD describes as **multi-actor, multi-step processes** (as opposed to simple CRUD) — these are the places where *order of operations* is itself a business rule, not just an implementation detail:

### 1. External Sync (`seq_01`)
Why it's worth diagramming: the PRD specifies a precise **three-layer tenant resolution** before any data touches the database (middleware → MediatR behavior → RLS, §5.3) — showing this as a sequence makes clear that `ICurrentTenant.CompanyId` is resolved *once*, early, by the API-key lookup, and then flows through every later step, rather than each layer re-deriving the tenant independently (which could, in theory, disagree with itself). The upsert branch (exists vs. not) is shown explicitly because it's the mechanism that produces the `EmployeePayrollProfileHistory` chain described in §7.4.

### 2. Payroll Generation (`seq_02`)
This is the most important diagram in the set because it's the literal translation of the PRD's own numbered algorithm in §15.5 into an actor-interaction view. The nested loops matter: the **outer loop is per-employee** (parallelizable, "max 10 concurrent" — shown as a note-worthy constraint even though sequence diagrams don't natively show concurrency limits well), and the **inner loop is per-component-in-sequence-order**, which is where `AllowanceRule`/`DeductionRule` checks happen *before* the formula is evaluated, not after — this ordering is what makes conditional components (like `ExcludeSummerVacation`) work correctly instead of computing a value and then discarding it.

### 3. Approval Workflow (`seq_03`)
I deliberately diagrammed a **reject-then-fix-then-re-approve** cycle instead of only the happy path, because the PRD's error codes (`APPROVAL_STEP_UNAUTHORIZED`, `APPROVAL_ALREADY_COMPLETED`) only make sense in light of what happens when the *wrong* actor tries to act, or when a step is repeated — showing only success would hide why those guards exist. The diagram also shows that a rejection resets the *run's* status but the approval history itself is never erased — `PayrollApprovalRecord` for the rejected step stays in the table, only a new row is added for the re-approval.

### 4. Salary Slip Delivery (`seq_04`)
The `par` (parallel) block for email delivery is intentional: PDF generation for all employees happens in a loop, but email sending is queued as *separate* Hangfire jobs per slip (§17.4, step 4 is its own job from step 3) — this decoupling is why a slow email provider can't block slip generation for the next employee, and why `SalarySlip.EmailSentAt` is a nullable field that gets filled in asynchronously, sometimes noticeably after `GeneratedAt`.

### 5. Dispute → Correction (`seq_05`)
This is the most complex flow in the PRD because it chains together three other diagrams: raising a dispute, running a scoped correction through the *same* approval machinery as a normal run, and regenerating a slip. I diagrammed all three of the dispute's possible endings (correction / direct resolution / rejection) in one file specifically so you can see they share the same first few steps (assign → investigate → comment) and only diverge at the point where the officer decides which status to move it to — which mirrors exactly how the state machine forks at `UNDER_REVIEW`.

### 6. Disbursement (`seq_06`)
The key thing this diagram makes visible that a data model alone can't: there is a **manual, out-of-band step** (uploading the CSV to an actual banking portal) sitting in the middle of an otherwise-automated pipeline, because v1 explicitly excludes direct bank API integration (§1.5). The `PATCH .../records/{rid}/fail` call is shown as `opt`(ional) because it only happens if the finance officer's manual bank upload had partial failures — this is the one point in the whole system where an external, unverifiable human action ("I uploaded it to the bank") is trusted and recorded via `POST /confirm`, rather than the system verifying it independently.

---

## What I intentionally left out (and why)

- **CRUD-only flows** (creating a Branch, a SalaryComponent, a Designation) — these are single request/response calls with no meaningful ordering or branching, so a sequence diagram would add nothing over just reading the endpoint list in §25.4.
- **Webhook delivery internals** (retry/backoff logic) — the PRD describes webhook *firing* as a side-effect of other flows (shown as a single arrow to `WebhookDispatcherService` in each diagram above) but never specifies its own retry state machine in detail, so diagramming one would be inventing behavior not in your PRD.
- **Report generation** (§23.3) — synchronous vs. queued (>500 employees) is a two-branch decision, not really a *sequence* worth its own diagram; it's one `alt` block, which I mentioned inline in the Payroll Generation diagram's spirit rather than as a seventh file.
