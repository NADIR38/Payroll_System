# PayrollMS — ERD Explained, Entity by Entity

This document walks through **every table** in the diagram, and for each one answers three questions:

- **Why does this entity exist at all** (what problem in the PRD does it solve)?
- **Why these specific attributes** (what would break without them)?
- **Why not the obvious alternative** (what simpler design was rejected, and why)?

Three ideas from the PRD repeat across almost every table, so it's worth naming them once instead of 40 times:

1. **Multi-tenancy (`company_id` everywhere)** — PayrollMS is *one* system serving *many* ERPs (School ERP, Hospital ERP, etc.). Every table that holds tenant data carries `company_id`, enforced three times over (middleware → MediatR behavior → Postgres RLS). This is why almost every entity below has `company_id FK` even when it's technically reachable through a parent (e.g. `PAYROLL_ENTRY_COMPONENT` could get `company_id` by joining through `PAYROLL_ENTRY`) — the PRD denormalizes it deliberately so that a single RLS policy (`company_id = current_setting(...)`) can protect every table independently, without relying on join correctness.
2. **Snapshotting** — PayrollMS explicitly does **not** own employee master data; it's fed by external ERPs. But once payroll runs, it must never let a later HR edit silently rewrite historical payslips. So anything that touches money copies (snapshots) the facts it depended on, at the moment it depended on them, into its own row — name, IBAN, formula text, department name, etc. This is why you'll see `employee_name`, `iban`, `department_name` duplicated on `PAYROLL_ENTRY` even though they "already exist" on `EMPLOYEE_PAYROLL_PROFILE`.
3. **Append-only / immutability** — nothing about money is ever destructively edited. Corrections create new versioned rows (`PayrollRun.Version`, `SalarySlip.Version`, `EmployeePayrollProfileHistory`) rather than overwriting old ones, and every mutation is mirrored into `AUDIT_LOG`.

Keep these three in mind — they explain 80% of "why this attribute" answers below.

---

## Module 01 — Tenant & Company Management

### `COMPANY`
**Why it exists:** This is the tenant root. Every other table ultimately hangs off a company, because PayrollMS is a multi-tenant service that any ERP (School, Hospital, future ERPs) can plug into.
**Why these attributes:**
- `code` (globally unique, not per-something-else) — because API keys and integrations reference a company by a stable human-readable code, not just a UUID.
- `logo_url` — needed at PDF-generation time (salary slips print the company logo, §17.3).
- `is_active` — companies are soft-disabled rather than deleted, consistent with the "nothing is ever deleted" principle (§27.4).
**Why not simpler:** You might ask why `Company` isn't just a row in a generic `tenants` table shared with other unrelated products — because RLS policies key off `company_id` directly in this schema; keeping `Company` as a first-class, richly-attributed entity (with contact info, logo, etc.) is required since PayrollMS itself renders documents (slips, reports) that need this data, not just an ID.

### `BRANCH`, `DEPARTMENT`, `DESIGNATION`, `COST_CENTER`
**Why they exist:** The PRD's formula engine and reporting need to slice payroll by branch/department/designation/cost-center (see Module 18's "Department Salary Report," "Cost Center Report"). These aren't just labels — `AllowanceRule.DesignationSpecific` and `DepartmentSpecific` modes (§11.3) literally branch payroll logic on them, so they must be real, queryable entities, not free-text fields on the employee.
**Why these attributes:**
- `code` unique **per company**, not globally — because two different school-ERP tenants might both use `"HR"` as a department code; uniqueness is scoped to avoid an artificial global-uniqueness constraint that has nothing to do with the business.
- `Designation.grade` (e.g. "BPS-17") — this is a real field seen in government/education payroll for grade-based allowance rules; it's nullable because private-sector companies won't use it.
- `Department.branch_id` is **nullable** — because the PRD explicitly allows department-only or company-wide departments that aren't tied to a specific branch (e.g. "Head Office Admin" that isn't part of any single branch).
**Why not one generic "OrgUnit" table:** A tempting simplification is a single self-referencing `OrgUnit(parent_id)` tree instead of four separate tables. The PRD rejects this because Branch, Department, Designation, and CostCenter each have **different cardinality relationships to the employee** (an employee has exactly one department AND one designation AND optionally one cost center simultaneously — they're not a strict hierarchy), and because `AllowanceRule` needs to filter by designation independent of department. A generic tree can't express "apply to Designation X regardless of Department" as cleanly as separate foreign keys can.

### `FINANCIAL_YEAR`
**Why it exists:** Reports like "Yearly Salary Ledger" (§23.1) and `IsCurrent` gating in Module 01 need an explicit year boundary that doesn't just default to Jan–Dec (many Pakistani/education organizations run July–June fiscal years, which the PRD's month numbering under `PayrollCalendar` supports).
**Why these attributes:** `IsCurrent` — the business rule "only one FinancialYear can be current per company" (§6.4) exists so payroll generation and new-employee-sync logic always have an unambiguous default year to attach to, without every command having to ask "which year do you mean?"

