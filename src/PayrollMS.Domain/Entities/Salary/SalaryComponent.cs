using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Salary;

/// <summary>
/// Atomic vocabulary component of payroll (e.g. BASIC, HOUSE_RENT, MEDICAL).
/// Reusable across multiple salary structures.
/// PRD §8.2
/// </summary>
public sealed class SalaryComponent : BaseAuditableEntity<SalaryComponentId>, IAggregateRoot
{
    private SalaryComponent() { }

    public CompanyId CompanyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public ComponentType Type { get; private set; }
    public CalculationMethod CalculationMethod { get; private set; }
    public decimal? DefaultValue { get; private set; }
    public bool IsTaxable { get; private set; }
    public bool IsRecurring { get; private set; }
    public bool IsOptional { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static SalaryComponent Create(
        CompanyId companyId,
        string name,
        string code,
        ComponentType type,
        CalculationMethod calculationMethod,
        decimal? defaultValue = null,
        bool isTaxable = true,
        bool isRecurring = true,
        bool isOptional = false,
        string? description = null)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "SalaryComponent must belong to a company.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Component name cannot be empty.");

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("CodeRequired", "Component code cannot be empty.");

        var component = new SalaryComponent
        {
            Id = SalaryComponentId.New(),
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Type = type,
            CalculationMethod = calculationMethod,
            DefaultValue = defaultValue,
            IsTaxable = isTaxable,
            IsRecurring = isRecurring,
            IsOptional = isOptional,
            IsActive = true,
            Description = description?.Trim()
        };

        component.AddDomainEvent(new SalaryComponentCreatedEvent(
            component.Id, component.CompanyId, component.Code, component.Name, component.Type));

        return component;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public void Update(
        string name,
        ComponentType type,
        CalculationMethod calculationMethod,
        decimal? defaultValue,
        bool isTaxable,
        bool isRecurring,
        bool isOptional,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Component name cannot be empty.");

        Name = name.Trim();
        Type = type;
        CalculationMethod = calculationMethod;
        DefaultValue = defaultValue;
        IsTaxable = isTaxable;
        IsRecurring = isRecurring;
        IsOptional = isOptional;
        Description = description?.Trim();

        SetUpdatedAt();

        AddDomainEvent(new SalaryComponentUpdatedEvent(Id, CompanyId, Name));
    }

    // ── Deactivate / Activate (Soft delete) ───────────────────────────────────

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        SetUpdatedAt();

        AddDomainEvent(new SalaryComponentDeactivatedEvent(Id, CompanyId));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        SetUpdatedAt();
    }
}
