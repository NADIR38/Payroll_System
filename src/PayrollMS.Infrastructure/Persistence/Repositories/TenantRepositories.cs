using Microsoft.EntityFrameworkCore;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Infrastructure.Persistence.Repositories;

public class CompanyRepository : Repository<Company, CompanyId>, ICompanyRepository
{
    public CompanyRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<Company?> GetByIdForUpdateAsync(CompanyId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FromSqlInterpolated($"SELECT *, xmin FROM payroll_companies WHERE \"Id\" = {id.Value} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class BranchRepository : Repository<Branch, BranchId>, IBranchRepository
{
    public BranchRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<Branch?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(b => b.CompanyId == companyId && b.Code == code, cancellationToken);
    }

    public async Task<Branch?> GetByIdForUpdateAsync(BranchId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FromSqlInterpolated($"SELECT *, xmin FROM payroll_branches WHERE \"Id\" = {id.Value} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class DepartmentRepository : Repository<Department, DepartmentId>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<Department?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(d => d.CompanyId == companyId && d.Code == code, cancellationToken);
    }

    public async Task<Department?> GetByIdForUpdateAsync(DepartmentId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FromSqlInterpolated($"SELECT *, xmin FROM payroll_departments WHERE \"Id\" = {id.Value} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class DesignationRepository : Repository<Designation, DesignationId>, IDesignationRepository
{
    public DesignationRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<Designation?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(d => d.CompanyId == companyId && d.Code == code, cancellationToken);
    }

    public async Task<Designation?> GetByIdForUpdateAsync(DesignationId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FromSqlInterpolated($"SELECT *, xmin FROM payroll_designations WHERE \"Id\" = {id.Value} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class CostCenterRepository : Repository<CostCenter, CostCenterId>, ICostCenterRepository
{
    public CostCenterRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<CostCenter?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Code == code, cancellationToken);
    }

    public async Task<CostCenter?> GetByIdForUpdateAsync(CostCenterId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FromSqlInterpolated($"SELECT *, xmin FROM payroll_cost_centers WHERE \"Id\" = {id.Value} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class FinancialYearRepository : Repository<FinancialYear, FinancialYearId>, IFinancialYearRepository
{
    public FinancialYearRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<FinancialYear?> GetCurrentAsync(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(f => f.CompanyId == companyId && f.IsCurrent, cancellationToken);
    }

    public async Task<FinancialYear?> GetByIdForUpdateAsync(FinancialYearId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FromSqlInterpolated($"SELECT *, xmin FROM payroll_financial_years WHERE \"Id\" = {id.Value} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class PayrollCalendarRepository : Repository<PayrollCalendar, PayrollCalendarId>, IPayrollCalendarRepository
{
    public PayrollCalendarRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<PayrollCalendar?> GetByMonthYearAsync(CompanyId companyId, int month, int year, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Month == month && p.Year == year, cancellationToken);
    }

    public async Task<PayrollCalendar?> GetByIdForUpdateAsync(PayrollCalendarId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FromSqlInterpolated($"SELECT *, xmin FROM payroll_calendars WHERE \"Id\" = {id.Value} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }
}