### `PAYROLL_CALENDAR`
**Why it exists:** This is the single most important "control" table in the whole system — it defines what a "working day" even *means* for a given month, and it's the thing that freezes a period. Without it, "WorkingDays" in a formula would be undefined and there would be no authoritative freeze mechanism.
**Why these attributes:**
- `working_days` is explicitly called out as **authoritative** (§6.4) — deliberately overriding whatever attendance sync reports, because attendance is *actual* days worked, while `PayrollCalendar.WorkingDays` is the *official* denominator used to compute daily rates (e.g. `BaseSalary / CalendarWorkingDays`). These are two different numbers on purpose — conflating them would make per-day deduction math wrong whenever attendance doesn't perfectly match the calendar (e.g. a company that officially has 26 working days but an employee's attendance system reports 25 present + 1 approved late arrival still counted as present).
- `payroll_freeze_date` — this single field is what powers the `PAYROLL_FROZEN` error code (§28.1) and the attendance-sync rejection rule (§13.4). It has to live on the calendar (not on PayrollRun) because the freeze is a *period* property that exists even before any PayrollRun has been created for that month.
- `holidays` stored as JSON rather than a separate `Holiday` table — a deliberate simplification: holidays are read as a whole list at formula-evaluation time and never queried individually across months, so a normalized table would add joins for no benefit.

---

## Module 02 — Employee Payroll Profile

### `EMPLOYEE_PAYROLL_PROFILE`
**Why it exists:** The PRD is explicit that PayrollMS does **not** own employee master data (§1.3) — the real HR record lives in the calling ERP. This table is a deliberately thin *payroll-relevant slice* of that record, synced in via `POST /employees/sync`. It exists so payroll calculations never need to call back out to the external ERP mid-calculation (which would be slow, fragile, and break the "PayrollMS works even if the ERP is offline" property).
**Why these attributes:**
- `external_employee_id` + `company_id` as the **upsert key** (§7.4), not the internal UUID — because the source of truth for "who this employee is" is the calling ERP, and re-syncing must be idempotent from the ERP's point of view without it needing to remember PayrollMS-generated UUIDs.
- `full_name`, `branch_name`, `department_name`, `designation_name` stored as **snapshot labels** alongside the FK — this looks redundant (why store the name if you have `branch_id`?) but it's intentional: if a Branch is renamed next year, old payroll views/reports should not silently relabel; the label at *sync time* is what's authoritative for that profile version.
- `base_salary` — this is not "the" salary, it's the specific number plugged into `{BaseSalary}` in NCalc formulas (§7.4); every other component (`House Rent`, `Medical`, etc.) is derived from it, which is why it lives here rather than being computed.
- `attendance_deduction_opt_in` — a **per-employee** override, because some staff (e.g. contractual/senior staff) are exempt from attendance-based deductions by policy even under the same salary structure as everyone else; if this lived only on `DeductionRule` (company-wide), you couldn't express "everyone except this one employee."
- `effective_from` — required because this table is versioned (see history table below); a profile without a version-start date can't be correctly picked for a payroll period that predates the latest sync.
**Why not just point at the ERP's live employee table:** Because payroll math must be reproducible **months or years later** even if the ERP's employee record has since changed (an employee transferred department, got a raise, etc.). A live join would corrupt historical payslips. This is the "snapshot principle" (§2.4) applied at the profile level.

