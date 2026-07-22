using MediatR;

namespace PayrollMS.Application.Features.SalaryComponents.Queries.GetSalaryComponentById;

public sealed record GetSalaryComponentByIdQuery(
    Guid Id,
    Guid CompanyId) : IRequest<SalaryComponentResponse>;
