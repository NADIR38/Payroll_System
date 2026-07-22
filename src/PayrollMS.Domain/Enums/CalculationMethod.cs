namespace PayrollMS.Domain.Enums;

/// <summary>
/// The method used to compute a salary component's value. Stored as a hint/shortcut
/// on SalaryComponent; the actual NCalc expression lives on SalaryStructureComponent.
/// PRD §8.4
/// </summary>
public enum CalculationMethod
{
    /// <summary>Static rupee amount, e.g. Medical = 3000.</summary>
    Fixed = 0,

    /// <summary>A percentage of BaseSalary, e.g. HouseRent = 40%.</summary>
    PercentageOfBase = 1,

    /// <summary>Full NCalc expression with any FormulaContext variables.</summary>
    FormulaExpression = 2,

    /// <summary>Daily rate × WorkingDays, e.g. TravelAllowance = 250 * WorkingDays.</summary>
    PerWorkingDay = 3,

    /// <summary>Hourly rate × hours, used for overtime.</summary>
    PerHour = 4
}
