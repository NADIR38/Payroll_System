namespace PayrollMS.Domain.Interfaces.Services;

/// <summary>
/// Context populated before NCalc formula evaluation.
/// Contains all variables available inside a salary formula.
/// PRD §10.3
/// </summary>
public sealed class FormulaContext
{
    // ── Employee variables ──────────────────────────────────────────────────
    public decimal BaseSalary { get; set; }

    /// <summary>Running total of all Earning components computed so far (in sequence order).</summary>
    public decimal GrossSalary { get; set; }

    // ── Calendar variables ──────────────────────────────────────────────────
    /// <summary>Authoritative working-day count from PayrollCalendar. Used as the denominator in per-day formulas.</summary>
    public int CalendarWorkingDays { get; set; }

    /// <summary>Actual days the employee was present (from AttendanceSummary).</summary>
    public int WorkingDays { get; set; }

    public int AbsentDays { get; set; }
    public int LateDays { get; set; }

    /// <summary>Accumulated late minutes across all days in the period.</summary>
    public int LateMinutes { get; set; }

    public decimal OvertimeHours { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal HalfDays { get; set; }

    // ── Loan / Advance variables ────────────────────────────────────────────
    public decimal LoanInstallmentAmount { get; set; }
    public decimal AdvanceRecoveryAmount { get; set; }

    // ── Computed component values (populated as each component is evaluated) ─
    /// <summary>
    /// Keyed by component Code (e.g. "BASIC", "HOUSE_RENT").
    /// Lower-sequence components are available as variables in higher-sequence formulas.
    /// </summary>
    public Dictionary<string, decimal> ComponentValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // ── Employee profile variables (for AllowanceRule evaluation) ─────────────

    /// <summary>Used by DesignationSpecific AllowanceRule — PRD §11.3</summary>
    public string DesignationCode { get; set; } = string.Empty;

    /// <summary>Used by DepartmentSpecific AllowanceRule — PRD §11.3</summary>
    public string DepartmentCode { get; set; } = string.Empty;

    /// <summary>Current period month (1–12) — used by ExcludeSummerVacation rule — PRD §11.3</summary>
    public int PeriodMonth { get; set; }

    /// <summary>Current period year.</summary>
    public int PeriodYear { get; set; }
}

/// <summary>
/// Sandboxed NCalc expression evaluator.
/// Only approved functions (Abs, Round, Floor, Ceiling, Min, Max, If) are permitted.
/// PRD §10.2, §10.7
/// </summary>
public interface IFormulaEvaluator
{
    /// <summary>
    /// Evaluates an NCalc expression against a variable context.
    /// Returns the computed decimal value.
    /// </summary>
    /// <exception cref="Domain.Exceptions.FormulaEvaluationException">
    /// Thrown when expression evaluation fails at runtime (division by zero, type error, etc.)
    /// </exception>
    decimal Evaluate(string expression, FormulaContext context);

    /// <summary>
    /// Validates an expression for syntax errors, undefined variables, dry-run errors,
    /// and circular dependency violations (PRD §10.6, all 4 checks).
    /// Returns an empty list when the expression is valid.
    /// </summary>
    /// <param name="expression">The NCalc expression string to validate.</param>
    /// <param name="knownVariables">
    /// All known variable names: FormulaContext property names + component codes of
    /// already-defined lower-sequence components.
    /// </param>
    IReadOnlyList<string> Validate(string expression, IEnumerable<string> knownVariables);
}
