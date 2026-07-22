using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.AddComponentToStructure;

public sealed record AddComponentToStructureCommand(
    Guid SalaryStructureId,
    Guid CompanyId,
    Guid SalaryComponentId,
    string FormulaExpression,
    int Sequence,
    decimal? FixedAmount = null) : IRequest<Guid>;
