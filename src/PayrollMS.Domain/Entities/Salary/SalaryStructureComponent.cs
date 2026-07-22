using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Salary;

/// <summary>
/// Connects a SalaryComponent to a SalaryStructure with a sequence order, formula expression,
/// and optional allowance/deduction rules.
/// PRD §9.2
/// </summary>
public sealed class SalaryStructureComponent : BaseEntity<SalaryStructureComponentId>
{
    private SalaryStructureComponent() { }

    public SalaryStructureId SalaryStructureId { get; private set; }
    public SalaryComponentId SalaryComponentId { get; private set; }
    public string FormulaExpression { get; private set; } = null!;
    public decimal? FixedAmount { get; private set; }
    public int Sequence { get; private set; }
    public bool IsActive { get; private set; }

    public SalaryComponent Component { get; private set; } = null!;
    public AllowanceRule? AllowanceRule { get; private set; }
    public DeductionRule? DeductionRule { get; private set; }

    internal static SalaryStructureComponent Create(
        SalaryStructureId salaryStructureId,
        SalaryComponentId salaryComponentId,
        string formulaExpression,
        int sequence,
        decimal? fixedAmount = null)
    {
        if (salaryStructureId == SalaryStructureId.Empty)
            throw new BusinessRuleViolationException("StructureRequired", "SalaryStructureComponent must belong to a salary structure.");

        if (salaryComponentId == SalaryComponentId.Empty)
            throw new BusinessRuleViolationException("ComponentRequired", "SalaryStructureComponent must reference a salary component.");

        if (string.IsNullOrWhiteSpace(formulaExpression))
            throw new BusinessRuleViolationException("FormulaRequired", "Formula expression is required.");

        if (sequence < 1)
            throw new BusinessRuleViolationException("InvalidSequence", "Sequence must be a positive integer starting from 1.");

        return new SalaryStructureComponent
        {
            Id = SalaryStructureComponentId.New(),
            SalaryStructureId = salaryStructureId,
            SalaryComponentId = salaryComponentId,
            FormulaExpression = formulaExpression.Trim(),
            Sequence = sequence,
            FixedAmount = fixedAmount,
            IsActive = true
        };
    }

    internal void Update(
        string formulaExpression,
        int sequence,
        decimal? fixedAmount)
    {
        if (string.IsNullOrWhiteSpace(formulaExpression))
            throw new BusinessRuleViolationException("FormulaRequired", "Formula expression is required.");

        if (sequence < 1)
            throw new BusinessRuleViolationException("InvalidSequence", "Sequence must be a positive integer starting from 1.");

        FormulaExpression = formulaExpression.Trim();
        Sequence = sequence;
        FixedAmount = fixedAmount;

        SetUpdatedAt();
    }

    internal void SetSequence(int sequence)
    {
        if (sequence < 1)
            throw new BusinessRuleViolationException("InvalidSequence", "Sequence must be a positive integer starting from 1.");

        Sequence = sequence;
        SetUpdatedAt();
    }

    internal AllowanceRule SetAllowanceRule(
        CompanyId companyId,
        AllowanceApplicationMode applicationMode,
        string? conditionExpression = null,
        string? description = null)
    {
        if (AllowanceRule is null)
        {
            AllowanceRule = AllowanceRule.Create(Id, companyId, applicationMode, conditionExpression, description);
        }
        else
        {
            AllowanceRule.Update(applicationMode, conditionExpression, description);
        }

        SetUpdatedAt();
        return AllowanceRule;
    }

    internal DeductionRule SetDeductionRule(
        CompanyId companyId,
        DeductionType deductionType,
        bool isOptIn = false,
        int gracePeriodMinutes = 0,
        string? deductionFormula = null)
    {
        if (DeductionRule is null)
        {
            DeductionRule = DeductionRule.Create(Id, companyId, deductionType, isOptIn, gracePeriodMinutes, deductionFormula);
        }
        else
        {
            DeductionRule.Update(deductionType, isOptIn, gracePeriodMinutes, deductionFormula);
        }

        SetUpdatedAt();
        return DeductionRule;
    }

    internal void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    internal void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
