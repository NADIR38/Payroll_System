using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;

namespace PayrollMS.Domain.Interfaces.Repositories;

public interface IEmployeePayrollProfileRepository : IRepository<EmployeePayrollProfile, EmployeePayrollProfileId>
{
    Task<EmployeePayrollProfile?> GetByExternalIdAsync(CompanyId companyId, string externalEmployeeId, CancellationToken cancellationToken = default);
    Task<EmployeePayrollProfile?> GetByEmployeeCodeAsync(CompanyId companyId, string employeeCode, CancellationToken cancellationToken = default);
    Task<EmployeePayrollProfile?> GetByIdForUpdateAsync(EmployeePayrollProfileId id, CancellationToken cancellationToken = default);
    Task<EmployeePayrollProfileHistory?> GetActiveHistoryOnDateAsync(EmployeePayrollProfileId profileId, DateOnly date, CancellationToken cancellationToken = default);
}
