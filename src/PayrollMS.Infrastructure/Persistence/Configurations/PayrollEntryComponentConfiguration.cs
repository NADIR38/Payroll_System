using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class PayrollEntryComponentConfiguration : IEntityTypeConfiguration<PayrollEntryComponent>
{
    public void Configure(EntityTypeBuilder<PayrollEntryComponent> builder)
    {
        builder.ToTable("payroll_entry_components");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new PayrollEntryComponentId(value));

        builder.Property(c => c.PayrollEntryId)
            .HasConversion(id => id.Value, value => new PayrollEntryId(value))
            .IsRequired();

        builder.Property(c => c.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(c => c.SalaryComponentId)
            .HasConversion(id => id.Value, value => new SalaryComponentId(value))
            .IsRequired();

        builder.Property(c => c.ComponentCode).IsRequired().HasMaxLength(50);
        builder.Property(c => c.ComponentName).IsRequired().HasMaxLength(150);

        builder.Property(c => c.ComponentType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.FormulaUsed).HasMaxLength(1000);
        builder.Property(c => c.CalculatedAmount).HasColumnType("numeric(12,2)");

        builder.Property(c => c.Version)
            .IsRowVersion();
    }
}
