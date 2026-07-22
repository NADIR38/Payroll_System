using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Common;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("payroll_departments");

        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id)
            .HasConversion(id => id.Value, value => new DepartmentId(value));

        builder.Property(d => d.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(d => d.BranchId)
            .HasConversion(
                id => id.HasValue ? (Guid?)id.Value.Value : null,
                value => value.HasValue ? new BranchId(value.Value) : null);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(d => new { d.CompanyId, d.Code })
            .IsUnique();

        builder.HasOne(d => d.Company)
            .WithMany(c => c.Departments)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Branch)
            .WithMany()
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(d => d.Version)
            .IsRowVersion();
    }
}
