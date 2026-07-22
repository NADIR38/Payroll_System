namespace PayrollMS.Application.Features.SalaryComponents;

/// <summary>
/// Response DTO for a Salary Component.
/// </summary>
public sealed record SalaryComponentResponse(
    Guid Id,
    Guid CompanyId,
    string Name,
    string Code,
    string Type,
    string CalculationMethod,
    decimal? DefaultValue,
    bool IsTaxable,
    bool IsRecurring,
    bool IsOptional,
    bool IsActive,
    string? Description,
    DateTimeOffset CreatedAt);

/// <summary>
/// Result DTO for seeding predefined salary components.
/// </summary>
public sealed record PredefinedComponentSeedResult(
    int TotalPredefinedCount,
    int NewlySeededCount,
    IReadOnlyList<string> SeededCodes);
