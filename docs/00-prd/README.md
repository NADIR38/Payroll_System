# PayrollMS — Enterprise Payroll Microservice

## Product Requirements Document (PRD) v1.0

**Document Status:** Final Draft  
**Version:** 1.0.0  
**Last Updated:** July 2025  
**Audience:** Backend Engineers, Frontend Engineers, QA, DevOps

---

## Table of Contents

1. [Executive Summary](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#1-executive-summary)
2. [System Context & Architecture](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#2-system-context--architecture)
3. [Technical Stack](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#3-technical-stack)
4. [Project Structure](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#4-project-structure)
5. [Authentication & Authorization](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#5-authentication--authorization)
6. [Module 01 — Tenant & Company Management](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#6-module-01--tenant--company-management)
7. [Module 02 — Employee Payroll Profile](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#7-module-02--employee-payroll-profile)
8. [Module 03 — Salary Components](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#8-module-03--salary-components)
9. [Module 04 — Salary Structures](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#9-module-04--salary-structures)
10. [Module 05 — Formula Engine (NCalc)](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#10-module-05--formula-engine-ncalc)
11. [Module 06 — Allowance Engine](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#11-module-06--allowance-engine)
12. [Module 07 — Deduction Engine](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#12-module-07--deduction-engine)
13. [Module 08 — Attendance Integration](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#13-module-08--attendance-integration)
14. [Module 09 — Leave Integration](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#14-module-09--leave-integration)
15. [Module 10 — Payroll Run Engine](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#15-module-10--payroll-run-engine)
16. [Module 11 — Approval Workflow](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#16-module-11--approval-workflow)
17. [Module 12 — Salary Slip](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#17-module-12--salary-slip)
18. [Module 13 — Dispute Management](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#18-module-13--dispute-management)
19. [Module 14 — Disbursement & Bank Transfer](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#19-module-14--disbursement--bank-transfer)
20. [Module 15 — Loan & Advance Management](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#20-module-15--loan--advance-management)
21. [Module 16 — Bonus & Overtime](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#21-module-16--bonus--overtime)
22. [Module 17 — Notifications & Events](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#22-module-17--notifications--events)
23. [Module 18 — Reports & BI](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#23-module-18--reports--bi)
24. [Module 19 — Audit Log](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#24-module-19--audit-log)
25. [External API Contract](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#25-external-api-contract)
26. [Database Schema (ERD Reference)](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#26-database-schema-erd-reference)
27. [Non-Functional Requirements](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#27-non-functional-requirements)
28. [Error Handling Standard](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#28-error-handling-standard)
29. [Development Phases & Milestones](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#29-development-phases--milestones)
30. [Glossary](https://claude.ai/chat/6a833266-7bc5-433d-ac35-e1c1b707b989#30-glossary)

---

## 1. Executive Summary

### 1.1 Product Name

**PayrollMS** — Enterprise Payroll Microservice

### 1.2 Purpose

PayrollMS is a standalone, multi-tenant payroll microservice designed to be integrated into any ERP system (School ERP, Hospital ERP, HR platforms) via REST API. It owns salary calculation, payroll generation, approval workflows, salary slip creation, dispute resolution, and bank disbursement — nothing else.

### 1.3 What PayrollMS Does NOT Own

- Employee master records (owned by the calling ERP)
- Attendance raw data (consumed, not owned)
- Leave records (consumed summaries only)
- Authentication of end users (delegated to calling ERP or Supabase Auth)

### 1.4 Core Value Proposition

- Drop any ERP into this system via API and get a fully functioning payroll engine
- Formula-driven salary calculations — no hardcoded business logic
- Full audit trail — nothing is ever deleted or overwritten
- Multi-tenant from day one — complete data isolation per company

### 1.5 Version 1 Scope

- Multi-tenant company setup
- Employee payroll profile management (snapshot model)
- Formula-based salary structures using NCalc
- Attendance + leave data ingestion
- Bulk payroll run generation
- Configurable multi-step approval workflows
- PDF salary slips with in-app viewer and email delivery
- Salary dispute management with revision versioning
- Bank disbursement via CSV/Excel export
- Loan and advance salary management
- Bonus and overtime modules
- Full BI reporting — Excel + PDF export for all modules
- Webhook outbound events for downstream consumers

**Out of scope for v1:** Tax slabs (FBR/EOBI), direct bank API integration, mobile app, payroll simulation/dry-run UI.

---

## 2. System Context & Architecture

### 2.1 High-Level Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                    EXTERNAL SYSTEMS                               │
│                                                                   │
│   School ERP          Hospital ERP         Any Future ERP        │
│   (employees,         (employees,          (employees,           │
│    attendance,         attendance,          attendance,           │
│    leave)              leave)               leave)               │
└─────────────────────────┬────────────────────────────────────────┘
                          │
                 REST API / Webhooks
                  (machine-to-machine)
                          │
┌─────────────────────────▼────────────────────────────────────────┐
│                     API GATEWAY                                   │
│              (Rate limiting, Auth validation,                     │
│               Routing, API versioning)                            │
└──────┬──────────────┬──────────────┬────────────────┬────────────┘
       │              │              │                │
       ▼              ▼              ▼                ▼
┌──────────┐  ┌──────────────┐  ┌──────────┐  ┌──────────────┐
│ Tenant & │  │   Payroll    │  │ Approval │  │ Disbursement │
│ Company  │  │   Engine     │  │Workflow  │  │   Module     │
│ Service  │  │   Service    │  │ Service  │  │   Service    │
└──────────┘  └──────────────┘  └──────────┘  └──────────────┘
       │              │              │                │
       └──────────────┴──────────────┴────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │   PostgreSQL DB   │
                    │   (Supabase)      │
                    └─────────┬─────────┘
                              │
              ┌───────────────┴───────────────┐
              │                               │
    ┌─────────▼─────────┐         ┌──────────▼──────────┐
    │  Supabase Storage │         │   Message Bus /      │
    │  (PDF slips,      │         │   Webhook Dispatcher │
    │   bank exports)   │         │   (outbound events)  │
    └───────────────────┘         └─────────────────────┘
```

### 2.2 Integration Model

PayrollMS operates as a **pure consumer + calculator**:

```
External System                    PayrollMS
──────────────                     ─────────────────────────────────
POST /api/v1/employees/sync    →   Creates/updates EmployeePayrollProfile
POST /api/v1/attendance/sync   →   Stores AttendanceSummary snapshot
POST /api/v1/leave/sync        →   Stores LeaveSummary snapshot
POST /api/v1/payroll/run       →   Triggers PayrollRun for a month
GET  /api/v1/slips/{id}/pdf    ←   Returns salary slip PDF
POST /api/v1/disputes          →   Employee raises a dispute
Webhook → external system      ←   payroll.approved / payroll.disbursed
```

### 2.3 Data Isolation Model

Every database table includes `company_id` (tenant identifier). Row-Level Security (RLS) is enforced at the PostgreSQL level via Supabase RLS policies. No cross-tenant data access is possible at the database layer.

### 2.4 Snapshot Principle

When a PayrollRun is executed, the system takes immutable snapshots of:

- Employee name, bank account, IBAN (from `EmployeePayrollProfile`)
- Salary structure components and formulas (from `SalaryStructureSnapshot`)
- Attendance summary for that period (from `AttendanceSummary`)
- Leave summary for that period (from `LeaveSummary`)

**Once a PayrollRun moves past GENERATED status, its snapshots are frozen and immutable.** Corrections produce a new versioned run, never an edit to the old one.

---

## 3. Technical Stack

### 3.1 Backend

|Concern|Technology|Rationale|
|---|---|---|
|Framework|ASP.NET Core 9 Web API|Strong typing, mature ecosystem, performance|
|Architecture|Clean Architecture (Domain → Application → Infrastructure → API)|Separation of concerns, testability|
|Pattern|CQRS + MediatR|Clear command/query separation, pipeline behaviors|
|Validation|FluentValidation|Declarative, testable validation rules|
|ORM|Entity Framework Core 9 + Npgsql|PostgreSQL support, LINQ queries|
|Formula Engine|NCalc2|Safe expression evaluation, no code injection risk|
|PDF Generation|QuestPDF|MIT license, C#-native, production grade|
|Background Jobs|Hangfire (PostgreSQL storage)|Bulk payroll runs, scheduled jobs|
|Email|MailKit + SMTP or Resend API|Salary slip delivery|
|Excel Export|ClosedXML|Bank transfer files, BI reports|
|Logging|Serilog → Supabase / file sink|Structured logging|
|API Docs|Swagger / Scalar UI|Developer-friendly API exploration|

### 3.2 Frontend

|Concern|Technology|
|---|---|
|Framework|Next.js 15 (App Router)|
|Language|TypeScript|
|Styling|Tailwind CSS|
|Data Fetching|TanStack Query v5|
|Forms|React Hook Form + Zod|
|PDF Viewer|react-pdf|
|Tables|TanStack Table v8|
|Charts|Recharts|
|Excel Export|SheetJS (client-side)|

### 3.3 Infrastructure

|Concern|Service|Free Tier|
|---|---|---|
|Database|Supabase PostgreSQL|500MB, unlimited API calls|
|File Storage|Supabase Storage|1GB|
|Backend Hosting|Railway|512MB RAM, $5 credit/month|
|Frontend Hosting|Vercel|Generous free tier|
|Email|Resend|3,000 emails/month free|
|Background Jobs|Hangfire on same Railway instance|Uses existing PostgreSQL|

### 3.4 Authentication

Two modes operate simultaneously:

**Mode A — Machine-to-Machine (ERP Integration)**

- ERP systems authenticate via API Key sent in `X-Api-Key` header
- API Keys are scoped to a `company_id` and stored hashed in the database
- Permissions are role-based, scoped to the tenant

**Mode B — Human Dashboard Login**

- Supabase Auth (email/password or SSO)
- JWT issued by Supabase, validated in ASP.NET Core middleware
- User's `company_id` is embedded in JWT claims
- Role assigned per user per company in `UserCompanyRole` table

---

## 4. Project Structure

### 4.1 Backend Solution Structure

```
PayrollMS/
├── src/
│   ├── PayrollMS.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs              ← Id, CreatedAt, UpdatedAt, IsDeleted
│   │   │   ├── BaseAuditableEntity.cs     ← + CreatedBy, UpdatedBy
│   │   │   └── ValueObjects/
│   │   │       ├── Money.cs
│   │   │       └── DateRange.cs
│   │   ├── Entities/
│   │   │   ├── Tenant/
│   │   │   │   ├── Company.cs
│   │   │   │   ├── Branch.cs
│   │   │   │   ├── Department.cs
│   │   │   │   ├── Designation.cs
│   │   │   │   ├── CostCenter.cs
│   │   │   │   ├── FinancialYear.cs
│   │   │   │   └── PayrollCalendar.cs
│   │   │   ├── Employee/
│   │   │   │   ├── EmployeePayrollProfile.cs
│   │   │   │   ├── EmployeePayrollProfileHistory.cs
│   │   │   │   └── EmployeeBankAccount.cs
│   │   │   ├── Salary/
│   │   │   │   ├── SalaryComponent.cs
│   │   │   │   ├── SalaryStructure.cs
│   │   │   │   ├── SalaryStructureComponent.cs
│   │   │   │   ├── AllowanceRule.cs
│   │   │   │   └── DeductionRule.cs
│   │   │   ├── Payroll/
│   │   │   │   ├── PayrollRun.cs
│   │   │   │   ├── PayrollEntry.cs
│   │   │   │   ├── PayrollEntryComponent.cs
│   │   │   │   ├── PayrollSnapshot.cs
│   │   │   │   └── AttendanceSummary.cs
│   │   │   ├── Approval/
│   │   │   │   ├── ApprovalWorkflowTemplate.cs
│   │   │   │   ├── ApprovalWorkflowStep.cs
│   │   │   │   └── PayrollApprovalRecord.cs
│   │   │   ├── Slip/
│   │   │   │   └── SalarySlip.cs
│   │   │   ├── Dispute/
│   │   │   │   ├── DisputeTicket.cs
│   │   │   │   └── DisputeComment.cs
│   │   │   ├── Disbursement/
│   │   │   │   ├── DisbursementBatch.cs
│   │   │   │   └── BankTransferRecord.cs
│   │   │   ├── Loan/
│   │   │   │   ├── EmployeeLoan.cs
│   │   │   │   └── LoanInstallment.cs
│   │   │   ├── Advance/
│   │   │   │   └── AdvanceSalaryRequest.cs
│   │   │   └── Bonus/
│   │   │       └── BonusRecord.cs
│   │   ├── Interfaces/
│   │   │   ├── Repositories/              ← one interface per entity
│   │   │   └── Services/
│   │   │       ├── IFormulaEvaluator.cs
│   │   │       ├── IPayrollCalculator.cs
│   │   │       └── IPdfGenerator.cs
│   │   ├── Exceptions/
│   │   │   ├── AppException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   ├── ValidationException.cs
│   │   │   ├── ConflictException.cs
│   │   │   ├── UnauthorizedException.cs
│   │   │   └── PayrollFrozenException.cs
│   │   └── Enums/
│   │       ├── PayrollRunStatus.cs
│   │       ├── ComponentType.cs
│   │       ├── AllowanceApplication.cs
│   │       ├── DeductionType.cs
│   │       ├── DisputeStatus.cs
│   │       └── DisbursementStatus.cs
│   │
│   ├── PayrollMS.Application/
│   │   ├── Features/
│   │   │   ├── Companies/
│   │   │   │   ├── Commands/
│   │   │   │   ├── Queries/
│   │   │   │   └── DTOs/
│   │   │   ├── Employees/
│   │   │   ├── SalaryComponents/
│   │   │   ├── SalaryStructures/
│   │   │   ├── PayrollRuns/
│   │   │   ├── Approvals/
│   │   │   ├── SalarySlips/
│   │   │   ├── Disputes/
│   │   │   ├── Disbursements/
│   │   │   ├── Loans/
│   │   │   ├── Advances/
│   │   │   ├── Bonuses/
│   │   │   └── Reports/
│   │   ├── Behaviors/
│   │   │   ├── LoggingBehavior.cs
│   │   │   ├── ValidationBehavior.cs
│   │   │   └── TenantIsolationBehavior.cs
│   │   ├── Interfaces/
│   │   │   ├── IUnitOfWork.cs
│   │   │   ├── ICurrentTenant.cs
│   │   │   ├── IEmailService.cs
│   │   │   ├── IStorageService.cs
│   │   │   └── IWebhookDispatcher.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── PayrollMS.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/            ← EF Core IEntityTypeConfiguration<T>
│   │   │   ├── Repositories/
│   │   │   ├── Migrations/
│   │   │   └── UnitOfWork.cs
│   │   ├── Services/
│   │   │   ├── FormulaEvaluatorService.cs ← NCalc implementation
│   │   │   ├── PayrollCalculatorService.cs
│   │   │   ├── PdfGeneratorService.cs     ← QuestPDF
│   │   │   ├── EmailService.cs            ← MailKit / Resend
│   │   │   ├── StorageService.cs          ← Supabase Storage
│   │   │   └── WebhookDispatcherService.cs
│   │   ├── Workers/
│   │   │   └── PayrollGenerationWorker.cs ← Hangfire background job
│   │   └── DependencyInjection.cs
│   │
│   └── PayrollMS.API/
│       ├── Controllers/
│       │   ├── v1/
│       │   │   ├── CompaniesController.cs
│       │   │   ├── EmployeesController.cs
│       │   │   ├── SalaryComponentsController.cs
│       │   │   ├── SalaryStructuresController.cs
│       │   │   ├── AttendanceController.cs
│       │   │   ├── LeaveController.cs
│       │   │   ├── PayrollRunsController.cs
│       │   │   ├── ApprovalsController.cs
│       │   │   ├── SalarySlipsController.cs
│       │   │   ├── DisputesController.cs
│       │   │   ├── DisbursementsController.cs
│       │   │   ├── LoansController.cs
│       │   │   ├── AdvancesController.cs
│       │   │   ├── BonusesController.cs
│       │   │   └── ReportsController.cs
│       ├── Middleware/
│       │   ├── GlobalExceptionMiddleware.cs
│       │   ├── TenantResolutionMiddleware.cs
│       │   └── ApiKeyAuthMiddleware.cs
│       ├── Extensions/
│       │   ├── SwaggerExtensions.cs
│       │   └── AuthExtensions.cs
│       └── Program.cs
│
└── tests/
    ├── PayrollMS.Domain.Tests/
    ├── PayrollMS.Application.Tests/
    └── PayrollMS.Integration.Tests/
```

### 4.2 Frontend Structure

```
payrollms-web/
├── app/
│   ├── (auth)/
│   │   ├── login/
│   │   └── layout.tsx
│   ├── (dashboard)/
│   │   ├── layout.tsx
│   │   ├── companies/
│   │   ├── employees/
│   │   ├── salary-structures/
│   │   ├── payroll/
│   │   │   ├── runs/
│   │   │   ├── approvals/
│   │   │   └── slips/
│   │   ├── disputes/
│   │   ├── disbursements/
│   │   ├── loans/
│   │   └── reports/
├── components/
│   ├── ui/                    ← shadcn/ui base components
│   ├── payroll/               ← domain-specific components
│   └── reports/
├── lib/
│   ├── api/                   ← typed API client functions
│   ├── hooks/                 ← TanStack Query hooks
│   └── utils/
└── types/                     ← TypeScript interfaces matching API DTOs
```

---

## 5. Authentication & Authorization

### 5.1 Dual Authentication Modes

#### Mode A — API Key (Machine-to-Machine)

Used by external ERP systems to push data into PayrollMS.

```
Request Header:
X-Api-Key: pk_live_xxxxxxxxxxxxxxxxxxxxxxxx
X-Company-Id: COMP-001
```

API Key resolution flow:

1. `ApiKeyAuthMiddleware` intercepts requests on `/api/v1/` routes
2. Looks up hashed key in `ApiKeys` table, validates `company_id` binding
3. Sets `ICurrentTenant` with resolved `companyId`
4. Sets claims principal with `Role = ApiClient`

API Keys are scoped — a School ERP's API key can only access `COMP-001` data.

```sql
CREATE TABLE api_keys (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  company_id UUID NOT NULL REFERENCES companies(id),
  key_hash TEXT NOT NULL UNIQUE,    -- SHA-256 hash of the actual key
  label TEXT NOT NULL,              -- "School ERP Production"
  is_active BOOLEAN DEFAULT TRUE,
  expires_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ DEFAULT NOW()
);
```

#### Mode B — JWT (Human Dashboard)

Used by HR managers, Finance officers, Payroll Officers logging into the dashboard.

```
Request Header:
Authorization: Bearer <supabase_jwt>
```

JWT validation flow:

1. Supabase issues JWT on login
2. ASP.NET Core validates JWT signature using Supabase JWT secret
3. `TenantResolutionMiddleware` extracts `company_id` from JWT claims
4. `UserCompanyRole` table provides the user's role within that company

### 5.2 Role-Based Access Control (RBAC)

#### Roles

|Role|Description|
|---|---|
|`SuperAdmin`|Platform-level access, can manage all companies|
|`CompanyAdmin`|Full access within their company|
|`HRManager`|Generate payroll, manage employee profiles, approve (HR step)|
|`FinanceManager`|Finance approval, disbursement authorization|
|`PayrollOfficer`|Generate payroll, edit slips, manage disputes|
|`Auditor`|Read-only access to everything including audit logs|
|`Employee`|View own salary slip, raise disputes|
|`ApiClient`|Machine-to-machine, limited to sync endpoints|

#### Permission Matrix

|Action|SuperAdmin|CompanyAdmin|HRManager|FinanceManager|PayrollOfficer|Auditor|Employee|
|---|---|---|---|---|---|---|---|
|Manage Companies|✅|❌|❌|❌|❌|❌|❌|
|Sync Employees|✅|✅|✅|❌|❌|❌|❌|
|Manage Salary Structures|✅|✅|✅|❌|❌|❌|❌|
|Generate Payroll Run|✅|✅|✅|❌|✅|❌|❌|
|Approve (HR Step)|✅|✅|✅|❌|❌|❌|❌|
|Approve (Finance Step)|✅|✅|❌|✅|❌|❌|❌|
|View Any Salary Slip|✅|✅|✅|✅|✅|✅|❌|
|View Own Salary Slip|✅|✅|✅|✅|✅|✅|✅|
|Raise Dispute|✅|✅|✅|✅|✅|❌|✅|
|Resolve Dispute|✅|✅|✅|❌|✅|❌|❌|
|Authorize Disbursement|✅|✅|❌|✅|❌|❌|❌|
|Export Reports|✅|✅|✅|✅|✅|✅|❌|
|View Audit Logs|✅|✅|❌|❌|❌|✅|❌|

### 5.3 Multi-Tenant Isolation Enforcement

Three layers of isolation:

1. **Middleware Layer** — `TenantResolutionMiddleware` sets `ICurrentTenant.CompanyId` on every request
2. **Application Layer** — `TenantIsolationBehavior` (MediatR pipeline behavior) injects `companyId` into every command/query before the handler executes
3. **Database Layer** — Supabase Row Level Security policies on all tables:

```sql
-- Example RLS policy on payroll_runs table
CREATE POLICY "tenant_isolation" ON payroll_runs
  USING (company_id = current_setting('app.current_company_id')::UUID);
```

---

## 6. Module 01 — Tenant & Company Management

### 6.1 Purpose

Manage multi-tenant company hierarchy. Every other entity belongs to a company.

### 6.2 Entities

#### Company

```
Company
├── Id (UUID)
├── Name (string, required)
├── Code (string, unique per platform)
├── LogoUrl (string, nullable)
├── Address (string)
├── ContactEmail (string)
├── ContactPhone (string)
├── IsActive (bool)
├── CreatedAt (DateTimeOffset)
└── UpdatedAt (DateTimeOffset)
```

#### Branch

```
Branch
├── Id (UUID)
├── CompanyId (UUID FK)
├── Name (string)
├── Code (string, unique per company)
├── Address (string)
└── IsActive (bool)
```

#### Department

```
Department
├── Id (UUID)
├── CompanyId (UUID FK)
├── BranchId (UUID FK, nullable)
├── Name (string)
├── Code (string, unique per company)
└── IsActive (bool)
```

#### Designation

```
Designation
├── Id (UUID)
├── CompanyId (UUID FK)
├── Name (string)
├── Code (string)
├── Grade (string, nullable)       ← e.g. BPS-17, Grade-4
└── IsActive (bool)
```

#### CostCenter

```
CostCenter
├── Id (UUID)
├── CompanyId (UUID FK)
├── Name (string)
├── Code (string)
└── IsActive (bool)
```

#### FinancialYear

```
FinancialYear
├── Id (UUID)
├── CompanyId (UUID FK)
├── Label (string)                 ← "FY 2024-25"
├── StartDate (DateOnly)
├── EndDate (DateOnly)
└── IsCurrent (bool)
```

#### PayrollCalendar

```
PayrollCalendar
├── Id (UUID)
├── CompanyId (UUID FK)
├── FinancialYearId (UUID FK)
├── Month (int)                    ← 1-12
├── Year (int)
├── PayrollFreezeDate (DateOnly)   ← after this, no edits allowed
├── PaymentDate (DateOnly)         ← target disbursement date
├── WorkingDays (int)              ← official working days for this month
├── Holidays (JSON)                ← [{date, name}]
└── Status (enum: Open, Frozen, Closed)
```

### 6.3 API Endpoints

```
POST   /api/v1/companies                   Create company (SuperAdmin only)
GET    /api/v1/companies/{id}              Get company details
PUT    /api/v1/companies/{id}              Update company
POST   /api/v1/companies/{id}/branches     Create branch
GET    /api/v1/companies/{id}/branches     List branches
POST   /api/v1/companies/{id}/departments  Create department
POST   /api/v1/companies/{id}/designations Create designation
POST   /api/v1/companies/{id}/cost-centers Create cost center
POST   /api/v1/companies/{id}/financial-years        Create financial year
POST   /api/v1/companies/{id}/payroll-calendar       Set payroll calendar for month
GET    /api/v1/companies/{id}/payroll-calendar/{year}/{month}
```

### 6.4 Business Rules

- A company must have at least one branch and department before employees can be synced
- `PayrollFreezeDate` — once this date passes, the payroll calendar month is frozen; no new payroll runs can be created or edited for that period
- `WorkingDays` on PayrollCalendar is the authoritative working day count used by the deduction and allowance engines; attendance summaries must be reconciled against this
- Only one `FinancialYear` can have `IsCurrent = true` per company at a time

---

## 7. Module 02 — Employee Payroll Profile

### 7.1 Purpose

Store the minimal employee snapshot needed for payroll calculation. Does not duplicate HR data beyond what is required.

### 7.2 Entities

#### EmployeePayrollProfile (Current Snapshot)

```
EmployeePayrollProfile
├── Id (UUID)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)    ← ID from the calling ERP system
├── EmployeeCode (string)          ← display code, e.g. EMP-1001
├── FullName (string)              ← snapshot at sync time
├── BranchId (UUID FK)
├── BranchName (string)            ← snapshot label
├── DepartmentId (UUID FK)
├── DepartmentName (string)        ← snapshot label
├── DesignationId (UUID FK)
├── DesignationName (string)       ← snapshot label
├── CostCenterId (UUID FK, nullable)
├── SalaryStructureId (UUID FK)
├── BaseSalary (decimal)           ← individual override; plugged into formula as {BaseSalary}
├── AttendanceDeductionOptIn (bool)← employee choice: deduct on absence or not
├── JoiningDate (DateOnly)         ← for proration on first month
├── LeavingDate (DateOnly, nullable)
├── Status (enum: Active, Inactive, Terminated)
├── EffectiveFrom (DateOnly)       ← when this version became active
└── CreatedAt (DateTimeOffset)
```

#### EmployeePayrollProfileHistory

```
EmployeePayrollProfileHistory
├── Id (UUID)
├── EmployeePayrollProfileId (UUID FK)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)
├── EmployeeCode (string)
├── FullName (string)
├── SalaryStructureId (UUID FK)
├── BaseSalary (decimal)
├── EffectiveFrom (DateOnly)
├── EffectiveTo (DateOnly, nullable)   ← null = this is the current version
├── ChangedBy (string)
├── ChangeReason (string)
└── CreatedAt (DateTimeOffset)
```

#### EmployeeBankAccount

```
EmployeeBankAccount
├── Id (UUID)
├── EmployeePayrollProfileId (UUID FK)
├── CompanyId (UUID FK)
├── BankName (string)
├── AccountTitle (string)
├── AccountNumber (string)
├── IBAN (string)
├── BranchCode (string, nullable)
├── IsPrimary (bool)
└── IsActive (bool)
```

### 7.3 Sync Contract (External ERP → PayrollMS)

External systems push employee data via this request:

```json
POST /api/v1/employees/sync
X-Api-Key: pk_live_xxx
X-Company-Id: COMP-001

{
  "externalEmployeeId": "EMP-1001",
  "employeeCode": "EMP-1001",
  "fullName": "Ahmed Raza",
  "branchId": "uuid-branch",
  "departmentId": "uuid-dept",
  "designationId": "uuid-designation",
  "costCenterId": "uuid-costcenter",
  "salaryStructureId": "uuid-structure",
  "baseSalary": 75000.00,
  "joiningDate": "2023-01-15",
  "status": "Active",
  "attendanceDeductionOptIn": true,
  "bankAccount": {
    "bankName": "HBL",
    "accountTitle": "Ahmed Raza",
    "accountNumber": "12345678901",
    "iban": "PK36HABB0000000123456702",
    "isPrimary": true
  }
}
```

Response:

```json
{
  "success": true,
  "profileId": "uuid-profile",
  "action": "Created" // or "Updated"
}
```

### 7.4 Business Rules

- `ExternalEmployeeId + CompanyId` is the unique identifier — upsert on sync
- On every update, the previous record is written to `EmployeePayrollProfileHistory` with `EffectiveTo = today`
- A new `EmployeePayrollProfileHistory` record is created with `EffectiveFrom = today`, `EffectiveTo = null`
- Payroll generation uses the profile version that was active **on the payroll period's start date**, not the current version
- An employee cannot be synced to a `SalaryStructureId` that does not belong to their `CompanyId`
- `BaseSalary` is the individual's actual base — it is passed as the `{BaseSalary}` variable into the NCalc formula engine

---

## 8. Module 03 — Salary Components

### 8.1 Purpose

Define the atomic building blocks of any salary structure. Components are reusable across structures.

### 8.2 Entity

```
SalaryComponent
├── Id (UUID)
├── CompanyId (UUID FK)
├── Name (string)                  ← "Basic Salary", "Medical Allowance"
├── Code (string, unique per company) ← "BASIC", "MEDICAL", "TRANSPORT"
├── Type (enum)                    ← Earning | Deduction | Employer Contribution
├── CalculationMethod (enum)       ← Fixed | PercentageOfBase | FormulaExpression | PerDay | PerHour
├── DefaultValue (decimal, nullable)   ← used if FormulaExpression is empty
├── IsTaxable (bool)
├── IsRecurring (bool)             ← false = one-time (bonus, etc.)
├── IsOptional (bool)              ← true = not auto-applied, assigned manually per employee
├── IsActive (bool)
└── Description (string, nullable)
```

### 8.3 Component Type Enum

```
ComponentType:
  Earning               ← adds to gross salary
  Deduction             ← subtracts from gross salary
  EmployerContribution  ← employer's share (EOBI future), not in net salary
```

### 8.4 Calculation Method Enum

```
CalculationMethod:
  Fixed                 ← static amount, e.g. Medical = 3000
  PercentageOfBase      ← e.g. HouseRent = 40% of BaseSalary
  FormulaExpression     ← full NCalc expression, e.g. "BaseSalary * 0.4"
  PerWorkingDay         ← daily rate × working days, e.g. TravelAllowance = 250 * WorkingDays
  PerHour               ← hourly rate × hours, used for overtime
```

### 8.5 Predefined Component Codes (Recommended Seed Data)

```
Code            Name                    Type        Method
BASIC           Basic Salary            Earning     Fixed (= BaseSalary)
HOUSE_RENT      House Rent Allowance    Earning     PercentageOfBase
MEDICAL         Medical Allowance       Earning     Fixed
TRANSPORT       Transport Allowance     Earning     Fixed
TRAVEL          Travel Allowance        Earning     PerWorkingDay
FUEL            Fuel Allowance          Earning     Fixed
INTERNET        Internet Allowance      Earning     Fixed
UTILITY         Utility Allowance       Earning     Fixed
OVERTIME        Overtime                Earning     PerHour
BONUS           Bonus                   Earning     Fixed (not recurring)
LATE_DEDUCT     Late Deduction          Deduction   FormulaExpression
ABSENT_DEDUCT   Absence Deduction       Deduction   FormulaExpression
LOAN_DEDUCT     Loan Recovery           Deduction   Fixed (per installment)
ADVANCE_DEDUCT  Advance Recovery        Deduction   Fixed (per installment)
```

### 8.6 API Endpoints

```
POST   /api/v1/salary-components           Create component
GET    /api/v1/salary-components           List all (with filters)
GET    /api/v1/salary-components/{id}
PUT    /api/v1/salary-components/{id}
DELETE /api/v1/salary-components/{id}      Soft delete only
```

---

## 9. Module 04 — Salary Structures

### 9.1 Purpose

Group salary components with their formulas and rules into a reusable template that can be assigned to employees.

### 9.2 Entities

#### SalaryStructure

```
SalaryStructure
├── Id (UUID)
├── CompanyId (UUID FK)
├── Name (string)                   ← "Teacher Structure", "Driver Structure"
├── Code (string, unique per company)
├── Description (string, nullable)
├── EffectiveFrom (DateOnly)        ← structure applies from this date
├── EffectiveTo (DateOnly, nullable)
├── IsActive (bool)
└── Components → [SalaryStructureComponent]
```

#### SalaryStructureComponent

```
SalaryStructureComponent
├── Id (UUID)
├── SalaryStructureId (UUID FK)
├── SalaryComponentId (UUID FK)
├── FormulaExpression (string)      ← NCalc expression, e.g. "BaseSalary * 0.4"
├── FixedAmount (decimal, nullable) ← used if CalculationMethod = Fixed
├── Sequence (int)                  ← evaluation order (lower = evaluated first)
├── IsActive (bool)
└── Conditions → [AllowanceRule]
```

### 9.3 Example Structure Definition

**Teacher Salary Structure:**

```
Component       Sequence  Formula / Value
─────────────────────────────────────────────────────────
Basic           1         {BaseSalary}
House Rent      2         BaseSalary * 0.40
Medical         3         3000
Transport       4         2000
Travel          5         WorkingDays * 250    (working days only)
Late Deduction  6         LateDays * 200       (if opted in)
Absent Deduct   7         (BaseSalary / CalendarWorkingDays) * AbsentDays (if opted in)
Loan Recovery   8         {LoanInstallmentAmount}  (if active loan)
```

### 9.4 Business Rules

- Sequence determines formula evaluation order — critical for dependencies (HouseRent depends on Basic, so Basic must be sequence 1)
- A component's `FormulaExpression` can reference any component that has a lower sequence number — circular dependency is a validation error
- When `EffectiveFrom` changes, a new version of the structure should be created rather than editing the existing one
- Employees keep their `SalaryStructureId` pointing to the current version; payroll generation selects the structure version active on the period start date

---

## 10. Module 05 — Formula Engine (NCalc)

### 10.1 Purpose

Evaluate salary component formulas safely at runtime without code changes. This is the core computational engine of PayrollMS.

### 10.2 Interface

```csharp
// Domain/Interfaces/Services/IFormulaEvaluator.cs
public interface IFormulaEvaluator
{
    /// <summary>
    /// Evaluates an NCalc expression against a variable context.
    /// Returns the computed decimal value.
    /// </summary>
    decimal Evaluate(string expression, FormulaContext context);

    /// <summary>
    /// Validates an expression for syntax errors and undefined variables.
    /// Returns a list of validation errors (empty = valid).
    /// </summary>
    IReadOnlyList<string> Validate(string expression, IEnumerable<string> knownVariables);
}
```

### 10.3 Formula Context — Variable Binding

Every payroll calculation receives a `FormulaContext` populated before evaluation:

```csharp
public class FormulaContext
{
    // Employee variables
    public decimal BaseSalary { get; set; }
    public decimal GrossSalary { get; set; }           // running total of earnings so far

    // Calendar variables
    public int CalendarWorkingDays { get; set; }       // from PayrollCalendar
    public int WorkingDays { get; set; }               // actual days employee worked
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }                  // days employee was late
    public int LateMinutes { get; set; }               // total late minutes
    public int OvertimeHours { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public int HalfDays { get; set; }

    // Loan/Advance variables
    public decimal LoanInstallmentAmount { get; set; }
    public decimal AdvanceRecoveryAmount { get; set; }

    // Computed components (populated as each component is evaluated in sequence order)
    public Dictionary<string, decimal> ComponentValues { get; set; }
    // e.g. ComponentValues["BASIC"] = 75000, ComponentValues["HOUSE_RENT"] = 30000
}
```

### 10.4 Evaluation Algorithm

```
Algorithm: EvaluatePayrollEntry(employee, period)

1. Load employee's SalaryStructure (version active on period start date)
2. Load PayrollCalendar for period → CalendarWorkingDays, Holidays
3. Load AttendanceSummary for employee + period → WorkingDays, AbsentDays, LateDays
4. Load LeaveSummary for employee + period → PaidLeaveDays, UnpaidLeaveDays, HalfDays
5. Load active Loan installment amount → LoanInstallmentAmount
6. Load active Advance recovery → AdvanceRecoveryAmount
7. Build FormulaContext with all above values
8. Sort SalaryStructureComponents by Sequence ASC
9. For each component (in order):
   a. Check AllowanceRule conditions (is this component applicable this period?)
   b. If not applicable → skip, value = 0
   c. If applicable → evaluate FormulaExpression using NCalc
   d. Store result in FormulaContext.ComponentValues[component.Code]
   e. Update GrossSalary running total (earnings only)
10. Aggregate:
    Gross  = sum of all Earning components
    TotalDeductions = sum of all Deduction components
    Net    = Gross - TotalDeductions
11. Create PayrollEntry with PayrollEntryComponents (one row per component)
12. Return PayrollEntry
```

### 10.5 NCalc Expression Examples

```
Component           Formula Expression
──────────────────────────────────────────────────────────────
Basic               BaseSalary
House Rent          BaseSalary * 0.40
Medical             3000
Travel Allowance    WorkingDays * 250
Overtime            OvertimeHours * (BaseSalary / CalendarWorkingDays / 8) * 1.5
Late Deduction      LateDays * 200
Absent Deduction    (BaseSalary / CalendarWorkingDays) * AbsentDays
Half Day Deduction  (BaseSalary / CalendarWorkingDays / 2) * HalfDays
```

### 10.6 Validation Rules for Formulas

When a user saves a `SalaryStructureComponent.FormulaExpression`:

1. Parse expression using NCalc — catch any syntax errors
2. Verify all variable names are in the known variable list (FormulaContext properties + lower-sequence component codes)
3. Run a dry-run evaluation with dummy values (BaseSalary=100000, all others=0) — catch division by zero or type errors
4. Check for circular dependencies — component at sequence N cannot reference a component at sequence >= N
5. Return all errors to the user before saving

### 10.7 Security Constraints

- NCalc operates in a sandboxed expression mode — no file I/O, no reflection, no method calls beyond approved math functions
- Approved NCalc functions: `Abs`, `Round`, `Floor`, `Ceiling`, `Min`, `Max`, `If`
- Any formula containing function calls outside this whitelist is rejected at validation
- Formula length limit: 1000 characters

---

## 11. Module 06 — Allowance Engine

### 11.1 Purpose

Control when and how salary components (specifically allowances) are applied. Instead of hardcoded conditional logic, rules are stored in the database and evaluated at payroll time.

### 11.2 Entity

```
AllowanceRule
├── Id (UUID)
├── SalaryStructureComponentId (UUID FK)
├── CompanyId (UUID FK)
├── ApplicationMode (enum)
├── ConditionExpression (string, nullable)  ← NCalc boolean expression
└── Description (string)
```

### 11.3 Application Mode Enum

```
AllowanceApplicationMode:
  Always                    ← apply every month regardless
  WorkingDaysOnly           ← rate × WorkingDays (not CalendarWorkingDays)
  ExcludeSummerVacation     ← do not apply in months July and August
  ExcludeHolidays           ← deduct for public holidays
  ConditionalExpression     ← evaluate ConditionExpression to determine applicability
  DesignationSpecific       ← apply only to certain designation codes
  DepartmentSpecific        ← apply only to certain department codes
```

### 11.4 Allowance Rule Examples

```
Travel Allowance (Teachers):
  ApplicationMode: WorkingDaysOnly
  Result: TravelAllowance = 250 * WorkingDays (not CalendarWorkingDays)

Medical Allowance:
  ApplicationMode: Always
  Result: Always paid, regardless of attendance

Internet Allowance:
  ApplicationMode: ConditionalExpression
  ConditionExpression: "DesignationCode == 'REMOTE_DEV'"

Summer Vacation Travel:
  ApplicationMode: ExcludeSummerVacation
  Result: Travel allowance is 0 in July and August
```

### 11.5 Evaluation Logic

```
function IsComponentApplicable(component, rule, context):
  switch rule.ApplicationMode:
    case Always → return true
    case WorkingDaysOnly → component formula already uses WorkingDays variable
    case ExcludeSummerVacation → return !(period.Month in [7, 8])
    case ExcludeHolidays → context.WorkingDays excludes holidays (handled in attendance)
    case ConditionalExpression → evaluate rule.ConditionExpression with context; must return bool
    case DesignationSpecific → return employee.DesignationCode in rule.AllowedDesignations
    case DepartmentSpecific → return employee.DepartmentCode in rule.AllowedDepartments
```

---

## 12. Module 07 — Deduction Engine

### 12.1 Purpose

Manage all salary deductions with configurable rules. Deductions are applied during payroll calculation based on stored rules and real-time data.

### 12.2 Entity

```
DeductionRule
├── Id (UUID)
├── SalaryStructureComponentId (UUID FK)
├── CompanyId (UUID FK)
├── DeductionType (enum)
├── IsOptIn (bool)                 ← employee can choose to opt out
├── GracePeriodMinutes (int)       ← for late deductions: no deduction if late < this
├── DeductionFormula (string)      ← NCalc expression
└── IsActive (bool)
```

### 12.3 Deduction Type Enum

```
DeductionType:
  AttendanceBased           ← per absent day
  LateBased                 ← per late day/minute
  HalfDayBased              ← per half day
  LoanRecovery              ← auto-deducted installment
  AdvanceRecovery           ← advance salary repayment
  Provident Fund            ← % of basic (future)
  Manual                    ← one-time manual deduction
```

### 12.4 Opt-In / Opt-Out Behavior

- `AttendanceDeductionOptIn` is stored on `EmployeePayrollProfile`
- If `IsOptIn = true` on the `DeductionRule` AND `employee.AttendanceDeductionOptIn = false` → skip this deduction entirely
- An employee who opts out still has their absence recorded; they simply do not receive a salary deduction for it
- HR can override the opt-in flag on a per-payroll-entry basis before approval

### 12.5 Deduction Calculation Examples

```
Late Deduction:
  GracePeriodMinutes = 15
  DeductionFormula = "If(LateMinutes > 15, LateDays * 200, 0)"

Absence Deduction:
  DeductionFormula = "(BaseSalary / CalendarWorkingDays) * AbsentDays"

Half Day Deduction:
  DeductionFormula = "(BaseSalary / CalendarWorkingDays) * HalfDays * 0.5"

Loan Recovery:
  DeductionFormula = "LoanInstallmentAmount"
  (LoanInstallmentAmount is pre-populated in FormulaContext from active loan)
```

---

## 13. Module 08 — Attendance Integration

### 13.1 Purpose

Accept attendance summary data from external systems. PayrollMS never owns raw biometric or punch records.

### 13.2 Entity

```
AttendanceSummary
├── Id (UUID)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)
├── PeriodYear (int)
├── PeriodMonth (int)
├── WorkingDays (int)              ← actual days present
├── AbsentDays (int)
├── LateDays (int)
├── LateMinutes (int)              ← total accumulated late minutes
├── OvertimeHours (decimal)
├── HalfDays (int)
├── Holidays (int)                 ← public holidays in this period
├── Weekends (int)
├── SyncedAt (DateTimeOffset)
├── SourceSystem (string)          ← "SchoolERP", "HospitalERP"
└── IdempotencyKey (string UNIQUE) ← prevents duplicate sync
```

### 13.3 Sync Contract

```json
POST /api/v1/attendance/sync
X-Api-Key: pk_live_xxx

{
  "idempotencyKey": "COMP-001-EMP-1001-2025-07",
  "companyId": "uuid",
  "externalEmployeeId": "EMP-1001",
  "periodYear": 2025,
  "periodMonth": 7,
  "workingDays": 22,
  "absentDays": 1,
  "lateDays": 3,
  "lateMinutes": 95,
  "overtimeHours": 4.5,
  "halfDays": 0,
  "holidays": 2,
  "weekends": 8
}
```

### 13.4 Business Rules

- Idempotency key must be unique per company + employee + period; duplicate syncs with the same key are silently accepted (return the existing record, not an error)
- Once a `PayrollRun` for this period moves past `GENERATED` status, attendance sync for that period is rejected with `409 Conflict`
- Attendance can be re-synced (updated) while the payroll run is in `DRAFT` status — useful for corrections
- Bulk sync endpoint: `POST /api/v1/attendance/sync/bulk` accepts an array of up to 500 records

---

## 14. Module 09 — Leave Integration

### 14.1 Purpose

Accept monthly leave summaries from the HR or Leave management system.

### 14.2 Entity

```
LeaveSummary
├── Id (UUID)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)
├── PeriodYear (int)
├── PeriodMonth (int)
├── PaidLeaveDays (decimal)        ← decimal to handle partial days
├── UnpaidLeaveDays (decimal)
├── MedicalLeaveDays (decimal)
├── CasualLeaveDays (decimal)
├── HalfDays (decimal)
├── SyncedAt (DateTimeOffset)
└── IdempotencyKey (string UNIQUE)
```

### 14.3 Leave Impact on Payroll

```
Leave Type         Payroll Impact
─────────────────────────────────────────────────────
Paid Leave         No deduction; counts as WorkingDays
Unpaid Leave       Deducted as AbsentDays (if opted in)
Medical Leave      Company-configurable (often treated as paid)
Casual Leave       Company-configurable (often treated as paid)
Half Day           50% of daily rate deducted (if opted in)
```

Leave type treatment is configurable per company in `CompanyLeavePolicy` (a separate configuration table).

---

## 15. Module 10 — Payroll Run Engine

### 15.1 Purpose

The core orchestrator that generates payroll for a company for a given month. Coordinates all modules — employee profiles, salary structures, attendance, leave, loans, advances — and produces payroll entries.

### 15.2 Entities

#### PayrollRun

```
PayrollRun
├── Id (UUID)
├── CompanyId (UUID FK)
├── FinancialYearId (UUID FK)
├── PeriodYear (int)
├── PeriodMonth (int)
├── Status (enum)                   ← see 15.3
├── RunType (enum)                  ← Regular | Supplementary | Correction
├── FilterBranchId (UUID, nullable) ← if null, all branches
├── FilterDepartmentId (UUID, nullable)
├── TotalEmployees (int)
├── TotalGross (decimal)
├── TotalDeductions (decimal)
├── TotalNet (decimal)
├── GeneratedAt (DateTimeOffset)
├── GeneratedBy (string)
├── Remarks (string, nullable)
├── Version (int)                   ← starts at 1, increments on correction
└── ParentRunId (UUID, nullable)    ← for correction runs: points to original
```

#### PayrollEntry (one per employee per PayrollRun)

```
PayrollEntry
├── Id (UUID)
├── PayrollRunId (UUID FK)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)     ← snapshot
├── EmployeeCode (string)           ← snapshot
├── EmployeeName (string)           ← snapshot
├── DepartmentName (string)         ← snapshot
├── DesignationName (string)        ← snapshot
├── BankName (string)               ← snapshot
├── IBAN (string)                   ← snapshot
├── BaseSalary (decimal)            ← snapshot
├── WorkingDays (int)               ← snapshot from attendance
├── AbsentDays (int)                ← snapshot
├── LateDays (int)                  ← snapshot
├── GrossSalary (decimal)
├── TotalDeductions (decimal)
├── NetSalary (decimal)
├── Status (enum)                   ← EntryStatus: Calculated, Disputed, Revised, Locked
└── Components → [PayrollEntryComponent]
```

#### PayrollEntryComponent (one row per component per PayrollEntry)

```
PayrollEntryComponent
├── Id (UUID)
├── PayrollEntryId (UUID FK)
├── SalaryComponentId (UUID FK)
├── ComponentCode (string)          ← snapshot
├── ComponentName (string)          ← snapshot
├── ComponentType (enum)            ← Earning | Deduction
├── FormulaUsed (string)            ← snapshot of formula at time of calculation
├── CalculatedAmount (decimal)
└── IsManualOverride (bool)         ← true if HR manually edited this component
```

### 15.3 PayrollRun Status Machine

```
DRAFT
  │   (payroll generation job completes)
  ▼
GENERATED
  │   (first approval step approved)
  ▼
UNDER_REVIEW        ← can have multiple approval steps here (configurable)
  │   (all configured approval steps completed)
  ▼
APPROVED
  │   (disbursement batch created)
  ▼
DISBURSEMENT_PENDING
  │   (bank CSV downloaded and confirmed)
  ▼
DISBURSED
  │
  ▼
CLOSED

Side states (can happen from GENERATED or UNDER_REVIEW):
DISPUTED    ← at least one PayrollEntry has a dispute
CANCELLED   ← run cancelled before disbursement
```

### 15.4 Payroll Generation API

```json
POST /api/v1/payroll/runs
{
  "companyId": "uuid",
  "periodYear": 2025,
  "periodMonth": 7,
  "runType": "Regular",
  "filterBranchId": null,
  "filterDepartmentId": null,
  "remarks": "July 2025 Monthly Payroll"
}
```

Response (immediate — job is queued):

```json
{
  "payrollRunId": "uuid",
  "status": "DRAFT",
  "jobId": "hangfire-job-id",
  "message": "Payroll generation queued. Poll /api/v1/payroll/runs/{id} for status."
}
```

### 15.5 Generation Job (Hangfire Background Worker)

```
PayrollGenerationWorker.Execute(payrollRunId):

1. Load PayrollRun, set status = GENERATING (transient, not in state machine)
2. Load all active EmployeePayrollProfiles for company + filters
   (use profile version active on period start date from history table)
3. For each employee (parallel processing, max 10 concurrent):
   a. Load AttendanceSummary for period
   b. Load LeaveSummary for period
   c. Load active Loan installments
   d. Load active Advance recovery amounts
   e. Build FormulaContext
   f. Run FormulaEvaluatorService for each component in sequence order
   g. Check AllowanceRules for each component
   h. Apply DeductionRules (check opt-in status)
   i. Create PayrollEntry + PayrollEntryComponents
   j. Take snapshot: copy employee name, IBAN, structure formula etc.
4. Aggregate totals → update PayrollRun
5. Set PayrollRun.Status = GENERATED
6. Fire webhook event: payroll.generated
7. Create notification records for HR
```

### 15.6 Error Handling During Generation

- If attendance data is missing for an employee → `PayrollEntry.Status = AttendanceMissing`; generation continues for other employees
- If salary structure is not assigned → `PayrollEntry.Status = StructureMissing`
- Run can be GENERATED with some entries in error state; HR must resolve before approval
- If the entire run fails → `PayrollRun.Status = FAILED`, error details stored in `PayrollRun.Remarks`

### 15.7 Manual Entry Override

HR can edit individual `PayrollEntryComponent.CalculatedAmount` before the run is approved:

```json
PATCH /api/v1/payroll/entries/{entryId}/components/{componentId}
{
  "overrideAmount": 5000.00,
  "overrideReason": "Transport allowance adjusted for this month"
}
```

This sets `IsManualOverride = true` and logs to the Audit Log. Only allowed when `PayrollRun.Status` is `GENERATED` or `UNDER_REVIEW`.

---

## 16. Module 11 — Approval Workflow

### 16.1 Purpose

Multi-step, configurable approval chain per company. Each company defines their own approval sequence and which roles approve at each step.

### 16.2 Entities

#### ApprovalWorkflowTemplate

```
ApprovalWorkflowTemplate
├── Id (UUID)
├── CompanyId (UUID FK)
├── Name (string)                    ← "Standard Approval", "CEO Required"
├── IsDefault (bool)
└── Steps → [ApprovalWorkflowStep]
```

#### ApprovalWorkflowStep

```
ApprovalWorkflowStep
├── Id (UUID)
├── WorkflowTemplateId (UUID FK)
├── StepOrder (int)                  ← 1 = first to approve
├── StepName (string)                ← "HR Review", "Finance Approval", "CEO Sign-off"
├── RequiredRole (string)            ← role that can approve this step
├── IsOptional (bool)                ← if true, can be skipped
└── SLAHours (int)                   ← optional SLA for notification escalation
```

#### PayrollApprovalRecord

```
PayrollApprovalRecord
├── Id (UUID)
├── PayrollRunId (UUID FK)
├── CompanyId (UUID FK)
├── WorkflowStepId (UUID FK)
├── StepOrder (int)                  ← snapshot
├── StepName (string)                ← snapshot
├── Action (enum)                    ← Approved | Rejected | Skipped
├── ActorUserId (string)
├── ActorName (string)
├── Comments (string, nullable)
└── ActionAt (DateTimeOffset)
```

### 16.3 Example Workflow Configurations

```
Company A (3-step):
  Step 1 → HRManager → "HR Review"
  Step 2 → FinanceManager → "Finance Approval"
  Step 3 → CompanyAdmin → "CEO Approval"

Company B (2-step):
  Step 1 → HRManager → "HR Review"
  Step 2 → FinanceManager → "Finance Approval"

Company C (1-step):
  Step 1 → CompanyAdmin → "Direct Approval"
```

### 16.4 API Endpoints

```
POST   /api/v1/payroll/runs/{id}/approve    Approve current pending step
POST   /api/v1/payroll/runs/{id}/reject     Reject with comments
GET    /api/v1/payroll/runs/{id}/approvals  Get approval history + current pending step
GET    /api/v1/approvals/pending            List all runs awaiting my approval
```

Approve request:

```json
POST /api/v1/payroll/runs/{id}/approve
{
  "comments": "Reviewed and approved. All figures look correct."
}
```

### 16.5 Business Rules

- Only the user with the role matching the current pending step's `RequiredRole` can approve
- Rejection at any step returns the run to `GENERATED` status; HR is notified
- Rejection comments are mandatory
- All approvals and rejections are permanently recorded in `PayrollApprovalRecord`
- Once all steps are complete, `PayrollRun.Status` automatically moves to `APPROVED`
- SLA breach (if `SLAHours` is set and not approved within that time) triggers an escalation notification

---

## 17. Module 12 — Salary Slip

### 17.1 Purpose

Generate, store, and deliver individual salary slips after payroll approval.

### 17.2 Entity

```
SalarySlip
├── Id (UUID)
├── PayrollEntryId (UUID FK, UNIQUE)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)
├── PeriodYear (int)
├── PeriodMonth (int)
├── PdfUrl (string)                  ← Supabase Storage URL
├── GeneratedAt (DateTimeOffset)
├── EmailSentAt (DateTimeOffset, nullable)
├── EmailSentTo (string, nullable)
├── ViewedAt (DateTimeOffset, nullable)
├── IsLocked (bool)                  ← locked after disbursement
└── Version (int)                    ← increments if regenerated after dispute revision
```

### 17.3 PDF Salary Slip Content (QuestPDF)

```
┌──────────────────────────────────────────────────────────────┐
│  [Company Logo]          SALARY SLIP                         │
│  Company Name            Month: July 2025                    │
│  Company Address                                             │
├──────────────────────────────────────────────────────────────┤
│  Employee Name:  Ahmed Raza      Employee Code: EMP-1001    │
│  Department:     Computer Sci    Designation:   Teacher      │
│  Branch:         Main Campus     Joining Date:  Jan 2023     │
├──────────────────────────────────────────────────────────────┤
│  ATTENDANCE SUMMARY                                          │
│  Calendar Days: 31   Working Days: 26   Present: 25         │
│  Absent: 1           Late: 2            Half Days: 0        │
├────────────────────────────┬─────────────────────────────────┤
│  EARNINGS                  │  DEDUCTIONS                     │
│  Basic Salary    75,000   │  Absent Deduction    2,885      │
│  House Rent      30,000   │  Late Deduction        400      │
│  Medical          3,000   │  Loan Recovery       5,000      │
│  Transport        2,000   │                                  │
│  Travel (25d)     6,250   │                                  │
│                            │                                  │
│  Gross:        116,250   │  Total Deductions:   8,285      │
├────────────────────────────┴─────────────────────────────────┤
│                          NET SALARY:  107,965               │
├──────────────────────────────────────────────────────────────┤
│  Bank: HBL               IBAN: PK36HABB0000000123456702     │
├──────────────────────────────────────────────────────────────┤
│  [QR Code]   HR Signature ___________  Finance ___________  │
│  Verify at: payrollms.app/verify/{slipId}                   │
└──────────────────────────────────────────────────────────────┘
```

### 17.4 Slip Generation Flow

```
1. PayrollRun moves to APPROVED
2. Hangfire job: GenerateSalarySlipsJob(payrollRunId)
3. For each PayrollEntry in the run:
   a. Build slip data from PayrollEntry + Company info
   b. Generate PDF using QuestPDF
   c. Upload PDF to Supabase Storage: /slips/{companyId}/{year}/{month}/{employeeCode}.pdf
   d. Create SalarySlip record with PdfUrl
   e. Queue email delivery job
4. Email job: send PDF to employee's registered email
5. Fire webhook: payroll.slips_generated
```

### 17.5 API Endpoints

```
GET    /api/v1/slips/{employeeId}/{year}/{month}    Get slip for employee + period
GET    /api/v1/slips/{id}/pdf                        Download PDF
GET    /api/v1/slips/{id}/view                       In-app viewer URL (signed URL)
GET    /api/v1/slips/verify/{slipId}                 Public QR verification endpoint
PATCH  /api/v1/slips/{id}/resend-email               Resend email
GET    /api/v1/payroll/runs/{runId}/slips            List all slips for a run
```

### 17.6 QR Code Verification

Each salary slip includes a QR code linking to a public endpoint:

```
GET /api/v1/slips/verify/{slipId}

Response:
{
  "isValid": true,
  "employeeName": "Ahmed Raza",
  "period": "July 2025",
  "netSalary": 107965.00,
  "generatedAt": "2025-07-31T...",
  "companyName": "Roots School System"
}
```

This allows third parties (banks, embassies) to verify slip authenticity without accessing the full system.

---

## 18. Module 13 — Dispute Management

### 18.1 Purpose

Allow employees to raise disputes against their salary slips. Track the full resolution lifecycle with audit trail.

### 18.2 Entities

#### DisputeTicket

```
DisputeTicket
├── Id (UUID)
├── CompanyId (UUID FK)
├── PayrollEntryId (UUID FK)
├── ExternalEmployeeId (string)
├── RaisedBy (string)               ← employee user ID or external employee ID
├── Subject (string)
├── Description (string)
├── Status (enum)
├── Priority (enum)                 ← Low | Medium | High
├── AssignedTo (string, nullable)   ← payroll officer user ID
├── ResolvedAt (DateTimeOffset, nullable)
├── ResolvedBy (string, nullable)
├── ResolutionNotes (string, nullable)
├── CreatedAt (DateTimeOffset)
└── Comments → [DisputeComment]
```

#### DisputeComment

```
DisputeComment
├── Id (UUID)
├── DisputeTicketId (UUID FK)
├── AuthorId (string)
├── AuthorName (string)
├── AuthorRole (string)
├── Content (string)
└── CreatedAt (DateTimeOffset)
```

### 18.3 Dispute Status Machine

```
OPEN
  │   (payroll officer picks it up)
  ▼
UNDER_REVIEW
  │   (if investigation requires payroll recalculation)
  ├──→ PAYROLL_CORRECTION_REQUIRED
  │         │   (correction run generated)
  │         ▼
  │     CORRECTION_IN_PROGRESS
  │         │   (correction approved and new slip generated)
  │         ▼
  │     RESOLVED (with new payslip version)
  │
  │   (if dispute is invalid)
  └──→ REJECTED
       (if dispute is valid, no recalculation needed)
       RESOLVED (with explanation)
```

### 18.4 Payroll Correction Flow

When a dispute requires salary correction:

```
1. PayrollOfficer marks dispute as PAYROLL_CORRECTION_REQUIRED
2. System creates a new PayrollRun of RunType = Correction
   - ParentRunId = original PayrollRun.Id
   - Version = original.Version + 1
3. Correction run contains only the disputed employee's entry
4. PayrollOfficer adjusts the relevant components manually
5. Correction run goes through the same approval workflow
6. On approval: new SalarySlip is generated (Version = 2)
7. Old SalarySlip is marked as superseded (not deleted)
8. Dispute status = RESOLVED
9. Difference amount is tracked for accounting reconciliation
```

### 18.5 API Endpoints

```
POST   /api/v1/disputes                          Raise dispute
GET    /api/v1/disputes                          List disputes (filtered by status, employee, period)
GET    /api/v1/disputes/{id}                     Get dispute details
POST   /api/v1/disputes/{id}/comments            Add comment
PATCH  /api/v1/disputes/{id}/assign              Assign to officer
PATCH  /api/v1/disputes/{id}/status              Update status
POST   /api/v1/disputes/{id}/initiate-correction Create correction run
POST   /api/v1/disputes/{id}/resolve             Mark as resolved
POST   /api/v1/disputes/{id}/reject              Reject with reason
```

---

## 19. Module 14 — Disbursement & Bank Transfer

### 19.1 Purpose

After payroll approval, generate bank transfer files (CSV/Excel) for manual upload to banking portals. Track disbursement status.

### 19.2 Entities

#### DisbursementBatch

```
DisbursementBatch
├── Id (UUID)
├── CompanyId (UUID FK)
├── PayrollRunId (UUID FK)
├── Status (enum)                    ← Pending | Generated | Confirmed | PartiallyFailed | Completed
├── TotalAmount (decimal)
├── TotalRecords (int)
├── BankFileUrl (string, nullable)   ← Supabase Storage URL of generated CSV/Excel
├── FileFormat (enum)                ← CSV | Excel
├── GeneratedAt (DateTimeOffset)
├── GeneratedBy (string)
├── ConfirmedAt (DateTimeOffset, nullable)
├── ConfirmedBy (string, nullable)
└── Remarks (string, nullable)
```

#### BankTransferRecord (one per employee per batch)

```
BankTransferRecord
├── Id (UUID)
├── DisbursementBatchId (UUID FK)
├── PayrollEntryId (UUID FK)
├── CompanyId (UUID FK)
├── EmployeeCode (string)
├── EmployeeName (string)
├── BankName (string)
├── AccountTitle (string)
├── IBAN (string)
├── Amount (decimal)
├── Status (enum)                    ← Pending | Transferred | Failed | Reversed
├── FailureReason (string, nullable)
└── TransferReference (string, nullable)  ← bank's reference number, filled manually
```

### 19.3 Bank File Formats

#### Standard CSV Format

```csv
Sr#,Employee Code,Employee Name,Bank Name,Account Title,IBAN,Amount,Remarks
1,EMP-1001,Ahmed Raza,HBL,Ahmed Raza,PK36HABB0000000123456702,107965.00,July 2025 Salary
2,EMP-1002,Sara Khan,UBL,Sara Khan,PK24UNIL0000000012345601,85000.00,July 2025 Salary
```

#### Excel Format (ClosedXML)

- Same data as CSV but with formatting
- Company header, month, total row
- Color-coded status column
- Protected cells (read-only bank details)

### 19.4 Disbursement Workflow

```
1. PayrollRun.Status = APPROVED
2. Finance officer triggers: POST /api/v1/disbursements
3. System creates DisbursementBatch + BankTransferRecords (from PayrollEntries)
4. Generates CSV + Excel file → uploads to Supabase Storage
5. DisbursementBatch.Status = Generated
6. Finance officer downloads file, uploads to banking portal
7. Finance officer confirms: POST /api/v1/disbursements/{id}/confirm
8. Optionally marks individual failed records: PATCH /api/v1/disbursements/{batchId}/records/{recordId}/fail
9. PayrollRun.Status = DISBURSED
10. Fire webhook: payroll.disbursed
```

### 19.5 API Endpoints

```
POST   /api/v1/disbursements                         Create disbursement batch
GET    /api/v1/disbursements/{id}                    Get batch details
GET    /api/v1/disbursements/{id}/download/csv       Download CSV
GET    /api/v1/disbursements/{id}/download/excel     Download Excel
POST   /api/v1/disbursements/{id}/confirm            Mark as transferred
PATCH  /api/v1/disbursements/{id}/records/{rid}/fail Mark individual record failed
GET    /api/v1/disbursements                         List batches (filtered)
```

---

## 20. Module 15 — Loan & Advance Management

### 20.1 Loan Entity

```
EmployeeLoan
├── Id (UUID)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)
├── LoanAmount (decimal)
├── InterestRate (decimal)           ← 0 if interest-free
├── TotalInstallments (int)
├── InstallmentAmount (decimal)      ← computed: LoanAmount / TotalInstallments
├── DisbursedDate (DateOnly)
├── StartDeductionMonth (int)
├── StartDeductionYear (int)
├── Status (enum)                    ← Active | Completed | Cancelled
├── RemainingAmount (decimal)
├── ApprovedBy (string)
└── Installments → [LoanInstallment]
```

```
LoanInstallment
├── Id (UUID)
├── LoanId (UUID FK)
├── InstallmentNumber (int)
├── PeriodYear (int)
├── PeriodMonth (int)
├── Amount (decimal)
├── DeductedFromPayrollEntryId (UUID FK, nullable)
└── Status (enum)                    ← Pending | Deducted | Skipped
```

### 20.2 Advance Salary Entity

```
AdvanceSalaryRequest
├── Id (UUID)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)
├── RequestedAmount (decimal)
├── ApprovedAmount (decimal, nullable)
├── RequestedOn (DateOnly)
├── Status (enum)                    ← Pending | Approved | Rejected | Recovered
├── RecoveryInstallments (int)       ← how many months to deduct from
├── RecoveryStartMonth (int)
├── RecoveryStartYear (int)
├── ApprovedBy (string, nullable)
└── RecoveryPerInstallment (decimal) ← ApprovedAmount / RecoveryInstallments
```

### 20.3 Auto-Deduction

During payroll generation:

1. `PayrollCalculatorService` queries active loans for the employee
2. Checks if the current period falls within deduction months
3. Populates `FormulaContext.LoanInstallmentAmount`
4. The `LOAN_DEDUCT` component formula evaluates to this amount
5. On successful payroll entry creation, marks `LoanInstallment.Status = Deducted`
6. Updates `EmployeeLoan.RemainingAmount`

---

## 21. Module 16 — Bonus & Overtime

### 21.1 Bonus Entity

```
BonusRecord
├── Id (UUID)
├── CompanyId (UUID FK)
├── ExternalEmployeeId (string)
├── BonusType (enum)                 ← Festival | Performance | Annual | Ad-hoc
├── Amount (decimal)
├── ForPeriodYear (int)
├── ForPeriodMonth (int)
├── Description (string)
├── ApprovedBy (string)
├── Status (enum)                    ← Pending | IncludedInPayroll | Paid
└── IncludedInPayrollEntryId (UUID FK, nullable)
```

### 21.2 Bonus in Payroll

Bonuses are included in payroll runs automatically when `Status = Pending` and `ForPeriodYear + ForPeriodMonth` matches the current run period. They appear as the `BONUS` component in the `PayrollEntry`.

### 21.3 Overtime Rules

Overtime is configured per company in `OvertimePolicy`:

```
OvertimePolicy
├── CompanyId (UUID FK)
├── HourlyMultiplier (decimal)       ← 1.5 for standard overtime
├── WeekendMultiplier (decimal)      ← 2.0 for weekend overtime
├── HolidayMultiplier (decimal)      ← 2.5 for holiday overtime
├── MaxOvertimeHoursPerMonth (int)
└── IsActive (bool)
```

Overtime formula in salary structure:

```
OvertimeAmount = OvertimeHours * (BaseSalary / CalendarWorkingDays / 8) * HourlyMultiplier
```

`OvertimeHours` comes from the `AttendanceSummary` sync.

---

## 22. Module 17 — Notifications & Events

### 22.1 Outbound Webhook Events

PayrollMS fires webhook events to registered consumer URLs. The calling ERP can subscribe to be notified of payroll lifecycle events.

#### Webhook Registration

```json
POST /api/v1/webhooks
{
  "url": "https://schoolerp.com/webhooks/payroll",
  "events": ["payroll.generated", "payroll.approved", "payroll.disbursed", "dispute.raised", "dispute.resolved"],
  "secret": "wh_secret_xxxx"
}
```

Events are signed with HMAC-SHA256 using the registered secret (standard webhook security pattern).

#### Event Payloads

```json
// payroll.approved
{
  "event": "payroll.approved",
  "companyId": "uuid",
  "payrollRunId": "uuid",
  "period": "2025-07",
  "totalEmployees": 150,
  "totalNet": 12500000.00,
  "approvedAt": "2025-07-31T14:22:00Z"
}

// payroll.disbursed
{
  "event": "payroll.disbursed",
  "companyId": "uuid",
  "payrollRunId": "uuid",
  "period": "2025-07",
  "disbursementBatchId": "uuid",
  "disbursedAt": "2025-07-31T16:00:00Z"
}

// dispute.raised
{
  "event": "dispute.raised",
  "companyId": "uuid",
  "disputeId": "uuid",
  "externalEmployeeId": "EMP-1001",
  "period": "2025-07",
  "raisedAt": "2025-08-02T09:15:00Z"
}
```

### 22.2 Internal Notifications (Dashboard)

```
NotificationRecord
├── Id (UUID)
├── CompanyId (UUID FK)
├── RecipientUserId (string)
├── Type (enum)                      ← PayrollGenerated | ApprovalRequired | DisputeRaised | SlipReady
├── Title (string)
├── Body (string)
├── ReferenceId (UUID)               ← PayrollRunId, DisputeId, etc.
├── ReferenceType (string)
├── IsRead (bool)
└── CreatedAt (DateTimeOffset)
```

### 22.3 Email Notifications (via Resend / MailKit)

|Trigger|Recipient|Template|
|---|---|---|
|Payroll generated|HR Manager|"Payroll for {month} is ready for review"|
|Approval required|Next approver|"Payroll awaiting your approval"|
|Payroll approved|HR + Finance|"Payroll for {month} has been approved"|
|Salary slip ready|Employee|"Your salary slip for {month} is ready" (with PDF attachment)|
|Dispute raised|Assigned officer|"New dispute raised by {employee}"|
|Dispute resolved|Employee|"Your dispute has been resolved"|
|SLA breach|Manager|"Approval overdue for {month} payroll"|

---

## 23. Module 18 — Reports & BI

### 23.1 Available Reports

All reports support export to **Excel (ClosedXML)** and **PDF (QuestPDF)**.

|Report|Description|Filters|
|---|---|---|
|Payroll Summary|Totals per payroll run: gross, deductions, net|Period, Branch, Department|
|Employee Salary Detail|Full breakdown per employee|Period, Employee, Department|
|Department Salary Report|Salary cost per department|Period, Branch|
|Cost Center Report|Salary distribution by cost center|Period, Financial Year|
|Bank Transfer Report|All IBAN transfers for a period|Period, Bank|
|Yearly Salary Ledger|Full year salary history per employee|Employee, Financial Year|
|Component Analysis|Breakdown by component type across employees|Period, Component|
|Deduction Report|All deductions (attendance, loans, advances)|Period, Deduction Type|
|Allowance Report|All allowances paid|Period, Allowance Type|
|Dispute Report|Disputes by status, resolution time|Period, Status|
|Loan Outstanding Report|Active loans, remaining amounts, installment schedule|Employee, Status|
|Overtime Report|Overtime hours and amounts|Period, Department|
|Headcount Cost Report|FTE cost with salary breakdown|Period, Branch|

### 23.2 API Endpoints

```
GET /api/v1/reports/payroll-summary?periodYear=2025&periodMonth=7&format=excel
GET /api/v1/reports/employee-detail?employeeId=EMP-1001&periodYear=2025&periodMonth=7&format=pdf
GET /api/v1/reports/department-salary?periodYear=2025&periodMonth=7&branchId=uuid&format=excel
GET /api/v1/reports/bank-transfer?periodYear=2025&periodMonth=7&format=csv
GET /api/v1/reports/yearly-ledger?employeeId=EMP-1001&financialYearId=uuid&format=excel
GET /api/v1/reports/component-analysis?periodYear=2025&periodMonth=7&format=excel
```

Format query param: `excel` | `pdf` | `json` (json returns data for frontend rendering)

### 23.3 Report Generation

Reports are generated on-demand (synchronous for small datasets, queued via Hangfire for large ones):

- < 500 employees → synchronous, returned in response
- ≥ 500 employees → queued job; client polls `GET /api/v1/reports/jobs/{jobId}`

---

## 24. Module 19 — Audit Log

### 24.1 Principle

Everything is logged. Nothing is deleted. The audit log is append-only.

### 24.2 Entity

```
AuditLog
├── Id (UUID)
├── CompanyId (UUID FK)
├── EntityType (string)              ← "PayrollRun", "PayrollEntry", "SalaryStructure", etc.
├── EntityId (string)
├── Action (enum)                    ← Created | Updated | Deleted(soft) | Approved | Rejected | Generated | Disbursed
├── ActorId (string)
├── ActorName (string)
├── ActorRole (string)
├── ActorIp (string, nullable)
├── OldValues (JSON, nullable)       ← previous state
├── NewValues (JSON, nullable)       ← new state
├── Diff (JSON, nullable)            ← field-level diff
├── Metadata (JSON, nullable)        ← additional context
└── CreatedAt (DateTimeOffset)
```

### 24.3 What Is Audited

|Action|Audit Logged|
|---|---|
|Employee profile synced / updated|✅|
|Salary structure created / modified|✅|
|Payroll run created / status changed|✅|
|Payroll entry component manually overridden|✅|
|Approval action (approve / reject)|✅|
|Salary slip generated / emailed|✅|
|Dispute raised / commented / resolved|✅|
|Disbursement batch created / confirmed|✅|
|Loan created / installment deducted|✅|
|Any user login (dashboard)|✅|
|API Key used (machine-to-machine call)|✅|
|Report exported|✅|

### 24.4 Implementation

Audit logging is implemented as a MediatR pipeline behavior (`AuditBehavior`) that wraps all commands. The behavior:

1. Captures the "before" state by reading the entity before the command executes
2. Lets the command execute normally
3. Captures the "after" state
4. Writes a diff to `AuditLog`

For bulk operations (payroll generation), a dedicated audit writer writes one record per material change.

---

## 25. External API Contract

### 25.1 API Versioning

All routes are prefixed with `/api/v1/`. Future breaking changes increment to `/api/v2/`.

### 25.2 Standard Response Envelope

All API responses use a consistent envelope:

```json
// Success
{
  "success": true,
  "data": { ... },
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}

// Error
{
  "success": false,
  "error": {
    "code": "PAYROLL_FROZEN",
    "message": "Payroll for this period is frozen and cannot be modified.",
    "details": []
  }
}
```

### 25.3 Pagination

All list endpoints support:

```
GET /api/v1/payroll/runs?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc
```

### 25.4 Full API Route Reference

```
# COMPANY & SETUP
POST   /api/v1/companies
GET    /api/v1/companies/{id}
PUT    /api/v1/companies/{id}
POST   /api/v1/companies/{id}/branches
GET    /api/v1/companies/{id}/branches
POST   /api/v1/companies/{id}/departments
GET    /api/v1/companies/{id}/departments
POST   /api/v1/companies/{id}/designations
GET    /api/v1/companies/{id}/designations
POST   /api/v1/companies/{id}/cost-centers
POST   /api/v1/companies/{id}/financial-years
GET    /api/v1/companies/{id}/financial-years
POST   /api/v1/companies/{id}/payroll-calendar
GET    /api/v1/companies/{id}/payroll-calendar/{year}/{month}

# EMPLOYEE PROFILES
POST   /api/v1/employees/sync
POST   /api/v1/employees/sync/bulk
GET    /api/v1/employees
GET    /api/v1/employees/{externalId}
GET    /api/v1/employees/{externalId}/history

# SALARY COMPONENTS
POST   /api/v1/salary-components
GET    /api/v1/salary-components
GET    /api/v1/salary-components/{id}
PUT    /api/v1/salary-components/{id}
DELETE /api/v1/salary-components/{id}

# SALARY STRUCTURES
POST   /api/v1/salary-structures
GET    /api/v1/salary-structures
GET    /api/v1/salary-structures/{id}
PUT    /api/v1/salary-structures/{id}
DELETE /api/v1/salary-structures/{id}
POST   /api/v1/salary-structures/{id}/components
PUT    /api/v1/salary-structures/{id}/components/{componentId}
DELETE /api/v1/salary-structures/{id}/components/{componentId}
POST   /api/v1/salary-structures/formula/validate

# ATTENDANCE & LEAVE
POST   /api/v1/attendance/sync
POST   /api/v1/attendance/sync/bulk
GET    /api/v1/attendance/{externalEmployeeId}/{year}/{month}
POST   /api/v1/leave/sync
POST   /api/v1/leave/sync/bulk
GET    /api/v1/leave/{externalEmployeeId}/{year}/{month}

# PAYROLL RUNS
POST   /api/v1/payroll/runs
GET    /api/v1/payroll/runs
GET    /api/v1/payroll/runs/{id}
GET    /api/v1/payroll/runs/{id}/entries
GET    /api/v1/payroll/runs/{id}/entries/{entryId}
PATCH  /api/v1/payroll/entries/{entryId}/components/{componentId}
POST   /api/v1/payroll/runs/{id}/approve
POST   /api/v1/payroll/runs/{id}/reject
GET    /api/v1/payroll/runs/{id}/approvals
GET    /api/v1/approvals/pending

# SALARY SLIPS
GET    /api/v1/slips/{externalEmployeeId}/{year}/{month}
GET    /api/v1/slips/{id}/pdf
GET    /api/v1/slips/{id}/view
GET    /api/v1/slips/verify/{slipId}
PATCH  /api/v1/slips/{id}/resend-email
GET    /api/v1/payroll/runs/{runId}/slips

# DISPUTES
POST   /api/v1/disputes
GET    /api/v1/disputes
GET    /api/v1/disputes/{id}
POST   /api/v1/disputes/{id}/comments
PATCH  /api/v1/disputes/{id}/assign
PATCH  /api/v1/disputes/{id}/status
POST   /api/v1/disputes/{id}/initiate-correction
POST   /api/v1/disputes/{id}/resolve
POST   /api/v1/disputes/{id}/reject

# DISBURSEMENTS
POST   /api/v1/disbursements
GET    /api/v1/disbursements
GET    /api/v1/disbursements/{id}
GET    /api/v1/disbursements/{id}/download/csv
GET    /api/v1/disbursements/{id}/download/excel
POST   /api/v1/disbursements/{id}/confirm
PATCH  /api/v1/disbursements/{id}/records/{rid}/fail

# LOANS & ADVANCES
POST   /api/v1/loans
GET    /api/v1/loans
GET    /api/v1/loans/{id}
PATCH  /api/v1/loans/{id}/approve
PATCH  /api/v1/loans/{id}/cancel
GET    /api/v1/loans/{id}/installments
POST   /api/v1/advances
GET    /api/v1/advances
PATCH  /api/v1/advances/{id}/approve
PATCH  /api/v1/advances/{id}/reject

# BONUSES & OVERTIME
POST   /api/v1/bonuses
GET    /api/v1/bonuses
PUT    /api/v1/bonuses/{id}
DELETE /api/v1/bonuses/{id}
GET    /api/v1/companies/{id}/overtime-policy
PUT    /api/v1/companies/{id}/overtime-policy

# REPORTS
GET    /api/v1/reports/payroll-summary
GET    /api/v1/reports/employee-detail
GET    /api/v1/reports/department-salary
GET    /api/v1/reports/cost-center
GET    /api/v1/reports/bank-transfer
GET    /api/v1/reports/yearly-ledger
GET    /api/v1/reports/component-analysis
GET    /api/v1/reports/deductions
GET    /api/v1/reports/allowances
GET    /api/v1/reports/disputes
GET    /api/v1/reports/loans-outstanding
GET    /api/v1/reports/overtime
GET    /api/v1/reports/headcount-cost
GET    /api/v1/reports/jobs/{jobId}

# AUDIT
GET    /api/v1/audit-logs
GET    /api/v1/audit-logs/{entityType}/{entityId}

# WEBHOOKS
POST   /api/v1/webhooks
GET    /api/v1/webhooks
DELETE /api/v1/webhooks/{id}
POST   /api/v1/webhooks/{id}/test

# AUTH & USERS
POST   /api/v1/auth/api-keys
DELETE /api/v1/auth/api-keys/{id}
GET    /api/v1/users/me
GET    /api/v1/users
POST   /api/v1/users/{id}/assign-role
```

---

## 26. Database Schema (ERD Reference)

### 26.1 Core Tables

```sql
-- COMPANIES
CREATE TABLE companies (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name TEXT NOT NULL,
  code TEXT UNIQUE NOT NULL,
  logo_url TEXT,
  address TEXT,
  contact_email TEXT,
  contact_phone TEXT,
  is_active BOOLEAN DEFAULT TRUE,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- BRANCHES
CREATE TABLE branches (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  company_id UUID NOT NULL REFERENCES companies(id),
  name TEXT NOT NULL,
  code TEXT NOT NULL,
  address TEXT,
  is_active BOOLEAN DEFAULT TRUE,
  UNIQUE(company_id, code)
);

-- EMPLOYEE PAYROLL PROFILES
CREATE TABLE employee_payroll_profiles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  company_id UUID NOT NULL REFERENCES companies(id),
  external_employee_id TEXT NOT NULL,
  employee_code TEXT NOT NULL,
  full_name TEXT NOT NULL,
  branch_id UUID REFERENCES branches(id),
  branch_name TEXT NOT NULL,
  department_id UUID,
  department_name TEXT NOT NULL,
  designation_id UUID,
  designation_name TEXT NOT NULL,
  salary_structure_id UUID NOT NULL REFERENCES salary_structures(id),
  base_salary NUMERIC(12,2) NOT NULL,
  attendance_deduction_opt_in BOOLEAN DEFAULT TRUE,
  joining_date DATE NOT NULL,
  leaving_date DATE,
  status TEXT NOT NULL DEFAULT 'Active',
  effective_from DATE NOT NULL DEFAULT CURRENT_DATE,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(company_id, external_employee_id)
);

-- SALARY COMPONENTS
CREATE TABLE salary_components (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  company_id UUID NOT NULL REFERENCES companies(id),
  name TEXT NOT NULL,
  code TEXT NOT NULL,
  type TEXT NOT NULL,                    -- Earning | Deduction | EmployerContribution
  calculation_method TEXT NOT NULL,
  default_value NUMERIC(12,2),
  is_taxable BOOLEAN DEFAULT FALSE,
  is_recurring BOOLEAN DEFAULT TRUE,
  is_optional BOOLEAN DEFAULT FALSE,
  is_active BOOLEAN DEFAULT TRUE,
  description TEXT,
  UNIQUE(company_id, code)
);

-- SALARY STRUCTURES
CREATE TABLE salary_structures (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  company_id UUID NOT NULL REFERENCES companies(id),
  name TEXT NOT NULL,
  code TEXT NOT NULL,
  description TEXT,
  effective_from DATE NOT NULL,
  effective_to DATE,
  is_active BOOLEAN DEFAULT TRUE,
  UNIQUE(company_id, code)
);

-- SALARY STRUCTURE COMPONENTS
CREATE TABLE salary_structure_components (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  salary_structure_id UUID NOT NULL REFERENCES salary_structures(id),
  salary_component_id UUID NOT NULL REFERENCES salary_components(id),
  formula_expression TEXT,
  fixed_amount NUMERIC(12,2),
  sequence INT NOT NULL,
  is_active BOOLEAN DEFAULT TRUE
);

-- PAYROLL RUNS
CREATE TABLE payroll_runs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  company_id UUID NOT NULL REFERENCES companies(id),
  period_year INT NOT NULL,
  period_month INT NOT NULL,
  status TEXT NOT NULL DEFAULT 'DRAFT',
  run_type TEXT NOT NULL DEFAULT 'Regular',
  total_employees INT DEFAULT 0,
  total_gross NUMERIC(14,2) DEFAULT 0,
  total_deductions NUMERIC(14,2) DEFAULT 0,
  total_net NUMERIC(14,2) DEFAULT 0,
  generated_at TIMESTAMPTZ,
  generated_by TEXT,
  version INT NOT NULL DEFAULT 1,
  parent_run_id UUID REFERENCES payroll_runs(id),
  remarks TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

-- PAYROLL ENTRIES
CREATE TABLE payroll_entries (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  payroll_run_id UUID NOT NULL REFERENCES payroll_runs(id),
  company_id UUID NOT NULL REFERENCES companies(id),
  external_employee_id TEXT NOT NULL,
  employee_code TEXT NOT NULL,
  employee_name TEXT NOT NULL,
  department_name TEXT NOT NULL,
  designation_name TEXT NOT NULL,
  bank_name TEXT,
  iban TEXT,
  base_salary NUMERIC(12,2) NOT NULL,
  working_days INT,
  absent_days INT,
  late_days INT,
  gross_salary NUMERIC(12,2) DEFAULT 0,
  total_deductions NUMERIC(12,2) DEFAULT 0,
  net_salary NUMERIC(12,2) DEFAULT 0,
  status TEXT NOT NULL DEFAULT 'Calculated'
);

-- PAYROLL ENTRY COMPONENTS
CREATE TABLE payroll_entry_components (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  payroll_entry_id UUID NOT NULL REFERENCES payroll_entries(id),
  salary_component_id UUID NOT NULL REFERENCES salary_components(id),
  component_code TEXT NOT NULL,
  component_name TEXT NOT NULL,
  component_type TEXT NOT NULL,
  formula_used TEXT,
  calculated_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
  is_manual_override BOOLEAN DEFAULT FALSE
);

-- AUDIT LOGS
CREATE TABLE audit_logs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  company_id UUID NOT NULL REFERENCES companies(id),
  entity_type TEXT NOT NULL,
  entity_id TEXT NOT NULL,
  action TEXT NOT NULL,
  actor_id TEXT NOT NULL,
  actor_name TEXT NOT NULL,
  actor_role TEXT,
  actor_ip TEXT,
  old_values JSONB,
  new_values JSONB,
  diff JSONB,
  metadata JSONB,
  created_at TIMESTAMPTZ DEFAULT NOW()
);
```

### 26.2 Row Level Security

```sql
-- Enable RLS on all tables
ALTER TABLE companies ENABLE ROW LEVEL SECURITY;
ALTER TABLE payroll_runs ENABLE ROW LEVEL SECURITY;
ALTER TABLE payroll_entries ENABLE ROW LEVEL SECURITY;
-- (repeat for all tables)

-- Policy: users can only see their company's data
CREATE POLICY tenant_isolation ON payroll_runs
  USING (company_id::TEXT = current_setting('app.current_company_id', TRUE));

-- Set tenant context at connection time (done in DbContext)
SET LOCAL "app.current_company_id" = 'uuid-here';
```

---

## 27. Non-Functional Requirements

### 27.1 Performance

|Operation|Target|
|---|---|
|Single payroll entry calculation|< 100ms|
|Bulk payroll generation (500 employees)|< 60 seconds (background job)|
|Salary slip PDF generation|< 2 seconds per slip|
|Report generation (< 500 employees)|< 10 seconds|
|API response time (P95)|< 300ms|
|List endpoints (paginated)|< 200ms|

### 27.2 Reliability

- Hangfire jobs must have retry logic: 3 retries with exponential backoff
- Failed payroll generation jobs must leave the `PayrollRun` in `FAILED` status with error details — never in an ambiguous state
- Idempotency keys on attendance/leave sync prevent double-counting on network retries
- All database writes use transactions — partial payroll generation is never committed

### 27.3 Security

- All API endpoints require authentication (API Key or JWT) — no public endpoints except `/slips/verify/{id}`
- API Keys are stored as SHA-256 hashes — the plaintext key is shown only once at creation
- PDF salary slips in Supabase Storage use signed URLs with 1-hour expiry
- All financial amounts stored as `NUMERIC(12,2)` — no floating point
- Formula expressions run in NCalc sandbox — no arbitrary code execution

### 27.4 Data Integrity

- All monetary amounts use `decimal` (C#) / `NUMERIC(12,2)` (PostgreSQL) — never `float` or `double`
- All timestamps stored as `TIMESTAMPTZ` (UTC) — display conversion is client-side
- Soft deletes only — no hard deletes on any financial entity
- Payroll entries and their components are immutable once `PayrollRun.Status = APPROVED`

---

## 28. Error Handling Standard

### 28.1 Error Codes

```
COMPANY_NOT_FOUND
EMPLOYEE_PROFILE_NOT_FOUND
SALARY_STRUCTURE_NOT_FOUND
SALARY_STRUCTURE_NOT_ASSIGNED
ATTENDANCE_NOT_SYNCED
PAYROLL_RUN_NOT_FOUND
PAYROLL_FROZEN                    ← period is past freeze date
PAYROLL_ALREADY_EXISTS            ← run for this period already exists
PAYROLL_ENTRY_LOCKED              ← entry is locked (post-approval)
FORMULA_SYNTAX_ERROR
FORMULA_CIRCULAR_DEPENDENCY
FORMULA_VARIABLE_UNDEFINED
APPROVAL_STEP_UNAUTHORIZED        ← wrong role trying to approve
APPROVAL_ALREADY_COMPLETED
DISPUTE_ALREADY_RESOLVED
DISBURSEMENT_NOT_APPROVED         ← trying to disburse unapproved payroll
TENANT_MISMATCH
API_KEY_INVALID
API_KEY_EXPIRED
INSUFFICIENT_PERMISSIONS
```

### 28.2 HTTP Status Mapping

```
400 Bad Request          → Validation errors, formula errors
401 Unauthorized         → Invalid/missing API key or JWT
403 Forbidden            → Authenticated but insufficient role
404 Not Found            → Entity not found
409 Conflict             → Duplicate run, frozen period, locked entry
422 Unprocessable        → Business rule violation
500 Internal Server Error → Unhandled exceptions
```

---

## 29. Development Phases & Milestones

### Phase 1 — Foundation (Weeks 1–3)

- [ ] Project setup: Clean Architecture solution, EF Core, PostgreSQL connection
- [ ] Tenant & Company module (companies, branches, departments, designations)
- [ ] API Key authentication middleware
- [ ] Supabase Auth JWT validation
- [ ] RBAC middleware (role extraction, permission enforcement)
- [ ] Global exception middleware + error response envelope
- [ ] Audit log infrastructure (MediatR behavior)
- [ ] Swagger/Scalar UI setup

### Phase 2 — Salary Engine (Weeks 4–6)

- [ ] Employee Payroll Profile (sync endpoint, history tracking)
- [ ] Salary Components CRUD
- [ ] Salary Structures CRUD with components
- [ ] NCalc Formula Engine (IFormulaEvaluator, validation, sandbox)
- [ ] AllowanceRule entity + evaluation logic
- [ ] DeductionRule entity + opt-in logic
- [ ] Formula Validator endpoint
- [ ] Unit tests: FormulaEvaluator, AllowanceEngine, DeductionEngine

### Phase 3 — Data Ingestion (Week 7)

- [ ] Attendance sync endpoint (single + bulk)
- [ ] Leave sync endpoint (single + bulk)
- [ ] Idempotency key enforcement
- [ ] PayrollCalendar setup

### Phase 4 — Payroll Run Engine (Weeks 8–10)

- [ ] PayrollRun entity + state machine
- [ ] Hangfire setup + PayrollGenerationWorker
- [ ] PayrollCalculatorService (orchestrates formula engine for all components)
- [ ] Snapshot logic (employee, structure, attendance freeze)
- [ ] Error handling for missing attendance, missing structure
- [ ] Manual component override endpoint
- [ ] Integration tests: end-to-end payroll generation

### Phase 5 — Approval & Slips (Weeks 11–12)

- [ ] ApprovalWorkflowTemplate CRUD
- [ ] Approval action endpoints (approve/reject)
- [ ] QuestPDF salary slip template
- [ ] SalarySlip generation job (Hangfire)
- [ ] Supabase Storage upload
- [ ] Email delivery (Resend integration)
- [ ] In-app PDF viewer (signed URL)
- [ ] QR code + public verify endpoint

### Phase 6 — Disputes & Disbursement (Weeks 13–14)

- [ ] DisputeTicket CRUD + status machine
- [ ] DisputeComment
- [ ] Correction run flow
- [ ] DisbursementBatch + BankTransferRecords
- [ ] CSV + Excel bank file generation (ClosedXML)
- [ ] Disbursement confirm flow

### Phase 7 — Loans, Advances, Bonuses (Week 15)

- [ ] EmployeeLoan + LoanInstallment
- [ ] AdvanceSalaryRequest
- [ ] BonusRecord
- [ ] OvertimePolicy
- [ ] Auto-deduction integration into payroll calculator

### Phase 8 — Reports & Frontend (Weeks 16–18)

- [ ] All 13 report endpoints (JSON + Excel + PDF)
- [ ] Async report job for large datasets
- [ ] Webhook registration + outbound event dispatcher
- [ ] Next.js frontend: dashboard, payroll run management, approvals, slip viewer
- [ ] Frontend: dispute management UI
- [ ] Frontend: reports + export

### Phase 9 — QA & Hardening (Weeks 19–20)

- [ ] Full integration test suite
- [ ] Load testing: 1000-employee payroll run
- [ ] Security audit: API key handling, RLS verification, formula sandbox
- [ ] Documentation: OpenAPI spec export, developer onboarding guide
- [ ] Deployment: Railway (API) + Vercel (frontend) + Supabase (DB + Storage)

---

## 30. Glossary

|Term|Definition|
|---|---|
|**PayrollRun**|A batch payroll calculation for a company for a specific month|
|**PayrollEntry**|An individual employee's salary calculation within a PayrollRun|
|**PayrollEntryComponent**|A single salary component (earning or deduction) within a PayrollEntry|
|**SalaryStructure**|A template of salary components and formulas assigned to an employee|
|**SalaryComponent**|An atomic unit of salary: a named, typed, calculable element (Basic, Medical, etc.)|
|**FormulaContext**|The set of variable values passed to the NCalc engine for evaluation|
|**AllowanceRule**|A condition that determines whether a salary component is applied in a given period|
|**DeductionRule**|Configuration for how and when a deduction component is applied|
|**EmployeePayrollProfile**|The minimal employee snapshot maintained by PayrollMS for calculation purposes|
|**Snapshot**|Immutable copy of data taken at payroll generation time; used for historical accuracy|
|**ExternalEmployeeId**|The primary key of the employee in the calling ERP system|
|**PayrollCalendar**|Company-specific definition of working days, holidays, and freeze dates per month|
|**DisbursementBatch**|A grouped bank transfer file containing all employee net salary transfers|
|**DisputeTicket**|A formal challenge raised by an employee against their salary slip|
|**Correction Run**|A new PayrollRun (type=Correction) created to fix a disputed entry|
|**Idempotency Key**|A unique key on attendance/leave sync to prevent duplicate processing|
|**API Key**|Machine-to-machine authentication credential scoped to a company|
|**Tenant**|A company registered in the system; all data is isolated per tenant|
|**Freeze Date**|The date after which a payroll period is locked for editing|
|**NCalc**|The .NET expression evaluation library used for salary formula computation|