using Xunit;
using FluentAssertions;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Common.ValueObjects;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;

namespace PayrollMS.Domain.UnitTests;

public class TenantDomainTests
{
    [Theory]
    [InlineData("test@domain.com")]
    [InlineData("hr@payroll.net.pk")]
    [InlineData("admin.user@sub.domain.co")]
    public void Create_EmailAddress_With_Valid_Format_Should_Succeed(string email)
    {
        // Act
        var emailObj = EmailAddress.Create(email);

        // Assert
        emailObj.Should().NotBeNull();
        emailObj.Value.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("test@domain")]
    [InlineData("@domain.com")]
    public void Create_EmailAddress_With_Invalid_Format_Should_Throw_BusinessRuleViolationException(string email)
    {
        // Act
        Action act = () => EmailAddress.Create(email);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().BeOneOf("EmailRequired", "InvalidEmailFormat");
    }

    [Theory]
    [InlineData("+923001234567")]
    [InlineData("03001234567")]
    [InlineData("02131234567")]
    public void Create_PhoneNumber_With_Valid_Format_Should_Succeed(string phone)
    {
        // Act
        var phoneObj = PhoneNumber.Create(phone);

        // Assert
        phoneObj.Should().NotBeNull();
        phoneObj.Value.Should().Be(phone.Replace(" ", "").Replace("-", ""));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    public void Create_PhoneNumber_With_Invalid_Format_Should_Throw_BusinessRuleViolationException(string phone)
    {
        // Act
        Action act = () => PhoneNumber.Create(phone);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().BeOneOf("PhoneNumberRequired", "InvalidPhoneFormat");
    }

    [Fact]
    public void Create_Address_With_Valid_Parameters_Should_Succeed()
    {
        // Act
        var address = Address.Create("Street 1", "Islamabad", "ICT", "Pakistan", "44000");

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be("Street 1");
        address.City.Should().Be("Islamabad");
        address.ToString().Should().Be("Street 1, Islamabad, ICT, Pakistan - 44000");
    }

    [Fact]
    public void Create_Address_With_Invalid_Parameters_Should_Throw_BusinessRuleViolationException()
    {
        // Act
        Action act = () => Address.Create("", "Islamabad", "ICT", "Pakistan", "44000");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be("StreetRequired");
    }

    [Fact]
    public void Create_Company_Should_Succeed_And_Raise_CompanyCreatedEvent()
    {
        // Arrange
        var name = "Acme Corp";
        var code = "ACM";
        var email = EmailAddress.Create("contact@acme.com");
        var phone = PhoneNumber.Create("03001234567");
        var address = Address.Create("123 Street", "Karachi", "Sindh", "Pakistan", "74000");

        // Act
        var company = Company.Create(name, code, email, phone, address);

        // Assert
        company.Should().NotBeNull();
        company.Id.Should().NotBe(CompanyId.Empty);
        company.Name.Should().Be(name);
        company.Code.Should().Be(code);
        company.ContactEmail.Should().Be(email);
        company.ContactPhone.Should().Be(phone);
        company.Address.Should().Be(address);
        company.IsActive.Should().BeTrue();

        company.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompanyCreatedEvent>();
    }

    [Fact]
    public void Update_Company_Details_Should_Succeed_And_Raise_CompanyUpdatedEvent()
    {
        // Arrange
        var company = Company.Create("Acme Corp", "ACM");
        company.ClearDomainEvents();

        var newEmail = EmailAddress.Create("new@acme.com");

        // Act
        company.UpdateDetails("Acme Corp Updated", newEmail, null, null, null);

        // Assert
        company.Name.Should().Be("Acme Corp Updated");
        company.ContactEmail.Should().Be(newEmail);
        company.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompanyUpdatedEvent>();
    }

    [Fact]
    public void Activate_Company_Should_Raise_CompanyActivatedEvent()
    {
        // Arrange
        var company = Company.Create("Acme Corp", "ACM");
        company.Deactivate();
        company.ClearDomainEvents();

        // Act
        company.Activate();

        // Assert
        company.IsActive.Should().BeTrue();
        company.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompanyActivatedEvent>();
    }

    [Fact]
    public void Deactivate_Company_Should_Raise_CompanyDeactivatedEvent()
    {
        // Arrange
        var company = Company.Create("Acme Corp", "ACM");
        company.ClearDomainEvents();

        // Act
        company.Deactivate();

        // Assert
        company.IsActive.Should().BeFalse();
        company.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompanyDeactivatedEvent>();
    }

    [Fact]
    public void Create_Company_With_Empty_Name_Should_Throw_Exception()
    {
        // Act
        Action act = () => Company.Create("", "ACM");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be("NameRequired");
    }

    [Fact]
    public void PayrollCalendar_Transitions_Should_Be_Valid()
    {
        // Arrange
        var companyId = CompanyId.New();
        var fyId = FinancialYearId.New();
        var calendar = PayrollCalendar.Create(
            companyId,
            fyId,
            7,
            2026,
            new DateOnly(2026, 7, 25),
            new DateOnly(2026, 7, 30),
            22);

        calendar.Status.Should().Be(PayrollCalendarStatus.Open);

        // Act & Assert 1: Open -> Frozen (Valid)
        calendar.Freeze();
        calendar.Status.Should().Be(PayrollCalendarStatus.Frozen);

        // Act & Assert 2: Frozen -> Closed (Valid)
        calendar.Close();
        calendar.Status.Should().Be(PayrollCalendarStatus.Closed);
    }

    [Fact]
    public void Close_PayrollCalendar_When_Open_Should_Throw_BusinessRuleViolationException()
    {
        // Arrange
        var companyId = CompanyId.New();
        var fyId = FinancialYearId.New();
        var calendar = PayrollCalendar.Create(
            companyId,
            fyId,
            7,
            2026,
            new DateOnly(2026, 7, 25),
            new DateOnly(2026, 7, 30),
            22);

        // Act
        Action act = () => calendar.Close();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be("InvalidState");
    }

    [Fact]
    public void Reopen_PayrollCalendar_When_Closed_Should_Throw_BusinessRuleViolationException()
    {
        // Arrange
        var companyId = CompanyId.New();
        var fyId = FinancialYearId.New();
        var calendar = PayrollCalendar.Create(
            companyId,
            fyId,
            7,
            2026,
            new DateOnly(2026, 7, 25),
            new DateOnly(2026, 7, 30),
            22);

        calendar.Freeze();
        calendar.Close();

        // Act
        Action act = () => calendar.Reopen();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be("InvalidState");
    }
}
