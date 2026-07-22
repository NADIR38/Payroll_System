using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Queries.GetSalaryStructureById;

public sealed record GetSalaryStructureByIdQuery(
    Guid Id,
    Guid CompanyId) : IRequest<SalaryStructureResponse>;
