using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class DeductionRuleConfiguration : IEntityTypeConfiguration<DeductionRule>
{
    public void Configure(EntityTypeBuilder<DeductionRule> builder)
    {
        builder.ToTable("payroll_deduction_rules");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasConversion(id => id.Value, value => new DeductionRuleId(value));

        builder.Property(d => d.SalaryStructureComponentId)
            .HasConversion(id => id.Value, value => new SalaryStructureComponentId(value))
            .IsRequired();

        builder.Property(d => d.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(d => d.DeductionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.IsOptIn)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.GracePeriodMinutes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(d => d.DeductionFormula)
            .HasMaxLength(1000);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
