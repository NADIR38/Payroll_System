using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class EmployeePayrollProfileConfiguration : IEntityTypeConfiguration<EmployeePayrollProfile>
{
    public void Configure(EntityTypeBuilder<EmployeePayrollProfile> builder)
    {
        builder.ToTable("payroll_employee_profiles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new EmployeePayrollProfileId(value));

        builder.Property(e => e.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(e => e.ExternalEmployeeId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.BranchId)
            .HasConversion(id => id.Value, value => new BranchId(value))
            .IsRequired();

        builder.Property(e => e.DepartmentId)
            .HasConversion(id => id.Value, value => new DepartmentId(value))
            .IsRequired();

        builder.Property(e => e.DesignationId)
            .HasConversion(id => id.Value, value => new DesignationId(value))
            .IsRequired();

        builder.Property(e => e.CostCenterId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new CostCenterId(value.Value) : null);

        builder.Property(e => e.SalaryStructureId)
            .HasConversion(id => id.Value, value => new SalaryStructureId(value))
            .IsRequired();

        builder.Property(e => e.BaseSalary)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.AttendanceDeductionOptIn)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.JoiningDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.EffectiveFrom)
            .IsRequired();

        // Unique index on (ExternalEmployeeId + CompanyId) — PRD §7.4
        builder.HasIndex(e => new { e.CompanyId, e.ExternalEmployeeId })
            .IsUnique();

        // Relationships
        builder.HasMany(e => e.History)
            .WithOne()
            .HasForeignKey(h => h.EmployeePayrollProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.BankAccounts)
            .WithOne()
            .HasForeignKey(b => b.EmployeePayrollProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.Version)
            .IsRowVersion();
    }
}
