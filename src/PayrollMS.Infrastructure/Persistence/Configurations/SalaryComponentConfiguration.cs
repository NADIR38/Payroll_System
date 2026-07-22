using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.ToTable("payroll_salary_components");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new SalaryComponentId(value));

        builder.Property(c => c.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.CalculationMethod)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.DefaultValue)
            .HasPrecision(18, 2);

        builder.Property(c => c.IsTaxable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.IsRecurring)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.IsOptional)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        // Unique index per company on Code
        builder.HasIndex(c => new { c.CompanyId, c.Code })
            .IsUnique();

        builder.Property(c => c.Version)
            .IsRowVersion();
    }
}
