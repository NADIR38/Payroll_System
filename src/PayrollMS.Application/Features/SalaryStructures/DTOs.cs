namespace PayrollMS.Application.Features.SalaryStructures;

/// <summary>
/// Response DTO for a Salary Structure with its embedded components and rules.
/// </summary>
public sealed record SalaryStructureResponse(
    Guid Id,
    Guid CompanyId,
    string Name,
    string Code,
    string? Description,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyList<SalaryStructureComponentResponse> Components);

public sealed record SalaryStructureComponentResponse(
    Guid Id,
    Guid SalaryStructureId,
    Guid SalaryComponentId,
    string ComponentCode,
    string ComponentName,
    string ComponentType,
    string CalculationMethod,
    string FormulaExpression,
    int Sequence,
    decimal? FixedAmount,
    bool IsActive,
    AllowanceRuleResponse? AllowanceRule,
    DeductionRuleResponse? DeductionRule);

public sealed record AllowanceRuleResponse(
    Guid Id,
    Guid SalaryStructureComponentId,
    Guid CompanyId,
    string ApplicationMode,
    string? ConditionExpression,
    string? Description,
    bool IsActive);

public sealed record DeductionRuleResponse(
    Guid Id,
    Guid SalaryStructureComponentId,
    Guid CompanyId,
    string DeductionType,
    bool IsOptIn,
    int GracePeriodMinutes,
    string? DeductionFormula,
    bool IsActive);

public sealed record FormulaValidationResult(
    bool IsValid,
    string? ErrorMessage,
    IReadOnlyList<string> ExtractedVariables,
    decimal? DryRunSampleValue);
