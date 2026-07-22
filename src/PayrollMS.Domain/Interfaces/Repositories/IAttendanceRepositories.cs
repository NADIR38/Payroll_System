using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;

namespace PayrollMS.Domain.Interfaces.Repositories;

// ── Attendance ──────────────────────────────────────────────────────────────

public interface IAttendanceSummaryRepository : IRepository<AttendanceSummary, AttendanceSummaryId>
{
    /// <summary>
    /// Looks up a summary by idempotency key. Returns null if not found.
    /// Used to implement the duplicate-sync-accepted rule — PRD §13.4
    /// </summary>
    Task<AttendanceSummary?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the attendance summary for a specific employee in a specific period.
    /// Used by the payroll calculator to build FormulaContext — PRD §10.3
    /// </summary>
    Task<AttendanceSummary?> GetByEmployeeAndPeriodAsync(
        CompanyId companyId,
        string externalEmployeeId,
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all attendance summaries for a company in a given period.
    /// Used by the PayrollGenerationWorker to bulk-load attendance — PRD §15.5
    /// </summary>
    Task<IReadOnlyList<AttendanceSummary>> GetByCompanyAndPeriodAsync(
        CompanyId companyId,
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default);
}

// ── Leave ───────────────────────────────────────────────────────────────────

public interface ILeaveSummaryRepository : IRepository<LeaveSummary, LeaveSummaryId>
{
    /// <summary>
    /// Looks up a leave summary by idempotency key. Returns null if not found.
    /// </summary>
    Task<LeaveSummary?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the leave summary for a specific employee in a specific period.
    /// </summary>
    Task<LeaveSummary?> GetByEmployeeAndPeriodAsync(
        CompanyId companyId,
        string externalEmployeeId,
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all leave summaries for a company in a given period.
    /// </summary>
    Task<IReadOnlyList<LeaveSummary>> GetByCompanyAndPeriodAsync(
        CompanyId companyId,
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default);
}
