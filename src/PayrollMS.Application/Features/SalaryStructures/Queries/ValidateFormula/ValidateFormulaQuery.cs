using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Queries.ValidateFormula;

public sealed record ValidateFormulaQuery(
    string FormulaExpression,
    IEnumerable<string>? CustomAvailableVariables = null) : IRequest<FormulaValidationResult>;
