using MediatR;

namespace PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfileHistory;

public sealed record GetEmployeeProfileHistoryQuery(Guid EmployeeProfileId) : IRequest<IReadOnlyList<EmployeeProfileHistoryResponse>>;
