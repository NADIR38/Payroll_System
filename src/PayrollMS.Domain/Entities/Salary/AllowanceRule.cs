using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Salary;

/// <summary>
/// Controls conditional applicability of an earning salary structure component.
/// PRD §11.2
/// </summary>
public sealed class AllowanceRule : BaseEntity<AllowanceRuleId>
{
    private AllowanceRule() { }

    public SalaryStructureComponentId SalaryStructureComponentId { get; private set; }
    public CompanyId CompanyId { get; private set; }
    public AllowanceApplicationMode ApplicationMode { get; private set; }
    public string? ConditionExpression { get; private set; }
    public string? Description { get; private set; }

    internal static AllowanceRule Create(
        SalaryStructureComponentId structureComponentId,
        CompanyId companyId,
        AllowanceApplicationMode applicationMode,
        string? conditionExpression = null,
        string? description = null)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "AllowanceRule must belong to a company.");

        if (applicationMode == AllowanceApplicationMode.ConditionalExpression && string.IsNullOrWhiteSpace(conditionExpression))
            throw new BusinessRuleViolationException("ConditionExpressionRequired", "Condition expression is required when application mode is ConditionalExpression.");

        return new AllowanceRule
        {
            Id = AllowanceRuleId.New(),
            SalaryStructureComponentId = structureComponentId,
            CompanyId = companyId,
            ApplicationMode = applicationMode,
            ConditionExpression = conditionExpression?.Trim(),
            Description = description?.Trim()
        };
    }

    internal void Update(
        AllowanceApplicationMode applicationMode,
        string? conditionExpression,
        string? description)
    {
        if (applicationMode == AllowanceApplicationMode.ConditionalExpression && string.IsNullOrWhiteSpace(conditionExpression))
            throw new BusinessRuleViolationException("ConditionExpressionRequired", "Condition expression is required when application mode is ConditionalExpression.");

        ApplicationMode = applicationMode;
        ConditionExpression = conditionExpression?.Trim();
        Description = description?.Trim();

        SetUpdatedAt();
    }
}
