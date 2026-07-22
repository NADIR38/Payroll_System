using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class EmployeePayrollProfileHistoryConfiguration : IEntityTypeConfiguration<EmployeePayrollProfileHistory>
{
    public void Configure(EntityTypeBuilder<EmployeePayrollProfileHistory> builder)
    {
        builder.ToTable("payroll_employee_profile_history");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasConversion(id => id.Value, value => new EmployeePayrollProfileHistoryId(value));

        builder.Property(h => h.EmployeePayrollProfileId)
            .HasConversion(id => id.Value, value => new EmployeePayrollProfileId(value))
            .IsRequired();

        builder.Property(h => h.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(h => h.ExternalEmployeeId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.EmployeeCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.SalaryStructureId)
            .HasConversion(id => id.Value, value => new SalaryStructureId(value))
            .IsRequired();

        builder.Property(h => h.BaseSalary)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(h => h.EffectiveFrom)
            .IsRequired();

        builder.Property(h => h.ChangedBy)
            .HasMaxLength(100);

        builder.Property(h => h.ChangeReason)
            .HasMaxLength(500);

        builder.HasIndex(h => new { h.CompanyId, h.EmployeePayrollProfileId, h.EffectiveFrom });
    }
}
