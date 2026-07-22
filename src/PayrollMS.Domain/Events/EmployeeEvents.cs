using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Domain.Events;

// Employee Payroll Profile Events
public record EmployeeProfileCreatedEvent(
    EmployeePayrollProfileId ProfileId,
    CompanyId CompanyId,
    string ExternalEmployeeId,
    string EmployeeCode,
    string FullName) : IDomainEvent;

public record EmployeeProfileUpdatedEvent(
    EmployeePayrollProfileId ProfileId,
    CompanyId CompanyId,
    string ExternalEmployeeId,
    string ChangedBy,
    string ChangeReason) : IDomainEvent;

public record EmployeeProfileStatusChangedEvent(
    EmployeePayrollProfileId ProfileId,
    CompanyId CompanyId,
    EmployeeStatus NewStatus) : IDomainEvent;

// Employee Bank Account Events
public record EmployeeBankAccountAddedEvent(
    EmployeeBankAccountId AccountId,
    EmployeePayrollProfileId ProfileId,
    CompanyId CompanyId,
    bool IsPrimary) : IDomainEvent;

public record EmployeeBankAccountUpdatedEvent(
    EmployeeBankAccountId AccountId,
    EmployeePayrollProfileId ProfileId) : IDomainEvent;

public record EmployeeBankAccountSetPrimaryEvent(
    EmployeeBankAccountId AccountId,
    EmployeePayrollProfileId ProfileId) : IDomainEvent;
