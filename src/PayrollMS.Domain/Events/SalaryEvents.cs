using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Domain.Events;

// Salary Component Events
public record SalaryComponentCreatedEvent(
    SalaryComponentId ComponentId,
    CompanyId CompanyId,
    string Code,
    string Name,
    ComponentType Type) : IDomainEvent;

public record SalaryComponentUpdatedEvent(
    SalaryComponentId ComponentId,
    CompanyId CompanyId,
    string Name) : IDomainEvent;

public record SalaryComponentDeactivatedEvent(
    SalaryComponentId ComponentId,
    CompanyId CompanyId) : IDomainEvent;

// Salary Structure Events
public record SalaryStructureCreatedEvent(
    SalaryStructureId StructureId,
    CompanyId CompanyId,
    string Code,
    string Name) : IDomainEvent;

public record SalaryStructureUpdatedEvent(
    SalaryStructureId StructureId,
    CompanyId CompanyId,
    string Name) : IDomainEvent;

public record SalaryStructureDeactivatedEvent(
    SalaryStructureId StructureId,
    CompanyId CompanyId) : IDomainEvent;

// Salary Structure Component Events
public record StructureComponentAddedEvent(
    SalaryStructureComponentId StructureComponentId,
    SalaryStructureId StructureId,
    SalaryComponentId ComponentId,
    int Sequence) : IDomainEvent;

public record StructureComponentUpdatedEvent(
    SalaryStructureComponentId StructureComponentId,
    SalaryStructureId StructureId) : IDomainEvent;

public record StructureComponentRemovedEvent(
    SalaryStructureComponentId StructureComponentId,
    SalaryStructureId StructureId) : IDomainEvent;

// Allowance / Deduction Rule Events
public record AllowanceRuleSetEvent(
    AllowanceRuleId RuleId,
    SalaryStructureComponentId StructureComponentId) : IDomainEvent;

public record DeductionRuleSetEvent(
    DeductionRuleId RuleId,
    SalaryStructureComponentId StructureComponentId) : IDomainEvent;
