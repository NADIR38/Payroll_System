using PayrollMS.Domain.Common;

namespace PayrollMS.Domain.Events;

// Company Events
public record CompanyCreatedEvent(CompanyId CompanyId, string Name, string Code) : IDomainEvent;
public record CompanyUpdatedEvent(CompanyId CompanyId, string Name) : IDomainEvent;
public record CompanyActivatedEvent(CompanyId CompanyId) : IDomainEvent;
public record CompanyDeactivatedEvent(CompanyId CompanyId) : IDomainEvent;

// Branch Events
public record BranchCreatedEvent(BranchId BranchId, CompanyId CompanyId, string Name, string Code) : IDomainEvent;
public record BranchUpdatedEvent(BranchId BranchId, string Name) : IDomainEvent;
public record BranchActivatedEvent(BranchId BranchId) : IDomainEvent;
public record BranchDeactivatedEvent(BranchId BranchId) : IDomainEvent;

// Department Events
public record DepartmentCreatedEvent(DepartmentId DepartmentId, CompanyId CompanyId, string Name, string Code) : IDomainEvent;
public record DepartmentUpdatedEvent(DepartmentId DepartmentId, string Name, BranchId? BranchId) : IDomainEvent;
public record DepartmentActivatedEvent(DepartmentId DepartmentId) : IDomainEvent;
public record DepartmentDeactivatedEvent(DepartmentId DepartmentId) : IDomainEvent;

// Designation Events
public record DesignationCreatedEvent(DesignationId DesignationId, CompanyId CompanyId, string Name, string Code, string? Grade) : IDomainEvent;
public record DesignationUpdatedEvent(DesignationId DesignationId, string Name, string? Grade) : IDomainEvent;
public record DesignationActivatedEvent(DesignationId DesignationId) : IDomainEvent;
public record DesignationDeactivatedEvent(DesignationId DesignationId) : IDomainEvent;

// Cost Center Events
public record CostCenterCreatedEvent(CostCenterId CostCenterId, CompanyId CompanyId, string Name, string Code) : IDomainEvent;
public record CostCenterUpdatedEvent(CostCenterId CostCenterId, string Name) : IDomainEvent;
public record CostCenterActivatedEvent(CostCenterId CostCenterId) : IDomainEvent;
public record CostCenterDeactivatedEvent(CostCenterId CostCenterId) : IDomainEvent;

// Financial Year Events
public record FinancialYearCreatedEvent(FinancialYearId FinancialYearId, CompanyId CompanyId, string Label) : IDomainEvent;
public record FinancialYearMarkedCurrentEvent(FinancialYearId FinancialYearId, CompanyId CompanyId) : IDomainEvent;
public record FinancialYearCurrentRemovedEvent(FinancialYearId FinancialYearId, CompanyId CompanyId) : IDomainEvent;

// Payroll Calendar Events
public record PayrollCalendarCreatedEvent(PayrollCalendarId CalendarId, CompanyId CompanyId, int Month, int Year) : IDomainEvent;
public record PayrollCalendarFrozenEvent(PayrollCalendarId CalendarId) : IDomainEvent;
public record PayrollCalendarClosedEvent(PayrollCalendarId CalendarId) : IDomainEvent;
public record PayrollCalendarReopenedEvent(PayrollCalendarId CalendarId) : IDomainEvent;
