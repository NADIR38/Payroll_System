using Microsoft.EntityFrameworkCore;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Infrastructure.Persistence.Repositories;

public class SalaryComponentRepository : Repository<SalaryComponent, SalaryComponentId>, ISalaryComponentRepository
{
    public SalaryComponentRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<SalaryComponent?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Code == code, cancellationToken);
    }

    public async Task<SalaryComponent?> GetByIdForUpdateAsync(SalaryComponentId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> IsComponentUsedInActiveStructuresAsync(SalaryComponentId componentId, CancellationToken cancellationToken = default)
    {
        return await DbContext.SalaryStructureComponents
            .AnyAsync(sc => sc.SalaryComponentId == componentId && sc.IsActive, cancellationToken);
    }
}

public class SalaryStructureRepository : Repository<SalaryStructure, SalaryStructureId>, ISalaryStructureRepository
{
    public SalaryStructureRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<SalaryStructure?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Components.Where(c => c.IsActive))
                .ThenInclude(c => c.Component)
            .Include(s => s.Components.Where(c => c.IsActive))
                .ThenInclude(c => c.AllowanceRule)
            .Include(s => s.Components.Where(c => c.IsActive))
                .ThenInclude(c => c.DeductionRule)
            .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.Code == code, cancellationToken);
    }

    public async Task<SalaryStructure?> GetByIdWithComponentsAsync(SalaryStructureId id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Components.Where(c => c.IsActive))
                .ThenInclude(c => c.Component)
            .Include(s => s.Components.Where(c => c.IsActive))
                .ThenInclude(c => c.AllowanceRule)
            .Include(s => s.Components.Where(c => c.IsActive))
                .ThenInclude(c => c.DeductionRule)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<SalaryStructure?> GetByIdForUpdateAsync(SalaryStructureId id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Components.Where(c => c.IsActive))
                .ThenInclude(c => c.AllowanceRule)
            .Include(s => s.Components.Where(c => c.IsActive))
                .ThenInclude(c => c.DeductionRule)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> HasAssignedEmployeesAsync(SalaryStructureId structureId, CancellationToken cancellationToken = default)
    {
        return await DbContext.EmployeeProfiles
            .AnyAsync(e => e.SalaryStructureId == structureId && e.Status != Domain.Enums.EmployeeStatus.Terminated, cancellationToken);
    }
}
