using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class AttendanceSummaryConfiguration : IEntityTypeConfiguration<AttendanceSummary>
{
    public void Configure(EntityTypeBuilder<AttendanceSummary> builder)
    {
        builder.ToTable("payroll_attendance_summaries");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new AttendanceSummaryId(value));

        builder.Property(a => a.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(a => a.ExternalEmployeeId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.OvertimeHours)
            .HasColumnType("numeric(12,2)");

        builder.Property(a => a.SourceSystem)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(a => a.IdempotencyKey)
            .IsUnique();

        builder.HasIndex(a => new { a.CompanyId, a.ExternalEmployeeId, a.PeriodYear, a.PeriodMonth })
            .IsUnique();

        builder.Property(a => a.Version)
            .IsRowVersion();
    }
}
