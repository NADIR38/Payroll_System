using Microsoft.EntityFrameworkCore;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Infrastructure.Persistence.Repositories;

public class EmployeePayrollProfileRepository : Repository<EmployeePayrollProfile, EmployeePayrollProfileId>, IEmployeePayrollProfileRepository
{
    public EmployeePayrollProfileRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<EmployeePayrollProfile?> GetByExternalIdAsync(CompanyId companyId, string externalEmployeeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.History)
            .Include(e => e.BankAccounts)
            .FirstOrDefaultAsync(e => e.CompanyId == companyId && e.ExternalEmployeeId == externalEmployeeId, cancellationToken);
    }

    public async Task<EmployeePayrollProfile?> GetByEmployeeCodeAsync(CompanyId companyId, string employeeCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.History)
            .Include(e => e.BankAccounts)
            .FirstOrDefaultAsync(e => e.CompanyId == companyId && e.EmployeeCode == employeeCode, cancellationToken);
    }

    public async Task<EmployeePayrollProfile?> GetByIdForUpdateAsync(EmployeePayrollProfileId id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.History)
            .Include(e => e.BankAccounts)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmployeePayrollProfileHistory?> GetActiveHistoryOnDateAsync(EmployeePayrollProfileId profileId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await DbContext.EmployeeProfileHistory
            .FirstOrDefaultAsync(h => h.EmployeePayrollProfileId == profileId &&
                                      h.EffectiveFrom <= date &&
                                      (h.EffectiveTo == null || h.EffectiveTo >= date), cancellationToken);
    }
}
