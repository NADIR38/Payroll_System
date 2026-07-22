using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Approval;

namespace PayrollMS.Domain.Interfaces.Repositories;

// ── ApprovalWorkflowTemplate ────────────────────────────────────────────────

public interface IApprovalWorkflowTemplateRepository
    : IRepository<ApprovalWorkflowTemplate, ApprovalWorkflowTemplateId>
{
    /// <summary>
    /// Returns the default template for a company.
    /// Returns null if no default has been set yet — caller must handle this.
    /// </summary>
    Task<ApprovalWorkflowTemplate?> GetDefaultAsync(
        CompanyId companyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a template with all its Steps loaded.
    /// </summary>
    Task<ApprovalWorkflowTemplate?> GetByIdWithStepsAsync(
        ApprovalWorkflowTemplateId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active templates for a company.
    /// </summary>
    Task<IReadOnlyList<ApprovalWorkflowTemplate>> GetAllByCompanyAsync(
        CompanyId companyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the IsDefault flag on all templates for a company.
    /// Called before setting a new default to maintain the "one default per company" rule.
    /// </summary>
    Task ClearDefaultsAsync(CompanyId companyId, CancellationToken cancellationToken = default);
}

// ── PayrollApprovalRecord ───────────────────────────────────────────────────

public interface IPayrollApprovalRecordRepository
    : IRepository<PayrollApprovalRecord, PayrollApprovalRecordId>
{
    /// <summary>
    /// Returns all approval records for a payroll run, ordered by ActionAt ASC.
    /// Used for the approval history view — PRD §16.4
    /// </summary>
    Task<IReadOnlyList<PayrollApprovalRecord>> GetByRunIdAsync(
        PayrollRunId runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the completed step orders for a run.
    /// Used by the approval handler to determine the next pending step.
    /// </summary>
    Task<IReadOnlyList<int>> GetCompletedStepOrdersAsync(
        PayrollRunId runId,
        CancellationToken cancellationToken = default);
}
