using Xunit;
using FluentAssertions;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.UnitTests;

public class EmployeeDomainTests
{
    [Fact]
    public void Create_EmployeePayrollProfile_WithValidParameters_ShouldSucceed_AndAddHistory_AndRaiseCreatedEvent()
    {
        // Arrange
        var companyId = CompanyId.New();
        var branchId = BranchId.New();
        var departmentId = DepartmentId.New();
        var designationId = DesignationId.New();
        var structureId = SalaryStructureId.New();
        var joiningDate = new DateOnly(2025, 1, 15);

        // Act
        var profile = EmployeePayrollProfile.Create(
            companyId,
            "EXT-1001",
            "EMP-1001",
            "Ahmed Raza",
            branchId,
            departmentId,
            designationId,
            null,
            structureId,
            75000.00m,
            joiningDate,
            attendanceDeductionOptIn: true,
            syncedBy: "ERP_SYNC");

        // Assert
        profile.Should().NotBeNull();
        profile.Id.Should().NotBe(EmployeePayrollProfileId.Empty);
        profile.CompanyId.Should().Be(companyId);
        profile.ExternalEmployeeId.Should().Be("EXT-1001");
        profile.EmployeeCode.Should().Be("EMP-1001");
        profile.FullName.Should().Be("Ahmed Raza");
        profile.BaseSalary.Should().Be(75000.00m);
        profile.Status.Should().Be(EmployeeStatus.Active);
        profile.AttendanceDeductionOptIn.Should().BeTrue();

        // History verification
        profile.History.Should().HaveCount(1);
        var history = profile.History.First();
        history.ExternalEmployeeId.Should().Be("EXT-1001");
        history.BaseSalary.Should().Be(75000.00m);
        history.EffectiveTo.Should().BeNull();

        // Event verification
        profile.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EmployeeProfileCreatedEvent>();
    }

    [Theory]
    [InlineData("", "EMP-1001", "Ahmed Raza", 50000, "ExternalIdRequired")]
    [InlineData("EXT-1", "", "Ahmed Raza", 50000, "CodeRequired")]
    [InlineData("EXT-1", "EMP-1", "", 50000, "NameRequired")]
    [InlineData("EXT-1", "EMP-1", "Ahmed", -500, "InvalidBaseSalary")]
    public void Create_EmployeePayrollProfile_WithInvalidParameters_ShouldThrowException(
        string extId, string code, string name, decimal salary, string expectedErrorCode)
    {
        // Arrange
        var companyId = CompanyId.New();

        // Act
        Action act = () => EmployeePayrollProfile.Create(
            companyId, extId, code, name,
            BranchId.New(), DepartmentId.New(), DesignationId.New(), null,
            SalaryStructureId.New(), salary, new DateOnly(2025, 1, 1));

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be(expectedErrorCode);
    }

    [Fact]
    public void Update_EmployeePayrollProfile_ShouldClosePreviousHistory_AndOpenNewHistory()
    {
        // Arrange
        var profile = EmployeePayrollProfile.Create(
            CompanyId.New(), "EXT-1001", "EMP-1001", "Ahmed Raza",
            BranchId.New(), DepartmentId.New(), DesignationId.New(), null,
            SalaryStructureId.New(), 75000.00m, new DateOnly(2025, 1, 1));

        profile.ClearDomainEvents();

        var newStructureId = SalaryStructureId.New();

        // Act
        profile.Update(
            "Ahmed Raza Junior",
            profile.BranchId,
            profile.DepartmentId,
            profile.DesignationId,
            null,
            newStructureId,
            85000.00m,
            profile.JoiningDate,
            null,
            attendanceDeductionOptIn: false,
            EmployeeStatus.Active,
            changedBy: "HR_ADMIN",
            changeReason: "Annual increment and structure change");

        // Assert
        profile.FullName.Should().Be("Ahmed Raza Junior");
        profile.BaseSalary.Should().Be(85000.00m);
        profile.AttendanceDeductionOptIn.Should().BeFalse();

        // History count should now be 2
        profile.History.Should().HaveCount(2);

        var firstHistory = profile.History.First();
        firstHistory.EffectiveTo.Should().NotBeNull(); // Old row closed

        var latestHistory = profile.History.Last();
        latestHistory.BaseSalary.Should().Be(85000.00m);
        latestHistory.EffectiveTo.Should().BeNull(); // New row open
        latestHistory.ChangeReason.Should().Be("Annual increment and structure change");

        profile.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EmployeeProfileUpdatedEvent>();
    }

    [Fact]
    public void Terminate_EmployeePayrollProfile_ShouldSetStatus_AndLeavingDate()
    {
        // Arrange
        var profile = EmployeePayrollProfile.Create(
            CompanyId.New(), "EXT-1001", "EMP-1001", "Ahmed Raza",
            BranchId.New(), DepartmentId.New(), DesignationId.New(), null,
            SalaryStructureId.New(), 75000.00m, new DateOnly(2025, 1, 1));

        profile.ClearDomainEvents();

        // Act
        profile.Terminate("HR_MANAGER");

        // Assert
        profile.Status.Should().Be(EmployeeStatus.Terminated);
        profile.LeavingDate.Should().NotBeNull();
        profile.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EmployeeProfileStatusChangedEvent>()
            .Which.NewStatus.Should().Be(EmployeeStatus.Terminated);
    }

    [Fact]
    public void AddBankAccount_WithPrimaryTrue_ShouldClearPreviousPrimary()
    {
        // Arrange
        var profile = EmployeePayrollProfile.Create(
            CompanyId.New(), "EXT-1001", "EMP-1001", "Ahmed Raza",
            BranchId.New(), DepartmentId.New(), DesignationId.New(), null,
            SalaryStructureId.New(), 75000.00m, new DateOnly(2025, 1, 1));

        // Act 1: Add first account as primary
        var acc1 = profile.AddBankAccount("HBL", "Ahmed Raza", "123456", "PK36HABB0000000123456701", "0001", isPrimary: true);

        // Act 2: Add second account as primary
        var acc2 = profile.AddBankAccount("UBL", "Ahmed Raza", "654321", "PK24UNIL0000000012345602", "0002", isPrimary: true);

        // Assert
        acc1.IsPrimary.Should().BeFalse();
        acc2.IsPrimary.Should().BeTrue();
        profile.BankAccounts.Where(b => b.IsPrimary).Should().ContainSingle();
    }

    [Fact]
    public void Deactivate_PrimaryBankAccount_ShouldThrowException()
    {
        // Arrange
        var profile = EmployeePayrollProfile.Create(
            CompanyId.New(), "EXT-1001", "EMP-1001", "Ahmed Raza",
            BranchId.New(), DepartmentId.New(), DesignationId.New(), null,
            SalaryStructureId.New(), 75000.00m, new DateOnly(2025, 1, 1));

        var acc = profile.AddBankAccount("HBL", "Ahmed Raza", "123456", "PK36HABB0000000123456701", null, isPrimary: true);

        // Act
        Action act = () => acc.Deactivate();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be("CannotDeactivatePrimary");
    }
}
