using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("payroll_runs");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new PayrollRunId(value));

        builder.Property(r => r.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(r => r.FinancialYearId)
            .HasConversion(id => id.Value, value => new FinancialYearId(value))
            .IsRequired();

        builder.Property(r => r.FilterBranchId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new BranchId(value.Value) : null);

        builder.Property(r => r.FilterDepartmentId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new DepartmentId(value.Value) : null);

        builder.Property(r => r.ParentRunId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new PayrollRunId(value.Value) : null);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.RunType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.TotalGross).HasColumnType("numeric(14,2)");
        builder.Property(r => r.TotalDeductions).HasColumnType("numeric(14,2)");
        builder.Property(r => r.TotalNet).HasColumnType("numeric(14,2)");

        builder.Property(r => r.GeneratedBy).HasMaxLength(150);
        builder.Property(r => r.Remarks).HasMaxLength(500);

        builder.HasIndex(r => new { r.CompanyId, r.PeriodYear, r.PeriodMonth, r.RunType });

        builder.HasMany(r => r.Entries)
            .WithOne()
            .HasForeignKey(e => e.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.Version)
            .IsRowVersion();
    }
}
