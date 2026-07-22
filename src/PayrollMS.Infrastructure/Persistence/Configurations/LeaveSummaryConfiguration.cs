using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class LeaveSummaryConfiguration : IEntityTypeConfiguration<LeaveSummary>
{
    public void Configure(EntityTypeBuilder<LeaveSummary> builder)
    {
        builder.ToTable("payroll_leave_summaries");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, value => new LeaveSummaryId(value));

        builder.Property(l => l.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(l => l.ExternalEmployeeId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.PaidLeaveDays).HasColumnType("numeric(5,2)");
        builder.Property(l => l.UnpaidLeaveDays).HasColumnType("numeric(5,2)");
        builder.Property(l => l.MedicalLeaveDays).HasColumnType("numeric(5,2)");
        builder.Property(l => l.CasualLeaveDays).HasColumnType("numeric(5,2)");
        builder.Property(l => l.HalfDays).HasColumnType("numeric(5,2)");

        builder.Property(l => l.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(l => l.IdempotencyKey)
            .IsUnique();

        builder.HasIndex(l => new { l.CompanyId, l.ExternalEmployeeId, l.PeriodYear, l.PeriodMonth })
            .IsUnique();

        builder.Property(l => l.Version)
            .IsRowVersion();
    }
}
