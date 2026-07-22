using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Salary;

/// <summary>
/// A reusable template grouping salary components with evaluation sequence numbers and NCalc formulas.
/// PRD §9.2
/// Aggregate Root.
/// </summary>
public sealed class SalaryStructure : BaseAuditableEntity<SalaryStructureId>, IAggregateRoot
{
    private readonly List<SalaryStructureComponent> _components = [];

    private SalaryStructure() { }

    public CompanyId CompanyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<SalaryStructureComponent> Components => _components.AsReadOnly();

    // ── Factory ───────────────────────────────────────────────────────────────

    public static SalaryStructure Create(
        CompanyId companyId,
        string name,
        string code,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo = null,
        string? description = null)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "SalaryStructure must belong to a company.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Salary structure name cannot be empty.");

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("CodeRequired", "Salary structure code cannot be empty.");

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new BusinessRuleViolationException("InvalidEffectiveTo", "EffectiveTo cannot be earlier than EffectiveFrom.");

        var structure = new SalaryStructure
        {
            Id = SalaryStructureId.New(),
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true
        };

        structure.AddDomainEvent(new SalaryStructureCreatedEvent(
            structure.Id, structure.CompanyId, structure.Code, structure.Name));

        return structure;
    }

    // ── Update Metadata ───────────────────────────────────────────────────────

    public void UpdateDetails(
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Salary structure name cannot be empty.");

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new BusinessRuleViolationException("InvalidEffectiveTo", "EffectiveTo cannot be earlier than EffectiveFrom.");

        Name = name.Trim();
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Description = description?.Trim();

        SetUpdatedAt();

        AddDomainEvent(new SalaryStructureUpdatedEvent(Id, CompanyId, Name));
    }

    // ── Component Management ──────────────────────────────────────────────────

    public SalaryStructureComponent AddComponent(
        SalaryComponentId salaryComponentId,
        string formulaExpression,
        int sequence,
        decimal? fixedAmount = null)
    {
        if (_components.Any(c => c.SalaryComponentId == salaryComponentId && c.IsActive))
            throw new BusinessRuleViolationException("DuplicateComponent", "This component is already included in the salary structure.");

        if (_components.Any(c => c.Sequence == sequence && c.IsActive))
            throw new BusinessRuleViolationException("DuplicateSequence", $"Sequence {sequence} is already assigned to another component in this structure.");

        var component = SalaryStructureComponent.Create(Id, salaryComponentId, formulaExpression, sequence, fixedAmount);
        _components.Add(component);

        SetUpdatedAt();

        AddDomainEvent(new StructureComponentAddedEvent(component.Id, Id, salaryComponentId, sequence));

        return component;
    }

    public void UpdateComponent(
        SalaryStructureComponentId structureComponentId,
        string formulaExpression,
        int sequence,
        decimal? fixedAmount)
    {
        var component = _components.FirstOrDefault(c => c.Id == structureComponentId && c.IsActive)
            ?? throw new BusinessRuleViolationException("ComponentNotFound", "Structure component not found or is inactive.");

        if (_components.Any(c => c.Id != structureComponentId && c.Sequence == sequence && c.IsActive))
            throw new BusinessRuleViolationException("DuplicateSequence", $"Sequence {sequence} is already assigned to another component in this structure.");

        component.Update(formulaExpression, sequence, fixedAmount);

        SetUpdatedAt();

        AddDomainEvent(new StructureComponentUpdatedEvent(structureComponentId, Id));
    }

    public void RemoveComponent(SalaryStructureComponentId structureComponentId)
    {
        var component = _components.FirstOrDefault(c => c.Id == structureComponentId && c.IsActive)
            ?? throw new BusinessRuleViolationException("ComponentNotFound", "Structure component not found or is inactive.");

        component.Deactivate();

        SetUpdatedAt();

        AddDomainEvent(new StructureComponentRemovedEvent(structureComponentId, Id));
    }

    public AllowanceRule SetAllowanceRule(
        SalaryStructureComponentId structureComponentId,
        AllowanceApplicationMode applicationMode,
        string? conditionExpression = null,
        string? description = null)
    {
        var component = _components.FirstOrDefault(c => c.Id == structureComponentId && c.IsActive)
            ?? throw new BusinessRuleViolationException("ComponentNotFound", "Structure component not found or is inactive.");

        var rule = component.SetAllowanceRule(CompanyId, applicationMode, conditionExpression, description);

        SetUpdatedAt();

        AddDomainEvent(new AllowanceRuleSetEvent(rule.Id, structureComponentId));

        return rule;
    }

    public DeductionRule SetDeductionRule(
        SalaryStructureComponentId structureComponentId,
        DeductionType deductionType,
        bool isOptIn = false,
        int gracePeriodMinutes = 0,
        string? deductionFormula = null)
    {
        var component = _components.FirstOrDefault(c => c.Id == structureComponentId && c.IsActive)
            ?? throw new BusinessRuleViolationException("ComponentNotFound", "Structure component not found or is inactive.");

        var rule = component.SetDeductionRule(CompanyId, deductionType, isOptIn, gracePeriodMinutes, deductionFormula);

        SetUpdatedAt();

        AddDomainEvent(new DeductionRuleSetEvent(rule.Id, structureComponentId));

        return rule;
    }

    // ── Deactivate / Activate ─────────────────────────────────────────────────

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        SetUpdatedAt();

        AddDomainEvent(new SalaryStructureDeactivatedEvent(Id, CompanyId));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        SetUpdatedAt();
    }
}
