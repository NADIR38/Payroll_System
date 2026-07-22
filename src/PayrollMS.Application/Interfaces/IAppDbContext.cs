using PayrollMS.Domain.Entities.Employee;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Entities.Tenant;

namespace PayrollMS.Application.Interfaces;

/// <summary>
/// Application database context interface.
/// Pure database-agnostic interface returning IQueryable sets without EF Core dependencies.
/// </summary>
public interface IAppDbContext
{
    IQueryable<Company> Companies { get; }
    IQueryable<Branch> Branches { get; }
    IQueryable<Department> Departments { get; }
    IQueryable<Designation> Designations { get; }
    IQueryable<CostCenter> CostCenters { get; }
    IQueryable<FinancialYear> FinancialYears { get; }
    IQueryable<PayrollCalendar> PayrollCalendars { get; }

    IQueryable<EmployeePayrollProfile> EmployeeProfiles { get; }
    IQueryable<EmployeePayrollProfileHistory> EmployeeProfileHistory { get; }
    IQueryable<EmployeeBankAccount> EmployeeBankAccounts { get; }

    IQueryable<SalaryComponent> SalaryComponents { get; }
    IQueryable<SalaryStructure> SalaryStructures { get; }
    IQueryable<SalaryStructureComponent> SalaryStructureComponents { get; }
    IQueryable<AllowanceRule> AllowanceRules { get; }
    IQueryable<DeductionRule> DeductionRules { get; }

    IQueryable<PayrollMS.Domain.Entities.Payroll.AttendanceSummary> AttendanceSummaries { get; }
    IQueryable<PayrollMS.Domain.Entities.Payroll.LeaveSummary> LeaveSummaries { get; }
    IQueryable<PayrollMS.Domain.Entities.Payroll.PayrollRun> PayrollRuns { get; }
    IQueryable<PayrollMS.Domain.Entities.Payroll.PayrollEntry> PayrollEntries { get; }
    IQueryable<PayrollMS.Domain.Entities.Payroll.PayrollEntryComponent> PayrollEntryComponents { get; }
    IQueryable<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowTemplate> ApprovalWorkflowTemplates { get; }
    IQueryable<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowStep> ApprovalWorkflowSteps { get; }
    IQueryable<PayrollMS.Domain.Entities.Approval.PayrollApprovalRecord> PayrollApprovalRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
