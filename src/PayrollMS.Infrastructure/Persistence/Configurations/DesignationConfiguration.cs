using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Common;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.ToTable("payroll_designations");

        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id)
            .HasConversion(id => id.Value, value => new DesignationId(value));

        builder.Property(d => d.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.Grade)
            .HasMaxLength(50);

        builder.HasIndex(d => new { d.CompanyId, d.Code })
            .IsUnique();

        builder.HasOne(d => d.Company)
            .WithMany(c => c.Designations)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.Version)
            .IsRowVersion();
    }
}
