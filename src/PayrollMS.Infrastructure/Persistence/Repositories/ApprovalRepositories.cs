using Microsoft.EntityFrameworkCore;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Approval;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Infrastructure.Persistence.Repositories;

public class ApprovalWorkflowTemplateRepository : Repository<ApprovalWorkflowTemplate, ApprovalWorkflowTemplateId>, IApprovalWorkflowTemplateRepository
{
    public ApprovalWorkflowTemplateRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<ApprovalWorkflowTemplate?> GetDefaultAsync(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.CompanyId == companyId && t.IsDefault && t.IsActive, cancellationToken);
    }

    public async Task<ApprovalWorkflowTemplate?> GetByIdWithStepsAsync(ApprovalWorkflowTemplateId id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalWorkflowTemplate>> GetAllByCompanyAsync(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Steps)
            .Where(t => t.CompanyId == companyId && t.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task ClearDefaultsAsync(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        var defaults = await DbSet
            .Where(t => t.CompanyId == companyId && t.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var template in defaults)
        {
            template.UnsetDefault();
        }
    }
}

public class PayrollApprovalRecordRepository : Repository<PayrollApprovalRecord, PayrollApprovalRecordId>, IPayrollApprovalRecordRepository
{
    public PayrollApprovalRecordRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<PayrollApprovalRecord>> GetByRunIdAsync(PayrollRunId runId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.PayrollRunId == runId)
            .OrderBy(r => r.ActionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetCompletedStepOrdersAsync(PayrollRunId runId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.PayrollRunId == runId && r.Action == Domain.Enums.ApprovalAction.Approved)
            .Select(r => r.StepOrder)
            .ToListAsync(cancellationToken);
    }
}
