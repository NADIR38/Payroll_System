using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.RemoveComponentFromStructure;

public sealed record RemoveComponentFromStructureCommand(
    Guid SalaryStructureId,
    Guid CompanyId,
    Guid StructureComponentId) : IRequest;
