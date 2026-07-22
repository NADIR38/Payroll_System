using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Queries.GetSalaryStructures;

public sealed record GetSalaryStructuresQuery(
    Guid CompanyId,
    bool? IsActiveOnly = true) : IRequest<IReadOnlyList<SalaryStructureResponse>>;
