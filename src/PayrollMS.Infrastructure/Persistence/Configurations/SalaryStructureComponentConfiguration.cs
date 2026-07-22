using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class SalaryStructureComponentConfiguration : IEntityTypeConfiguration<SalaryStructureComponent>
{
    public void Configure(EntityTypeBuilder<SalaryStructureComponent> builder)
    {
        builder.ToTable("payroll_salary_structure_components");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new SalaryStructureComponentId(value));

        builder.Property(c => c.SalaryStructureId)
            .HasConversion(id => id.Value, value => new SalaryStructureId(value))
            .IsRequired();

        builder.Property(c => c.SalaryComponentId)
            .HasConversion(id => id.Value, value => new SalaryComponentId(value))
            .IsRequired();

        builder.Property(c => c.FormulaExpression)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.FixedAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.Sequence)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(c => c.Component)
            .WithMany()
            .HasForeignKey(c => c.SalaryComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AllowanceRule)
            .WithOne()
            .HasForeignKey<AllowanceRule>(a => a.SalaryStructureComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.DeductionRule)
            .WithOne()
            .HasForeignKey<DeductionRule>(d => d.SalaryStructureComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.SalaryStructureId, c.Sequence })
            .IsUnique();
    }
}
