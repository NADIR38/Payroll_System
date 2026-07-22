using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Interfaces.Services;

namespace PayrollMS.Infrastructure.Workers;

public class PayrollGenerationWorker
{
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IPayrollEntryRepository _payrollEntryRepo;
    private readonly IEmployeePayrollProfileRepository _employeeProfileRepo;
    private readonly ISalaryStructureRepository _structureRepo;
    private readonly IAttendanceSummaryRepository _attendanceRepo;
    private readonly ILeaveSummaryRepository _leaveRepo;
    private readonly IPayrollCalendarRepository _calendarRepo;
    private readonly IPayrollCalculator _calculator;
    private readonly IUnitOfWork _unitOfWork;

    public PayrollGenerationWorker(
        IPayrollRunRepository payrollRunRepo,
        IPayrollEntryRepository payrollEntryRepo,
        IEmployeePayrollProfileRepository employeeProfileRepo,
        ISalaryStructureRepository structureRepo,
        IAttendanceSummaryRepository attendanceRepo,
        ILeaveSummaryRepository leaveRepo,
        IPayrollCalendarRepository calendarRepo,
        IPayrollCalculator calculator,
        IUnitOfWork unitOfWork)
    {
        _payrollRunRepo = payrollRunRepo;
        _payrollEntryRepo = payrollEntryRepo;
        _employeeProfileRepo = employeeProfileRepo;
        _structureRepo = structureRepo;
        _attendanceRepo = attendanceRepo;
        _leaveRepo = leaveRepo;
        _calendarRepo = calendarRepo;
        _calculator = calculator;
        _unitOfWork = unitOfWork;
    }

    public async Task Execute(Guid payrollRunIdGuid)
    {
        var payrollRunId = new PayrollRunId(payrollRunIdGuid);

        var run = await _payrollRunRepo.GetByIdAsync(payrollRunId);
        if (run == null) return;

        try
        {
            run.MarkGenerating();
            _payrollRunRepo.Update(run);
            await _unitOfWork.SaveChangesAsync();

            // Load all profiles for company
            var profiles = await _employeeProfileRepo.FindAsync(e => e.CompanyId == run.CompanyId && e.Status == Domain.Enums.EmployeeStatus.Active);

            var calendar = await _calendarRepo.GetByMonthYearAsync(run.CompanyId, run.PeriodMonth, run.PeriodYear);
            int calendarWorkingDays = calendar?.WorkingDays ?? 30;

            var entries = new List<PayrollEntry>();

            foreach (var profile in profiles)
            {
                var structure = await _structureRepo.GetByIdWithComponentsAsync(profile.SalaryStructureId);
                var attendance = await _attendanceRepo.GetByEmployeeAndPeriodAsync(run.CompanyId, profile.ExternalEmployeeId, run.PeriodYear, run.PeriodMonth);
                var leave = await _leaveRepo.GetByEmployeeAndPeriodAsync(run.CompanyId, profile.ExternalEmployeeId, run.PeriodYear, run.PeriodMonth);

                var primaryBank = profile.BankAccounts.FirstOrDefault(b => b.IsPrimary);

                var input = new PayrollCalculationInput
                {
                    PayrollRunId = run.Id,
                    CompanyId = run.CompanyId,
                    ExternalEmployeeId = profile.ExternalEmployeeId,
                    EmployeeCode = profile.EmployeeCode,
                    EmployeeName = profile.FullName,
                    DepartmentName = "Department",
                    DepartmentCode = "DEPT",
                    DesignationName = "Designation",
                    DesignationCode = "DESG",
                    BankName = primaryBank?.BankName,
                    IBAN = primaryBank?.IBAN,
                    BaseSalary = profile.BaseSalary,
                    AttendanceDeductionOptIn = profile.AttendanceDeductionOptIn,
                    PeriodYear = run.PeriodYear,
                    PeriodMonth = run.PeriodMonth,
                    Structure = structure,
                    Attendance = attendance,
                    Leave = leave,
                    CalendarWorkingDays = calendarWorkingDays,
                    LoanInstallmentAmount = 0,
                    AdvanceRecoveryAmount = 0
                };

                var entry = await _calculator.CalculateAsync(input);
                entries.Add(entry);
            }

            await _payrollEntryRepo.AddRangeAsync(entries);

            decimal totalGross = entries.Sum(e => e.GrossSalary);
            decimal totalDeductions = entries.Sum(e => e.TotalDeductions);
            decimal totalNet = entries.Sum(e => e.NetSalary);

            run.MarkGenerated(entries.Count, totalGross, totalDeductions, totalNet, "PayrollWorker");
            _payrollRunRepo.Update(run);

            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            run.MarkFailed(ex.Message);
            _payrollRunRepo.Update(run);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
