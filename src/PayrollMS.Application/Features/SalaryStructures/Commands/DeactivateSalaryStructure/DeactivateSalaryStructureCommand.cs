using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.DeactivateSalaryStructure;

public sealed record DeactivateSalaryStructureCommand(
    Guid SalaryStructureId,
    Guid CompanyId) : IRequest;
