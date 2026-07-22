using MediatR;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.CreateSalaryComponent;

public sealed record CreateSalaryComponentCommand(
    Guid CompanyId,
    string Name,
    string Code,
    string Type,               // "Earning" | "Deduction" | "EmployerContribution"
    string CalculationMethod,  // "Fixed" | "PercentageOfBase" | "FormulaExpression" | "PerWorkingDay" | "PerHour"
    decimal? DefaultValue = null,
    bool IsTaxable = true,
    bool IsRecurring = true,
    bool IsOptional = false,
    string? Description = null) : IRequest<Guid>;
