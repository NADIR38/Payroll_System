using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;

namespace PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfiles;

public class GetEmployeeProfilesQueryHandler : IRequestHandler<GetEmployeeProfilesQuery, IReadOnlyList<EmployeeProfileResponse>>
{
    private readonly IAppDbContext _dbContext;

    public GetEmployeeProfilesQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeProfileResponse>> Handle(GetEmployeeProfilesQuery request, CancellationToken cancellationToken)
    {
        var targetCompanyId = new Domain.Common.CompanyId(request.CompanyId);
        var query = _dbContext.EmployeeProfiles
            .AsNoTracking()
            .Include(e => e.BankAccounts)
            .Where(e => e.CompanyId == targetCompanyId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(search) ||
                e.EmployeeCode.ToLower().Contains(search) ||
                e.ExternalEmployeeId.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.StatusFilter) && Enum.TryParse<Domain.Enums.EmployeeStatus>(request.StatusFilter, true, out var statusEnum))
        {
            query = query.Where(e => e.Status == statusEnum);
        }

        if (request.BranchId.HasValue)
        {
            var targetBranchId = new Domain.Common.BranchId(request.BranchId.Value);
            query = query.Where(e => e.BranchId == targetBranchId);
        }

        if (request.DepartmentId.HasValue)
        {
            var targetDepartmentId = new Domain.Common.DepartmentId(request.DepartmentId.Value);
            query = query.Where(e => e.DepartmentId == targetDepartmentId);
        }

        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var size = request.PageSize < 1 ? 50 : (request.PageSize > 200 ? 200 : request.PageSize);

        var profiles = await query
            .OrderBy(e => e.FullName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return profiles.Select(p => new EmployeeProfileResponse(
            p.Id.Value,
            p.CompanyId.Value,
            p.ExternalEmployeeId,
            p.EmployeeCode,
            p.FullName,
            p.BranchId.Value,
            p.DepartmentId.Value,
            p.DesignationId.Value,
            p.CostCenterId?.Value,
            p.SalaryStructureId.Value,
            p.BaseSalary,
            p.AttendanceDeductionOptIn,
            p.JoiningDate,
            p.LeavingDate,
            p.Status.ToString(),
            p.EffectiveFrom,
            p.CreatedAt,
            p.BankAccounts.Select(b => new EmployeeBankAccountResponse(
                b.Id.Value,
                b.EmployeePayrollProfileId.Value,
                b.BankName,
                b.AccountTitle,
                b.AccountNumber,
                b.IBAN,
                b.BranchCode,
                b.IsPrimary,
                b.IsActive)).ToList()
        )).ToList();
    }
}
