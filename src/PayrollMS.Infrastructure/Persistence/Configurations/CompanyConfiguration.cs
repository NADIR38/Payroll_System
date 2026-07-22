using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common.ValueObjects;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Common;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("payroll_companies");

        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new CompanyId(value));

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(500);

        builder.Property(c => c.ContactEmail)
            .HasConversion(
                e => e != null ? e.Value : null,
                s => s != null ? EmailAddress.Create(s) : null)
            .HasMaxLength(255);

        builder.Property(c => c.ContactPhone)
            .HasConversion(
                p => p != null ? p.Value : null,
                s => s != null ? PhoneNumber.Create(s) : null)
            .HasMaxLength(50);

        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(p => p.Street).HasColumnName("AddressStreet").HasMaxLength(250);
            a.Property(p => p.City).HasColumnName("AddressCity").HasMaxLength(100);
            a.Property(p => p.State).HasColumnName("AddressState").HasMaxLength(100);
            a.Property(p => p.Country).HasColumnName("AddressCountry").HasMaxLength(100);
            a.Property(p => p.ZipCode).HasColumnName("AddressZipCode").HasMaxLength(20);
        });

        builder.Property(c => c.Version)
            .IsRowVersion();
    }
}
