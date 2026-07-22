using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfileById;

public class GetEmployeeProfileByIdQueryHandler : IRequestHandler<GetEmployeeProfileByIdQuery, EmployeeProfileResponse>
{
    private readonly IAppDbContext _dbContext;

    public GetEmployeeProfileByIdQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EmployeeProfileResponse> Handle(GetEmployeeProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new Domain.Common.EmployeePayrollProfileId(request.Id);

        var profile = await _dbContext.EmployeeProfiles
            .AsNoTracking()
            .Include(e => e.BankAccounts)
            .FirstOrDefaultAsync(e => e.Id == targetId, cancellationToken);

        if (profile == null)
            throw new NotFoundException($"Employee profile with ID '{request.Id}' was not found.");

        return new EmployeeProfileResponse(
            profile.Id.Value,
            profile.CompanyId.Value,
            profile.ExternalEmployeeId,
            profile.EmployeeCode,
            profile.FullName,
            profile.BranchId.Value,
            profile.DepartmentId.Value,
            profile.DesignationId.Value,
            profile.CostCenterId?.Value,
            profile.SalaryStructureId.Value,
            profile.BaseSalary,
            profile.AttendanceDeductionOptIn,
            profile.JoiningDate,
            profile.LeavingDate,
            profile.Status.ToString(),
            profile.EffectiveFrom,
            profile.CreatedAt,
            profile.BankAccounts.Select(b => new EmployeeBankAccountResponse(
                b.Id.Value,
                b.EmployeePayrollProfileId.Value,
                b.BankName,
                b.AccountTitle,
                b.AccountNumber,
                b.IBAN,
                b.BranchCode,
                b.IsPrimary,
                b.IsActive)).ToList()
        );
    }
}
