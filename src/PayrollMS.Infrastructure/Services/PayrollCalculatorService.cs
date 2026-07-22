using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Interfaces.Services;

namespace PayrollMS.Infrastructure.Services;

/// <summary>
/// Implements IPayrollCalculator — orchestrates formula evaluation and rule checks per employee — PRD §10.4
/// </summary>
public sealed class PayrollCalculatorService : IPayrollCalculator
{
    private readonly IFormulaEvaluator _formulaEvaluator;

    public PayrollCalculatorService(IFormulaEvaluator formulaEvaluator)
    {
        _formulaEvaluator = formulaEvaluator;
    }

    public Task<PayrollEntry> CalculateAsync(PayrollCalculationInput input, CancellationToken cancellationToken = default)
    {
        // 1. Guard against missing structure or attendance — PRD §15.6
        if (input.Structure == null)
        {
            var errorEntry = PayrollEntry.CreateWithError(
                input.PayrollRunId, input.CompanyId, input.ExternalEmployeeId,
                input.EmployeeCode, input.EmployeeName, PayrollEntryStatus.StructureMissing);
            return Task.FromResult(errorEntry);
        }

        if (input.Attendance == null)
        {
            var errorEntry = PayrollEntry.CreateWithError(
                input.PayrollRunId, input.CompanyId, input.ExternalEmployeeId,
                input.EmployeeCode, input.EmployeeName, PayrollEntryStatus.AttendanceMissing);
            return Task.FromResult(errorEntry);
        }

        // 2. Build FormulaContext — PRD §10.3
        var context = new FormulaContext
        {
            BaseSalary = input.BaseSalary,
            GrossSalary = 0,
            CalendarWorkingDays = input.CalendarWorkingDays > 0 ? input.CalendarWorkingDays : 30,
            WorkingDays = input.Attendance.WorkingDays,
            AbsentDays = input.Attendance.AbsentDays,
            LateDays = input.Attendance.LateDays,
            LateMinutes = input.Attendance.LateMinutes,
            OvertimeHours = input.Attendance.OvertimeHours,
            HalfDays = input.Attendance.HalfDays + (int)(input.Leave?.HalfDays ?? 0),
            PaidLeaveDays = input.Leave?.PaidLeaveDays ?? 0,
            UnpaidLeaveDays = input.Leave?.UnpaidLeaveDays ?? 0,
            LoanInstallmentAmount = input.LoanInstallmentAmount,
            AdvanceRecoveryAmount = input.AdvanceRecoveryAmount,
            DesignationCode = input.DesignationCode,
            DepartmentCode = input.DepartmentCode,
            PeriodMonth = input.PeriodMonth,
            PeriodYear = input.PeriodYear
        };

        var entryComponents = new List<PayrollEntryComponent>();
        decimal grossRunningTotal = 0;
        decimal totalDeductions = 0;

        // 3. Sort components by sequence ASC — PRD §10.4
        var sortedComponents = input.Structure.Components
            .Where(c => c.IsActive)
            .OrderBy(c => c.Sequence)
            .ToList();

        foreach (var sc in sortedComponents)
        {
            // Evaluate AllowanceRules if present (e.g. ExcludeSummerVacation)
            if (sc.AllowanceRule != null && !IsAllowanceApplicable(sc.AllowanceRule, context))
            {
                context.ComponentValues[sc.SalaryComponentId.Value.ToString()] = 0;
                continue;
            }

            // Evaluate DeductionRules opt-in check — PRD §12.4
            if (sc.DeductionRule != null && sc.DeductionRule.IsOptIn && !input.AttendanceDeductionOptIn)
            {
                context.ComponentValues[sc.SalaryComponentId.Value.ToString()] = 0;
                continue;
            }

            // Determine formula to evaluate
            string formulaToUse = !string.IsNullOrWhiteSpace(sc.FormulaExpression)
                ? sc.FormulaExpression
                : (sc.FixedAmount.HasValue ? sc.FixedAmount.Value.ToString() : "0");

            decimal calculatedAmount = 0;

            if (!string.IsNullOrWhiteSpace(formulaToUse))
            {
                try
                {
                    calculatedAmount = _formulaEvaluator.Evaluate(formulaToUse, context);
                }
                catch
                {
                    calculatedAmount = sc.FixedAmount ?? 0;
                }
            }

            // Component type mapping (simplified for initial structures)
            ComponentType compType = ComponentType.Earning;
            if (sc.DeductionRule != null) compType = ComponentType.Deduction;

            if (compType == ComponentType.Earning)
            {
                grossRunningTotal += calculatedAmount;
                context.GrossSalary = grossRunningTotal;
            }
            else if (compType == ComponentType.Deduction)
            {
                totalDeductions += calculatedAmount;
            }

            context.ComponentValues[sc.SalaryComponentId.Value.ToString()] = calculatedAmount;

            // Generate entry component snapshot
            var entryComp = PayrollEntryComponent.Create(
                PayrollEntryId.New(),
                input.CompanyId,
                sc.SalaryComponentId,
                sc.SalaryComponentId.Value.ToString()[..8].ToUpper(),
                "Component",
                compType,
                formulaToUse,
                calculatedAmount);

            entryComponents.Add(entryComp);
        }

        // 4. Create and return populated entry
        var entry = PayrollEntry.CreateCalculated(
            input.PayrollRunId,
            input.CompanyId,
            input.ExternalEmployeeId,
            input.EmployeeCode,
            input.EmployeeName,
            input.DepartmentName,
            input.DesignationName,
            input.BankName,
            input.IBAN,
            input.BaseSalary,
            input.Attendance.WorkingDays,
            input.Attendance.AbsentDays,
            input.Attendance.LateDays,
            grossRunningTotal,
            totalDeductions,
            entryComponents);

        return Task.FromResult(entry);
    }

    private static bool IsAllowanceApplicable(Domain.Entities.Salary.AllowanceRule rule, FormulaContext context)
    {
        return rule.ApplicationMode switch
        {
            Domain.Enums.AllowanceApplicationMode.Always => true,
            Domain.Enums.AllowanceApplicationMode.ExcludeSummerVacation => context.PeriodMonth is not (7 or 8),
            _ => true
        };
    }
}
