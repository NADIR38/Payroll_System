using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Common;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class FinancialYearConfiguration : IEntityTypeConfiguration<FinancialYear>
{
    public void Configure(EntityTypeBuilder<FinancialYear> builder)
    {
        builder.ToTable("payroll_financial_years");

        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => new FinancialYearId(value));

        builder.Property(f => f.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(f => f.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.StartDate)
            .IsRequired();

        builder.Property(f => f.EndDate)
            .IsRequired();

        builder.Property(f => f.IsCurrent)
            .IsRequired();

        builder.HasIndex(f => new { f.CompanyId, f.Label })
            .IsUnique();

        builder.HasOne(f => f.Company)
            .WithMany(c => c.FinancialYears)
            .HasForeignKey(f => f.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(f => f.Version)
            .IsRowVersion();
    }
}
