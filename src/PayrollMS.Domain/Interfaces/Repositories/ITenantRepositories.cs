using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;

namespace PayrollMS.Domain.Interfaces.Repositories;

public interface ICompanyRepository : IRepository<Company, CompanyId>
{
    Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Company?> GetByIdForUpdateAsync(CompanyId id, CancellationToken cancellationToken = default);
}

public interface IBranchRepository : IRepository<Branch, BranchId>
{
    Task<Branch?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default);
    Task<Branch?> GetByIdForUpdateAsync(BranchId id, CancellationToken cancellationToken = default);
}

public interface IDepartmentRepository : IRepository<Department, DepartmentId>
{
    Task<Department?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default);
    Task<Department?> GetByIdForUpdateAsync(DepartmentId id, CancellationToken cancellationToken = default);
}

public interface IDesignationRepository : IRepository<Designation, DesignationId>
{
    Task<Designation?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default);
    Task<Designation?> GetByIdForUpdateAsync(DesignationId id, CancellationToken cancellationToken = default);
}

public interface ICostCenterRepository : IRepository<CostCenter, CostCenterId>
{
    Task<CostCenter?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default);
    Task<CostCenter?> GetByIdForUpdateAsync(CostCenterId id, CancellationToken cancellationToken = default);
}

public interface IFinancialYearRepository : IRepository<FinancialYear, FinancialYearId>
{
    Task<FinancialYear?> GetCurrentAsync(CompanyId companyId, CancellationToken cancellationToken = default);
    Task<FinancialYear?> GetByIdForUpdateAsync(FinancialYearId id, CancellationToken cancellationToken = default);
}

public interface IPayrollCalendarRepository : IRepository<PayrollCalendar, PayrollCalendarId>
{
    Task<PayrollCalendar?> GetByMonthYearAsync(CompanyId companyId, int month, int year, CancellationToken cancellationToken = default);
    Task<PayrollCalendar?> GetByIdForUpdateAsync(PayrollCalendarId id, CancellationToken cancellationToken = default);
}
