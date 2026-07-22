using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Common;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("payroll_cost_centers");

        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new CostCenterId(value));

        builder.Property(c => c.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => new { c.CompanyId, c.Code })
            .IsUnique();

        builder.HasOne(c => c.Company)
            .WithMany(comp => comp.CostCenters)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.Version)
            .IsRowVersion();
    }
}
