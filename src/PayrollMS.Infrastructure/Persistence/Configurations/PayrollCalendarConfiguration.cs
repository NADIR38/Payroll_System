using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class PayrollCalendarConfiguration : IEntityTypeConfiguration<PayrollCalendar>
{
    public void Configure(EntityTypeBuilder<PayrollCalendar> builder)
    {
        builder.ToTable("payroll_calendars");

        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new PayrollCalendarId(value));

        builder.Property(p => p.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(p => p.FinancialYearId)
            .HasConversion(id => id.Value, value => new FinancialYearId(value))
            .IsRequired();

        builder.Property(p => p.Month)
            .IsRequired();

        builder.Property(p => p.Year)
            .IsRequired();

        builder.Property(p => p.PayrollFreezeDate)
            .IsRequired();

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.WorkingDays)
            .IsRequired();

        builder.Property(p => p.Holidays)
            .HasColumnType("jsonb");

        builder.Property(p => p.Status)
            .HasConversion(
                v => v.ToString(),
                v => (PayrollCalendarStatus)Enum.Parse(typeof(PayrollCalendarStatus), v))
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(p => new { p.CompanyId, p.FinancialYearId, p.Month, p.Year })
            .IsUnique();

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.FinancialYear)
            .WithMany(f => f.PayrollCalendars)
            .HasForeignKey(p => p.FinancialYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Version)
            .IsRowVersion();
    }
}
