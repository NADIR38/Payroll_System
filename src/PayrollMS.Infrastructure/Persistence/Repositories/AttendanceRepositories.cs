using Microsoft.EntityFrameworkCore;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Infrastructure.Persistence.Repositories;

public class AttendanceSummaryRepository : Repository<AttendanceSummary, AttendanceSummaryId>, IAttendanceSummaryRepository
{
    public AttendanceSummaryRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<AttendanceSummary?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(a => a.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<AttendanceSummary?> GetByEmployeeAndPeriodAsync(CompanyId companyId, string externalEmployeeId, int periodYear, int periodMonth, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(a =>
            a.CompanyId == companyId &&
            a.ExternalEmployeeId == externalEmployeeId &&
            a.PeriodYear == periodYear &&
            a.PeriodMonth == periodMonth, cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceSummary>> GetByCompanyAndPeriodAsync(CompanyId companyId, int periodYear, int periodMonth, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(a =>
            a.CompanyId == companyId &&
            a.PeriodYear == periodYear &&
            a.PeriodMonth == periodMonth).ToListAsync(cancellationToken);
    }
}

public class LeaveSummaryRepository : Repository<LeaveSummary, LeaveSummaryId>, ILeaveSummaryRepository
{
    public LeaveSummaryRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<LeaveSummary?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(l => l.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<LeaveSummary?> GetByEmployeeAndPeriodAsync(CompanyId companyId, string externalEmployeeId, int periodYear, int periodMonth, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(l =>
            l.CompanyId == companyId &&
            l.ExternalEmployeeId == externalEmployeeId &&
            l.PeriodYear == periodYear &&
            l.PeriodMonth == periodMonth, cancellationToken);
    }

    public async Task<IReadOnlyList<LeaveSummary>> GetByCompanyAndPeriodAsync(CompanyId companyId, int periodYear, int periodMonth, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(l =>
            l.CompanyId == companyId &&
            l.PeriodYear == periodYear &&
            l.PeriodMonth == periodMonth).ToListAsync(cancellationToken);
    }
}
