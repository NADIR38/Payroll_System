using MediatR;

namespace PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfiles;

public sealed record GetEmployeeProfilesQuery(
    Guid CompanyId,
    string? SearchTerm = null,
    string? StatusFilter = null,
    Guid? BranchId = null,
    Guid? DepartmentId = null,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<IReadOnlyList<EmployeeProfileResponse>>;