### `EMPLOYEE_PAYROLL_PROFILE_HISTORY`
**Why it exists:** Directly required by §7.4: "Payroll generation uses the profile version active on the payroll period's start date, not the current version." Without a history table, a mid-month promotion or salary revision would retroactively alter last month's payroll if it were ever regenerated.
**Why these attributes:** `effective_from` / `effective_to` (with `null` meaning "current") is the classic **temporal table** pattern — chosen over "just keep every version as a new profile row" because the *current* pointer (`EmployeePayrollProfile.SalaryStructureId`, etc.) needs to stay a single, cheap-to-query row for 95% of use cases (generating this month's payroll), while history is only consulted for back-dated corrections or audits — separating them keeps the hot path fast.
**Why not event-sourcing the whole profile:** A full event stream (store only diffs/events, replay to reconstruct state) was rejected in favor of storing full historical snapshots — because payroll calculation needs to read one full profile row per employee per run, and reconstructing it by replaying events for every employee on every run would be far slower than reading one denormalized history row.

### `EMPLOYEE_BANK_ACCOUNT`
**Why it exists:** Disbursement (Module 14) needs IBAN/account details, and salary slips display bank info (§17.3). Kept separate from `EmployeePayrollProfile` rather than inlined, because:
- An employee can have **multiple** accounts (`is_primary` flag implies a 1-to-many relationship) — e.g. a savings account and a salary account, or a mid-year bank switch.
- Bank details change independently of and far less often than salary/structure data, so versioning them together with the profile would bloat the profile-history table with irrelevant churn.

---

## Module 03 — Salary Components

### `SALARY_COMPONENT`
**Why it exists:** This is the "atomic vocabulary" of payroll — Basic, House Rent, Medical, etc. — defined once and reused across many structures (§8.1), instead of hardcoding "Basic Salary logic" into application code. This is the mechanism that makes PayrollMS "formula-driven... no hardcoded business logic" (§1.4), which is the product's core value proposition.
**Why these attributes:**
- `type` (Earning / Deduction / EmployerContribution) — this drives how the component rolls up into Gross vs. Net vs. an off-books employer cost; keeping it as an enum on the component (rather than inferring it from the formula's sign) means the system can validate and total correctly even before any formula is evaluated.
- `calculation_method` — exists as a *hint/shortcut* enum alongside the free-form `FormulaExpression`, so simple cases (Fixed, PercentageOfBase) don't require someone to hand-write an NCalc string; but `FormulaExpression` (on `SalaryStructureComponent`, not here) is still the actual thing evaluated — this table just declares the *default shape*.
- `is_optional` — distinguishes components that are auto-applied to every employee on a structure vs. ones that must be manually assigned (e.g. a one-off transport top-up for a specific driver). Without this flag, every special-case allowance would need its own structure variant.
- `is_recurring` — lets Bonus (a `SALARY_COMPONENT` of type Earning but `IsRecurring = false`) share the exact same computation/snapshot pipeline as recurring components, instead of the Bonus module needing entirely separate plumbing through `PAYROLL_ENTRY_COMPONENT`.
**Why not hardcode components in code (an enum of "BasicPay, HouseRent, ...")** This is exactly the alternative the PRD rejects in §1.4 — a hardcoded enum would mean every new ERP customer with a slightly different allowance structure requires a code deployment. Storing components as data lets each tenant define its own vocabulary.

### `SALARY_STRUCTURE` / `SALARY_STRUCTURE_COMPONENT`
**Why they exist:** A `SalaryStructure` is the reusable *template* ("Teacher Structure," "Driver Structure") that groups components together with company- and role-specific formulas; `SalaryStructureComponent` is the join table that also carries the **formula** and **evaluation order** for that specific pairing.
**Why these attributes:**
- `sequence` on `SalaryStructureComponent` — required because formulas can depend on each other (`HouseRent = BaseSalary * 0.4` depends on `Basic` being computed first if referenced by name) (§9.4). This is why the validation rule explicitly forbids a component referencing another component with an equal-or-higher sequence number (§10.6) — it prevents circular/forward dependencies without needing a full dependency graph solver at runtime; a simple ordered list is sufficient and far cheaper to validate.
- `formula_expression` lives on `SalaryStructureComponent`, **not** on `SalaryComponent` — because the same component (e.g. `HOUSE_RENT`) might be 40% of base in one structure and a flat 15,000 in another; the formula is a property of "this component *in this structure*," not of the component in the abstract.
- `EffectiveFrom`/`EffectiveTo` with the rule "create a new version rather than edit in place" (§9.4) — same rationale as employee history: a mid-year formula change must not retroactively alter already-approved payroll runs, which only hold a *snapshot* of the formula text anyway (`PayrollEntryComponent.FormulaUsed`).

### `ALLOWANCE_RULE` / `DEDUCTION_RULE`
**Why they exist as separate tables (not just more columns on `SalaryStructureComponent`):** Both express *conditional applicability* — "should this component apply this month, for this employee?" — which is logically distinct from *how much it's worth* (the formula). Splitting them keeps the formula engine simple ("compute a number") and lets a completely separate rules layer decide "does this number apply at all" (Always / WorkingDaysOnly / ExcludeSummerVacation / DesignationSpecific / etc., §11.3). This mirrors a common "policy vs. calculation" separation so that adding a new *rule type* later doesn't require touching formula-evaluation code.
**Why `DeductionRule.IsOptIn` + `GracePeriodMinutes` specifically:** `IsOptIn` implements the opt-out business rule (§12.4) — an employee who opted out of attendance deductions still has absences *recorded* (via `AttendanceSummary`) but isn't financially penalized; `GracePeriodMinutes` implements a very concrete real-world payroll policy ("no deduction unless late by more than 15 minutes," §12.5) that would otherwise be buried as a magic number inside a formula string, where it couldn't be validated or reported on independently.

---

## Module 08/09 — Attendance & Leave Integration

### `ATTENDANCE_SUMMARY` / `LEAVE_SUMMARY`
**Why they exist:** The PRD is explicit — PayrollMS "never owns raw biometric or punch records" (§13.1); it only ingests **monthly summaries**. This is a deliberate scope boundary: attendance devices, biometric systems, and leave-approval workflows are complex domains the calling ERP already owns, and duplicating that complexity inside a payroll microservice would violate the single-responsibility boundary set out in §1.3.
**Why these attributes:**
- `idempotency_key` (unique) — sync endpoints will legitimately be called more than once (network retries, batch re-runs); rather than making the caller de-duplicate, PayrollMS accepts duplicate keys silently and returns the existing record (§13.4). Without this, a retried HTTP call could double-count absences.
- Separate numeric fields (`working_days`, `absent_days`, `late_days`, `late_minutes`, `half_days`, `overtime_hours`...) rather than one JSON blob — because every one of these is individually referenced **by name** as a variable inside NCalc formulas (`FormulaContext`, §10.3); a JSON blob would require the formula engine to parse/flatten it on every evaluation, and would prevent using these fields directly in SQL-level reports (Module 18's Overtime Report, Deduction Report).
- `LeaveSummary` splits leave into paid/unpaid/medical/casual **and** half-days rather than one "days on leave" number — because each leave type has a different payroll treatment (§14.3: paid leave counts as worked, unpaid leave is deducted, medical/casual are company-configurable). Collapsing these into one number would make that per-type policy impossible to apply.
**Why not store attendance/leave directly on `PayrollEntry`:** Because attendance/leave sync happens *before* a payroll run exists (sometimes weeks earlier, or re-synced multiple times while status is DRAFT, §13.4), and the same summary must be reusable if a payroll run needs to be regenerated. Coupling it to `PayrollEntry` would force re-syncing attendance every time payroll is (re)generated.

---

## Module 10 — Payroll Run Engine

### `PAYROLL_RUN`
**Why it exists:** This is the top-level "batch" concept — one execution of "calculate payroll for Company X, Month Y" (§15.1). Everything about approvals, disbursement, and correction hangs off this one row per run.
**Why these attributes:**
- `status` as an explicit enum with a strict state machine (DRAFT → GENERATED → UNDER_REVIEW → APPROVED → DISBURSEMENT_PENDING → DISBURSED → CLOSED, plus side-states DISPUTED/CANCELLED, §15.3) rather than a set of boolean flags (`is_approved`, `is_disbursed`, ...) — a single status column with an enforced transition graph prevents impossible/contradictory combinations (e.g. "disbursed but not approved") that independent booleans could accidentally allow.
- `run_type` (Regular / Supplementary / Correction) — exists because a "Correction" run is structurally identical to a Regular one (same entries/components/approval flow) but semantically different (it only contains the disputed employee, and links back via `parent_run_id`, §18.4). Reusing the same table avoids a parallel "CorrectionRun" schema.
- `version` + `parent_run_id` (self-referencing FK) — this is how the "never edit, always version" rule (§2.4, §18.4) is implemented at the run level: a correction is a *new* row, not a mutation of the old one, and `parent_run_id` preserves the lineage for audit and reporting.
- `total_gross` / `total_deductions` / `total_net` are **stored**, not computed on the fly — a deliberate denormalization for the dashboard and webhook payloads (`payroll.approved` event, §22.1) which need these totals instantly without summing potentially thousands of `PayrollEntry` rows on every request.
- `filter_branch_id` / `filter_department_id` — supports partial/scoped runs (e.g. running payroll for one branch ahead of others), which the API explicitly allows (§15.4).

### `PAYROLL_ENTRY`
**Why it exists:** One row per employee per run — the actual calculated result. This is where the snapshot principle is most visible.
**Why these attributes:** Every "snapshot" field (`employee_name`, `department_name`, `bank_name`, `iban`, `base_salary`, `working_days`, `absent_days`, `late_days`) duplicates something already present elsewhere (`EmployeePayrollProfile`, `AttendanceSummary`) **on purpose** — per §2.4, once a run passes GENERATED, "its snapshots are frozen and immutable." If `PayrollEntry` merely pointed at `EmployeePayrollProfile` by FK, a later profile update (say, a bank account correction) would silently rewrite a salary slip from six months ago. `status` (Calculated/Disputed/Revised/Locked) tracks the entry's own lifecycle *within* disputes and correction runs, independent of the parent run's overall status.
**Why not just store the whole calculation as JSON on this row:** Because individual components need to be queried, reported on, and — critically — **manually overridden one at a time** (§15.7, `PATCH .../components/{componentId}`) with an audit trail per component. A JSON blob can't hold a per-field `IsManualOverride` flag or be joined against `SalaryComponent` for reporting (Module 18's Component Analysis report), so components get their own table.

### `PAYROLL_ENTRY_COMPONENT`
**Why it exists:** The line-item breakdown (Basic: 75,000, House Rent: 30,000, ...) shown on the salary slip (§17.3) and used for the Component Analysis / Deduction / Allowance reports (§23.1).
**Why `formula_used` is stored as text here, again:** This is the clearest example of the snapshot principle applied at the finest grain — even if `SalaryStructureComponent.FormulaExpression` changes next month, this row remembers *exactly* what expression produced *this* number, so a dispute investigation or audit can reconstruct the calculation years later without needing the (possibly since-modified or deleted) structure.
**Why `is_manual_override`:** HR is explicitly allowed to hand-edit a component before approval (§15.7); this flag is what distinguishes "the formula computed this" from "a human typed this number in," which matters enormously for audit (§24.3 explicitly calls out "manually overridden" as a must-audit action) and for trust in the system (a report can highlight all manually-touched entries for extra scrutiny).

---

## Module 11 — Approval Workflow

### `APPROVAL_WORKFLOW_TEMPLATE` / `APPROVAL_WORKFLOW_STEP`
**Why they exist:** Different companies need different sign-off chains — a small company might need one CEO approval, a large one needs HR → Finance → CEO (§16.3). Rather than hardcoding a fixed 2- or 3-step pipeline, the PRD stores the workflow *as configuration*, mirroring exactly the same "data, not code" philosophy applied to salary formulas.
**Why `RequiredRole` is a string tied to the RBAC roles, and `StepOrder` is a plain integer (not a linked-list of "next step"):** A simple ordered integer is sufficient because approval is strictly sequential (§16.5: "only the user with the role matching the current pending step... can approve") — there's no branching/parallel approval in v1, so a more complex graph structure would be unused complexity.
**Why `SLAHours` is optional:** Not every company wants SLA escalation notifications; making it nullable/optional avoids forcing a policy decision on tenants who don't need it.

### `PAYROLL_APPROVAL_RECORD`
**Why it exists:** The append-only ledger of every approve/reject/skip action taken against a run (§16.2) — this is what makes the approval chain auditable and disputable later ("who approved this and when").
**Why it snapshots `step_order`/`step_name`:** Same reasoning as everywhere else — if the workflow template is edited after a run has already gone through Step 2, the historical record of what "Step 2" was called at the time must not silently change.

---

## Module 12 — Salary Slip

### `SALARY_SLIP`
**Why it exists:** The final, employee-facing artifact — a generated PDF, tracked independently of the calculation that produced it (§17.1).
**Why `payroll_entry_id` is unique (1:1)** for the *current* slip, but the table also has a `version` column — because a slip can be **regenerated** after a dispute correction (§18.4: "new SalarySlip is generated, Version = 2... old SalarySlip is marked as superseded, not deleted"). The 1:1 uniqueness is on the *live* relationship; superseded versions are kept as historical rows rather than deleted, which is why version exists instead of simply overwriting the PDF URL.
**Why `is_locked` exists separately from the payroll run's own status:** A slip becomes uneditable once disbursed even if, hypothetically, some other part of the system still allowed touching the run — this is a belt-and-suspenders immutability flag scoped to the artifact the employee actually sees.
**Why `viewed_at`:** Supports read-receipt-style reporting/notifications ("has the employee seen their slip yet") without needing a separate access-log table for this one specific, high-value event.

---

## Module 13 — Dispute Management

### `DISPUTE_TICKET`
**Why it exists:** Employees need a formal, trackable channel to challenge a slip (§18.1) rather than an informal side-channel (email/phone) that leaves no trace and can't drive a correction workflow.
**Why it links to `payroll_entry_id` specifically (not just "employee + period"):** A dispute is against a *specific calculated result* — tying it to the exact entry means a later correction run can be generated referencing precisely what's being contested, even if the employee has since been re-synced with different profile data.
**Why `status` has a branching state machine (OPEN → UNDER_REVIEW → then either PAYROLL_CORRECTION_REQUIRED → CORRECTION_IN_PROGRESS → RESOLVED, or REJECTED, or RESOLVED directly, §18.3):** Because not every dispute needs a recalculation — some are resolved with just an explanation. Modeling this as one flexible status enum (rather than two separate booleans like `is_correction_needed` / `is_resolved`) keeps the valid-transitions logic centralized and preventable of invalid states like "resolved but still marked correction-in-progress."

### `DISPUTE_COMMENT`
**Why it's a separate table instead of a single `Description` field growing over time:** Disputes are conversational — HR, the employee, and possibly Finance go back and forth (§18.2). A one-to-many comment thread (with `author_role` captured per comment) supports this naturally and lets the UI render a timeline, which a single mutable text field could not.

---

## Module 14 — Disbursement & Bank Transfer

### `DISBURSEMENT_BATCH`
**Why it exists:** Bundles all of a run's net-salary transfers into one bank-file-generation event (§19.1) — since v1 explicitly does **not** integrate directly with bank APIs (§1.5 out-of-scope), this table exists to track the manual "generate file → upload to bank portal → confirm" workflow as its own lifecycle, distinct from `PayrollRun`'s lifecycle.
**Why `file_format` and `bank_file_url` live here, not on `PayrollRun`:** Because a single approved run could, in principle, need re-generation of the bank file (different format, or after a `PartiallyFailed` batch is corrected) without re-running payroll itself — keeping disbursement as its own entity decouples "calculating pay" from "moving money."

### `BANK_TRANSFER_RECORD`
**Why it's not just `PayrollEntry` + `Amount`:** Because a transfer has its **own** status independent of the payroll entry's status — a specific transfer can fail (wrong IBAN, closed account) and be marked `Failed`/`Reversed` (§19.5) *after* the payroll entry itself is long "Locked." Mixing these into one row would mean a bank-side failure could improperly reopen or contaminate the frozen payroll snapshot.

---

## Module 15 — Loan & Advance Management

### `EMPLOYEE_LOAN` / `LOAN_INSTALLMENT`
**Why split into two tables instead of one "Loan" row with an installment count:** Because each individual installment needs its own state — `Pending`/`Deducted`/`Skipped` — and a pointer to the *exact* `PayrollEntry` it was deducted in (`deducted_from_payroll_entry_id`, §20.1). This is essential for the "Loan Outstanding Report" (§23.1), which must show a month-by-month schedule, and for reconciling `EmployeeLoan.RemainingAmount` deterministically — a single-row loan couldn't answer "was March's installment actually deducted, and in which run?"
**Why `InstallmentAmount` is computed (`LoanAmount / TotalInstallments`) and stored rather than calculated on read:** Because once installments begin, the *original* per-installment figure must stay fixed even if, say, a partial early repayment changes `RemainingAmount` later — recomputing from current `RemainingAmount` would silently change what future installments deduct.

### `ADVANCE_SALARY_REQUEST`
**Why it's a separate entity from `EmployeeLoan` rather than "just a type of loan":** Advances and loans have meaningfully different lifecycles in the PRD — an advance starts as a `Pending` **request** requiring approval before any money moves (`RequestedAmount` vs `ApprovedAmount` can differ), whereas a Loan in this schema is modeled from the point of disbursement onward. Recovery fields (`RecoveryInstallments`, `RecoveryStartMonth/Year`, `RecoveryPerInstallment`) mirror the loan's installment concept but don't need a full child-table like `LOAN_INSTALLMENT` because advances are simpler (no interest, typically short recovery windows) — the PRD doesn't define an `AdvanceInstallment` table, reflecting that advances are treated as a lighter-weight process than loans.

---

## Module 16 — Bonus & Overtime

### `BONUS_RECORD`
**Why it exists as its own entity rather than just a manually-added `PayrollEntryComponent`:** A bonus needs to be **approved and queued** *before* a payroll run even exists for that period (§21.1: "Bonuses are included in payroll runs automatically when Status = Pending and the period matches"). It needs its own approval trail (`ApprovedBy`), its own status independent of any specific run (`Pending` → `IncludedInPayroll` → `Paid`), and to be retroactively traceable to whichever `PayrollEntry` eventually included it.
**Why `IsRecurring = false` on the underlying `SALARY_COMPONENT` (BONUS) rather than a totally separate code path:** So that bonuses still flow through the exact same formula/snapshot/reporting machinery as every other earning — the PRD favors reusing the general component pipeline over building bonus-specific calculation logic.

### `OVERTIME_POLICY`
**Why it's one row per company (1:1), not per employee or per structure:** Overtime multipliers (standard/weekend/holiday) are described in the PRD as a single company-wide policy (§21.3), not something that varies structure-by-structure — modeling it as 1:1 with `Company` avoids needless duplication across every salary structure a company has.

---

## Module 17 — Notifications & Events

### `NOTIFICATION_RECORD`
**Why it exists:** In-app notifications (§22.2) are a different concern from outbound webhooks (§22.1) or emails (§22.3) — they're read/unread inbox items for dashboard users. `reference_id` + `reference_type` (instead of several nullable FK columns, one per possible source) is a deliberate generic-reference pattern, chosen because notifications can point at many different entity types (a PayrollRun, a Dispute, a Slip) and adding a new notification type shouldn't require a schema migration to add yet another nullable FK column.
**Why webhooks aren't modeled as their own full entity in this ERD:** The PRD describes webhook *registration* (`POST /webhooks` with url/events/secret) but treats delivery as fire-and-forget HTTP calls rather than a queryable domain entity with reports/audits attached to it the way Notifications are — so it's represented here only implicitly (via the `WebhookDispatcherService` in the architecture, not as an ERD table), matching how lightly the PRD itself treats it.

---

## Module 19 — Audit Log

### `AUDIT_LOG`
**Why it exists:** The PRD's non-negotiable principle: "Everything is logged. Nothing is deleted." (§24.1). This is the single table that makes every other table's "soft delete only" / "immutable after approval" promises verifiable after the fact.
**Why `old_values` / `new_values` / `diff` are three separate JSON columns instead of one:** `old_values`/`new_values` preserve the full before/after state (needed to reconstruct an entity at any point in time), while `diff` is a **derived**, field-level summary kept alongside for fast UI rendering ("show me just what changed") without having to compute the diff of two JSON blobs on every read — a small storage cost traded for much cheaper reads, which matters because audit logs are read far more often (compliance review, dispute investigation) than written per-entity.
**Why it's implemented as a cross-cutting MediatR behavior (§24.4) rather than manual logging calls sprinkled through each handler:** So that *no* command can accidentally skip audit logging — centralizing it in the pipeline (like `ValidationBehavior` and `TenantIsolationBehavior`) makes "always audited" a structural guarantee instead of a discipline the team has to remember for every new feature.

---

## Authentication: `API_KEY`
**Why it exists:** Machine-to-machine auth (§5.1 Mode A) needs long-lived, revocable, tenant-scoped credentials distinct from human JWT sessions.
**Why `key_hash` (SHA-256) instead of storing the plaintext key:** Explicit security requirement (§27.3) — the plaintext is shown to the user exactly once at creation and never persisted, so that a database breach can't leak usable API keys, only unusable hashes (the same principle as password hashing).
**Why it's scoped to exactly one `company_id`, not a list of companies:** Enforces the strictest possible blast radius — a compromised key can only ever act on one tenant's data (§5.1: "a School ERP's API key can only access COMP-001 data"), which is far safer than a key that could theoretically be granted multi-tenant access.

---

## A few explicit "why NOT" design decisions worth calling out directly

- **Why not one giant `Transactions` table for all money movement (loans, advances, bonuses, deductions) instead of five separate tables?**
 Because each has a genuinely different lifecycle, approval path, and set of report filters (Loan Outstanding Report vs. Bonus reporting vs. Deduction Report, §23.1). A single polymorphic table would need a `type` discriminator and a pile of nullable columns that only apply to some types — the PRD instead keeps each concept's shape explicit, at the cost of a few more tables.
- **Why not store salary formulas as compiled code (e.g., a C# expression tree) instead of text?**
 Security and safety (§10.7) — NCalc text expressions run in a sandboxed evaluator with an explicit function whitelist (`Abs, Round, Floor, Ceiling, Min, Max, If`) and no reflection or file I/O. Compiled code would reopen the arbitrary-code-execution risk the PRD explicitly designs around.
- **Why keep `PayrollEntry` and `PayrollEntryComponent` as two tables instead of one wide table with a column per possible component?**
 Because the set of components is **tenant-defined** (Module 03) — one company might have 8 components, another 15. A fixed-column table can't represent a variable, per-tenant schema; an entry/line-item pair can.
- **Why is almost nothing hard-deleted anywhere in this schema?**
 Payroll is a financial and legal record. §27.4 mandates soft deletes only and immutability post-approval — this is why you see `IsActive`/`IsDeleted`-style soft flags and versioning (History tables, `PayrollRun.Version`, `SalarySlip.Version`) instead of `DELETE` statements throughout the schema.

---

*Diagram file: `payrollms_erd.mermaid` — open alongside this document; entity names in the diagram match the `SCREAMING_SNAKE_CASE` headers used above.*
