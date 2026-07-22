using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.ToTable("payroll_salary_structures");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new SalaryStructureId(value));

        builder.Property(s => s.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.EffectiveFrom)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique index per company on Code
        builder.HasIndex(s => new { s.CompanyId, s.Code })
            .IsUnique();

        // Components relationship
        builder.HasMany(s => s.Components)
            .WithOne()
            .HasForeignKey(c => c.SalaryStructureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Version)
            .IsRowVersion();
    }
}
