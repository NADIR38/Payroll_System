using Microsoft.EntityFrameworkCore;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Infrastructure.Persistence.Repositories;

public class PayrollRunRepository : Repository<PayrollRun, PayrollRunId>, IPayrollRunRepository
{
    public PayrollRunRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<PayrollRun?> GetByIdWithEntriesAsync(PayrollRunId id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Entries)
                .ThenInclude(e => e.Components)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsForPeriodAsync(CompanyId companyId, int periodYear, int periodMonth, PayrollRunType runType, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(r =>
            r.CompanyId == companyId &&
            r.PeriodYear == periodYear &&
            r.PeriodMonth == periodMonth &&
            r.RunType == runType &&
            r.Status != PayrollRunStatus.Cancelled &&
            r.Status != PayrollRunStatus.Failed, cancellationToken);
    }

    public async Task<(IReadOnlyList<PayrollRun> Items, int TotalCount)> GetPagedAsync(
        CompanyId companyId,
        int page,
        int pageSize,
        PayrollRunStatus? statusFilter = null,
        int? periodYear = null,
        int? periodMonth = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(r => r.CompanyId == companyId);

        if (statusFilter.HasValue)
            query = query.Where(r => r.Status == statusFilter.Value);

        if (periodYear.HasValue)
            query = query.Where(r => r.PeriodYear == periodYear.Value);

        if (periodMonth.HasValue)
            query = query.Where(r => r.PeriodMonth == periodMonth.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<PayrollRunStatus?> GetStatusAsync(PayrollRunId id, CancellationToken cancellationToken = default)
    {
        var run = await DbSet
            .Select(r => new { r.Id, r.Status })
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return run?.Status;
    }
}

public class PayrollEntryRepository : Repository<PayrollEntry, PayrollEntryId>, IPayrollEntryRepository
{
    public PayrollEntryRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<PayrollEntry?> GetByIdWithComponentsAsync(PayrollEntryId id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Components)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PayrollEntry>> GetByRunIdAsync(PayrollRunId runId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Components)
            .Where(e => e.PayrollRunId == runId)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<PayrollEntry> Items, int TotalCount)> GetPagedByRunIdAsync(
        PayrollRunId runId,
        int page,
        int pageSize,
        PayrollEntryStatus? statusFilter = null,
        string? employeeSearch = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(e => e.Components)
            .Where(e => e.PayrollRunId == runId);

        if (statusFilter.HasValue)
            query = query.Where(e => e.Status == statusFilter.Value);

        if (!string.IsNullOrWhiteSpace(employeeSearch))
        {
            var search = employeeSearch.Trim().ToLower();
            query = query.Where(e => e.EmployeeName.ToLower().Contains(search) || e.EmployeeCode.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(e => e.EmployeeName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<PayrollEntry?> GetByRunAndEmployeeAsync(PayrollRunId runId, string externalEmployeeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Components)
            .FirstOrDefaultAsync(e => e.PayrollRunId == runId && e.ExternalEmployeeId == externalEmployeeId, cancellationToken);
    }
}
