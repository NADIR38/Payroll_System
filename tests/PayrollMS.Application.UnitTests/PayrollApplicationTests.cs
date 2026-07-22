using Xunit;
using FluentAssertions;
using NSubstitute;
using PayrollMS.Application.Features.Attendance.Commands.SyncAttendance;
using PayrollMS.Application.Features.PayrollRuns.Commands.CreatePayrollRun;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Entities.Employee;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.UnitTests;

public class PayrollApplicationTests
{
    private readonly IAttendanceSummaryRepository _attendanceRepo = Substitute.For<IAttendanceSummaryRepository>();
    private readonly IEmployeePayrollProfileRepository _employeeRepo = Substitute.For<IEmployeePayrollProfileRepository>();
    private readonly IPayrollCalendarRepository _calendarRepo = Substitute.For<IPayrollCalendarRepository>();
    private readonly IPayrollRunRepository _payrollRunRepo = Substitute.For<IPayrollRunRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_SyncAttendance_WhenPeriodIsFrozen_ShouldThrowBusinessRuleViolationException()
    {
        var companyId = Guid.NewGuid();
        var command = new SyncAttendanceCommand(
            companyId, "EMP-1", 2025, 7, 22, 0, 0, 0, 0, 0, 0, 0, "KEY1");

        var calendar = PayrollCalendar.Create(new CompanyId(companyId), FinancialYearId.New(), 7, 2025, new DateOnly(2025, 7, 25), new DateOnly(2025, 7, 30), 22);
        calendar.Freeze();

        _calendarRepo.GetByMonthYearAsync(Arg.Any<CompanyId>(), 7, 2025, Arg.Any<CancellationToken>())
            .Returns(calendar);

        var handler = new SyncAttendanceCommandHandler(_attendanceRepo, _employeeRepo, _calendarRepo, _unitOfWork);

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task Handle_CreatePayrollRun_WhenRegularRunAlreadyExists_ShouldThrowBusinessRuleViolationException()
    {
        var companyId = Guid.NewGuid();
        var command = new CreatePayrollRunCommand(
            companyId, Guid.NewGuid(), 2025, 7, PayrollRunType.Regular);

        _payrollRunRepo.ExistsForPeriodAsync(Arg.Any<CompanyId>(), 2025, 7, PayrollRunType.Regular, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CreatePayrollRunCommandHandler(_payrollRunRepo, _calendarRepo, _unitOfWork);

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }
}
