using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.CreateSalaryStructure;

public sealed record CreateSalaryStructureCommand(
    Guid CompanyId,
    string Name,
    string Code,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo = null,
    string? Description = null) : IRequest<Guid>;
