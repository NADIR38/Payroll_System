using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class EmployeeBankAccountConfiguration : IEntityTypeConfiguration<EmployeeBankAccount>
{
    public void Configure(EntityTypeBuilder<EmployeeBankAccount> builder)
    {
        builder.ToTable("payroll_employee_bank_accounts");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(id => id.Value, value => new EmployeeBankAccountId(value));

        builder.Property(b => b.EmployeePayrollProfileId)
            .HasConversion(id => id.Value, value => new EmployeePayrollProfileId(value))
            .IsRequired();

        builder.Property(b => b.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(b => b.BankName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.AccountTitle)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.AccountNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.IBAN)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.BranchCode)
            .HasMaxLength(20);

        builder.Property(b => b.IsPrimary)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(b => new { b.CompanyId, b.EmployeePayrollProfileId });
    }
}
