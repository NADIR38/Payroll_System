using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Domain.Interfaces.Repositories;

// ── PayrollRun ──────────────────────────────────────────────────────────────

public interface IPayrollRunRepository : IRepository<PayrollRun, PayrollRunId>
{
    /// <summary>
    /// Loads a PayrollRun with all its Entries and their Components.
    /// Used by the approval and override handlers.
    /// </summary>
    Task<PayrollRun?> GetByIdWithEntriesAsync(PayrollRunId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an active Regular run already exists for this company+period.
    /// Used to enforce PRD §15.4: "no existing Regular run for the same period".
    /// </summary>
    Task<bool> ExistsForPeriodAsync(
        CompanyId companyId,
        int periodYear,
        int periodMonth,
        PayrollRunType runType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of runs for a company, optionally filtered by status/period.
    /// </summary>
    Task<(IReadOnlyList<PayrollRun> Items, int TotalCount)> GetPagedAsync(
        CompanyId companyId,
        int page,
        int pageSize,
        PayrollRunStatus? statusFilter = null,
        int? periodYear = null,
        int? periodMonth = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns only the Status of a run — lightweight poll for the frontend.
    /// </summary>
    Task<PayrollRunStatus?> GetStatusAsync(PayrollRunId id, CancellationToken cancellationToken = default);
}

// ── PayrollEntry ────────────────────────────────────────────────────────────

public interface IPayrollEntryRepository : IRepository<PayrollEntry, PayrollEntryId>
{
    /// <summary>
    /// Loads a PayrollEntry with all its Components.
    /// </summary>
    Task<PayrollEntry?> GetByIdWithComponentsAsync(PayrollEntryId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entries for a PayrollRun (without components — for list views).
    /// </summary>
    Task<IReadOnlyList<PayrollEntry>> GetByRunIdAsync(PayrollRunId runId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entries for a run, paginated, with optional employee/status filters.
    /// </summary>
    Task<(IReadOnlyList<PayrollEntry> Items, int TotalCount)> GetPagedByRunIdAsync(
        PayrollRunId runId,
        int page,
        int pageSize,
        PayrollEntryStatus? statusFilter = null,
        string? employeeSearch = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the PayrollEntry for a specific employee within a specific run.
    /// Used for dispute and correction run lookups.
    /// </summary>
    Task<PayrollEntry?> GetByRunAndEmployeeAsync(
        PayrollRunId runId,
        string externalEmployeeId,
        CancellationToken cancellationToken = default);
}
