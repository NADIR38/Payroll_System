using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("payroll_branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(id => id.Value, value => new BranchId(value));

        builder.Property(b => b.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(b => new { b.CompanyId, b.Code })
            .IsUnique();

        builder.Property(b => b.Address)
            .HasMaxLength(250);

        builder.HasOne(b => b.Company)
            .WithMany(c => c.Branches)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(b => b.Version)
            .IsRowVersion();
    }
}
