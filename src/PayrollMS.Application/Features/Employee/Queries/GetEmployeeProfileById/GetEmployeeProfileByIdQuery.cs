using MediatR;

namespace PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfileById;

public sealed record GetEmployeeProfileByIdQuery(Guid Id) : IRequest<EmployeeProfileResponse>;
