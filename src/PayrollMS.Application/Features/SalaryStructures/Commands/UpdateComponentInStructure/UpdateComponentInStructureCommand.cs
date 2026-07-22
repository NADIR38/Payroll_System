using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.UpdateComponentInStructure;

public sealed record UpdateComponentInStructureCommand(
    Guid SalaryStructureId,
    Guid CompanyId,
    Guid StructureComponentId,
    string FormulaExpression,
    int Sequence,
    decimal? FixedAmount = null) : IRequest;
