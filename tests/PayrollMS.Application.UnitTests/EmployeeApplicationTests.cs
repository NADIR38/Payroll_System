using FluentAssertions;
using NSubstitute;
using PayrollMS.Application.Features.Employee.Commands.SyncEmployee;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using Xunit;

namespace PayrollMS.Application.UnitTests;

public class EmployeeApplicationTests
{
    private readonly IEmployeePayrollProfileRepository _employeeRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeApplicationTests()
    {
        _employeeRepository = Substitute.For<IEmployeePayrollProfileRepository>();
        _dbContext = Substitute.For<IAppDbContext>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public void SyncEmployeeCommandValidator_WithValidPayload_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = CreateValidSyncCommand(Guid.NewGuid());
        var validator = new SyncEmployeeCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SyncEmployeeCommandValidator_WithNegativeSalary_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidSyncCommand(Guid.NewGuid()) with { BaseSalary = -1000m };
        var validator = new SyncEmployeeCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "BaseSalary");
    }

    private static SyncEmployeeCommand CreateValidSyncCommand(Guid companyId)
    {
        return new SyncEmployeeCommand(
            CompanyId: companyId,
            ExternalEmployeeId: "EMP-1001",
            EmployeeCode: "EMP-1001",
            FullName: "Ahmed Raza",
            BranchId: Guid.NewGuid(),
            DepartmentId: Guid.NewGuid(),
            DesignationId: Guid.NewGuid(),
            CostCenterId: null,
            SalaryStructureId: Guid.NewGuid(),
            BaseSalary: 75000.00m,
            JoiningDate: new DateOnly(2025, 1, 15),
            AttendanceDeductionOptIn: true,
            BankName: "HBL",
            AccountTitle: "Ahmed Raza",
            AccountNumber: "12345678",
            IBAN: "PK36HABB0000000123456701");
    }
}
