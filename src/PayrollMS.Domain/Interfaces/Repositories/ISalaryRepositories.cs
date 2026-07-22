using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;

namespace PayrollMS.Domain.Interfaces.Repositories;

public interface ISalaryComponentRepository : IRepository<SalaryComponent, SalaryComponentId>
{
    Task<SalaryComponent?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default);
    Task<SalaryComponent?> GetByIdForUpdateAsync(SalaryComponentId id, CancellationToken cancellationToken = default);
    Task<bool> IsComponentUsedInActiveStructuresAsync(SalaryComponentId componentId, CancellationToken cancellationToken = default);
}

public interface ISalaryStructureRepository : IRepository<SalaryStructure, SalaryStructureId>
{
    Task<SalaryStructure?> GetByCodeAsync(CompanyId companyId, string code, CancellationToken cancellationToken = default);
    Task<SalaryStructure?> GetByIdWithComponentsAsync(SalaryStructureId id, CancellationToken cancellationToken = default);
    Task<SalaryStructure?> GetByIdForUpdateAsync(SalaryStructureId id, CancellationToken cancellationToken = default);
    Task<bool> HasAssignedEmployeesAsync(SalaryStructureId structureId, CancellationToken cancellationToken = default);
}
