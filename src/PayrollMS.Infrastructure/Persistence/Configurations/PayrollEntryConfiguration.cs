using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class PayrollEntryConfiguration : IEntityTypeConfiguration<PayrollEntry>
{
    public void Configure(EntityTypeBuilder<PayrollEntry> builder)
    {
        builder.ToTable("payroll_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new PayrollEntryId(value));

        builder.Property(e => e.PayrollRunId)
            .HasConversion(id => id.Value, value => new PayrollRunId(value))
            .IsRequired();

        builder.Property(e => e.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(e => e.ExternalEmployeeId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.EmployeeName).IsRequired().HasMaxLength(150);
        builder.Property(e => e.DepartmentName).HasMaxLength(150);
        builder.Property(e => e.DesignationName).HasMaxLength(150);
        builder.Property(e => e.BankName).HasMaxLength(100);
        builder.Property(e => e.IBAN).HasMaxLength(50);

        builder.Property(e => e.BaseSalary).HasColumnType("numeric(12,2)");
        builder.Property(e => e.GrossSalary).HasColumnType("numeric(12,2)");
        builder.Property(e => e.TotalDeductions).HasColumnType("numeric(12,2)");
        builder.Property(e => e.NetSalary).HasColumnType("numeric(12,2)");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => new { e.CompanyId, e.PayrollRunId, e.ExternalEmployeeId });

        builder.HasMany(e => e.Components)
            .WithOne()
            .HasForeignKey(c => c.PayrollEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.Version)
            .IsRowVersion();
    }
}
