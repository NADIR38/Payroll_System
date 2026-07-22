using StronglyTypedIds;

namespace PayrollMS.Domain.Common;

// ==================== MODULE 01 — TENANT ====================

[StronglyTypedId(Template.Guid)]
public partial struct CompanyId {}

[StronglyTypedId(Template.Guid)]
public partial struct BranchId {}

[StronglyTypedId(Template.Guid)]
public partial struct DepartmentId {}

[StronglyTypedId(Template.Guid)]
public partial struct DesignationId {}

[StronglyTypedId(Template.Guid)]
public partial struct CostCenterId {}

[StronglyTypedId(Template.Guid)]
public partial struct FinancialYearId {}

[StronglyTypedId(Template.Guid)]
public partial struct PayrollCalendarId {}

// ==================== MODULE 02 — EMPLOYEE PROFILE ====================

[StronglyTypedId(Template.Guid)]
public partial struct EmployeePayrollProfileId {}

[StronglyTypedId(Template.Guid)]
public partial struct EmployeePayrollProfileHistoryId {}

[StronglyTypedId(Template.Guid)]
public partial struct EmployeeBankAccountId {}

// ==================== MODULE 03 — SALARY COMPONENTS ====================

[StronglyTypedId(Template.Guid)]
public partial struct SalaryComponentId {}

// ==================== MODULE 04 — SALARY STRUCTURES ====================

[StronglyTypedId(Template.Guid)]
public partial struct SalaryStructureId {}

[StronglyTypedId(Template.Guid)]
public partial struct SalaryStructureComponentId {}

[StronglyTypedId(Template.Guid)]
public partial struct AllowanceRuleId {}

[StronglyTypedId(Template.Guid)]
public partial struct DeductionRuleId {}

// ==================== MODULE 08 — ATTENDANCE ====================

[StronglyTypedId(Template.Guid)]
public partial struct AttendanceSummaryId {}

// ==================== MODULE 09 — LEAVE ====================

[StronglyTypedId(Template.Guid)]
public partial struct LeaveSummaryId {}

// ==================== MODULE 10 — PAYROLL RUN ENGINE ====================

[StronglyTypedId(Template.Guid)]
public partial struct PayrollRunId {}

[StronglyTypedId(Template.Guid)]
public partial struct PayrollEntryId {}

[StronglyTypedId(Template.Guid)]
public partial struct PayrollEntryComponentId {}

// ==================== MODULE 11 — APPROVAL WORKFLOW ====================

[StronglyTypedId(Template.Guid)]
public partial struct ApprovalWorkflowTemplateId {}

[StronglyTypedId(Template.Guid)]
public partial struct ApprovalWorkflowStepId {}

[StronglyTypedId(Template.Guid)]
public partial struct PayrollApprovalRecordId {}
