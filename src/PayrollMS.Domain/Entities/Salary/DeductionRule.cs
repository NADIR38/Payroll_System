using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Salary;

/// <summary>
/// Configurable rules for salary component deductions.
/// PRD §12.2
/// </summary>
public sealed class DeductionRule : BaseEntity<DeductionRuleId>
{
    private DeductionRule() { }

    public SalaryStructureComponentId SalaryStructureComponentId { get; private set; }
    public CompanyId CompanyId { get; private set; }
    public DeductionType DeductionType { get; private set; }
    public bool IsOptIn { get; private set; }
    public int GracePeriodMinutes { get; private set; }
    public string? DeductionFormula { get; private set; }
    public bool IsActive { get; private set; }

    internal static DeductionRule Create(
        SalaryStructureComponentId structureComponentId,
        CompanyId companyId,
        DeductionType deductionType,
        bool isOptIn = false,
        int gracePeriodMinutes = 0,
        string? deductionFormula = null)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "DeductionRule must belong to a company.");

        if (gracePeriodMinutes < 0)
            throw new BusinessRuleViolationException("InvalidGracePeriod", "Grace period minutes cannot be negative.");

        return new DeductionRule
        {
            Id = DeductionRuleId.New(),
            SalaryStructureComponentId = structureComponentId,
            CompanyId = companyId,
            DeductionType = deductionType,
            IsOptIn = isOptIn,
            GracePeriodMinutes = gracePeriodMinutes,
            DeductionFormula = deductionFormula?.Trim(),
            IsActive = true
        };
    }

    internal void Update(
        DeductionType deductionType,
        bool isOptIn,
        int gracePeriodMinutes,
        string? deductionFormula)
    {
        if (gracePeriodMinutes < 0)
            throw new BusinessRuleViolationException("InvalidGracePeriod", "Grace period minutes cannot be negative.");

        DeductionType = deductionType;
        IsOptIn = isOptIn;
        GracePeriodMinutes = gracePeriodMinutes;
        DeductionFormula = deductionFormula?.Trim();

        SetUpdatedAt();
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
