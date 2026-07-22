using MediatR;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.UpdateSalaryComponent;

public sealed record UpdateSalaryComponentCommand(
    Guid ComponentId,
    Guid CompanyId,
    string Name,
    string Type,               // "Earning" | "Deduction" | "EmployerContribution"
    string CalculationMethod,  // "Fixed" | "PercentageOfBase" | "FormulaExpression" | "PerWorkingDay" | "PerHour"
    decimal? DefaultValue,
    bool IsTaxable,
    bool IsRecurring,
    bool IsOptional,
    string? Description) : IRequest;
