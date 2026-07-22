using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class AllowanceRuleConfiguration : IEntityTypeConfiguration<AllowanceRule>
{
    public void Configure(EntityTypeBuilder<AllowanceRule> builder)
    {
        builder.ToTable("payroll_allowance_rules");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new AllowanceRuleId(value));

        builder.Property(a => a.SalaryStructureComponentId)
            .HasConversion(id => id.Value, value => new SalaryStructureComponentId(value))
            .IsRequired();

        builder.Property(a => a.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(a => a.ApplicationMode)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.ConditionExpression)
            .HasMaxLength(1000);

        builder.Property(a => a.Description)
            .HasMaxLength(500);
    }
}
