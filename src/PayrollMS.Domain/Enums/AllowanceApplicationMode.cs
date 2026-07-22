namespace PayrollMS.Domain.Enums;

/// <summary>
/// Controls when/whether an earning component (allowance) is applied during payroll
/// calculation. Evaluated in IsComponentApplicable() before the formula runs.
/// PRD §11.3
/// </summary>
public enum AllowanceApplicationMode
{
    /// <summary>Apply every month regardless of attendance or period.</summary>
    Always = 0,

    /// <summary>
    /// Rate × WorkingDays (actual days worked, not CalendarWorkingDays).
    /// The formula itself uses WorkingDays; this mode confirms intent.
    /// </summary>
    WorkingDaysOnly = 1,

    /// <summary>Do not apply in months July (7) and August (8) — summer vacation.</summary>
    ExcludeSummerVacation = 2,

    /// <summary>Public holidays are excluded; handled via CalendarWorkingDays vs WorkingDays.</summary>
    ExcludeHolidays = 3,

    /// <summary>Evaluate ConditionExpression (NCalc boolean) to determine applicability.</summary>
    ConditionalExpression = 4,

    /// <summary>Apply only to employees whose DesignationCode is in AllowedDesignations.</summary>
    DesignationSpecific = 5,

    /// <summary>Apply only to employees whose DepartmentCode is in AllowedDepartments.</summary>
    DepartmentSpecific = 6
}
