using NCalc;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace PayrollMS.Infrastructure.Services;

/// <summary>
/// Sandboxed NCalc formula evaluation service.
/// Implements PRD §10.2, §10.6, §10.7.
/// </summary>
public sealed class FormulaEvaluatorService : IFormulaEvaluator
{
    private static readonly HashSet<string> ApprovedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Abs", "Round", "Floor", "Ceiling", "Min", "Max", "If"
    };

    private static readonly HashSet<string> DefaultContextVariables = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(FormulaContext.BaseSalary),
        nameof(FormulaContext.GrossSalary),
        nameof(FormulaContext.CalendarWorkingDays),
        nameof(FormulaContext.WorkingDays),
        nameof(FormulaContext.AbsentDays),
        nameof(FormulaContext.LateDays),
        nameof(FormulaContext.LateMinutes),
        nameof(FormulaContext.OvertimeHours),
        nameof(FormulaContext.PaidLeaveDays),
        nameof(FormulaContext.UnpaidLeaveDays),
        nameof(FormulaContext.HalfDays),
        nameof(FormulaContext.LoanInstallmentAmount),
        nameof(FormulaContext.AdvanceRecoveryAmount)
    };

    public decimal Evaluate(string expression, FormulaContext context)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new FormulaEvaluationException("EmptyExpression", expression, "Formula expression cannot be empty.");

        try
        {
            var expr = new Expression(expression);

            if (expr.HasErrors())
            {
                throw new FormulaEvaluationException(
                    "SyntaxError",
                    expression,
                    $"Syntax error in formula expression: {expr.Error}");
            }

            // Populate parameters from context
            expr.Parameters["BaseSalary"] = Convert.ToDouble(context.BaseSalary);
            expr.Parameters["GrossSalary"] = Convert.ToDouble(context.GrossSalary);
            expr.Parameters["CalendarWorkingDays"] = context.CalendarWorkingDays;
            expr.Parameters["WorkingDays"] = context.WorkingDays;
            expr.Parameters["AbsentDays"] = context.AbsentDays;
            expr.Parameters["LateDays"] = context.LateDays;
            expr.Parameters["LateMinutes"] = context.LateMinutes;
            expr.Parameters["OvertimeHours"] = Convert.ToDouble(context.OvertimeHours);
            expr.Parameters["PaidLeaveDays"] = Convert.ToDouble(context.PaidLeaveDays);
            expr.Parameters["UnpaidLeaveDays"] = Convert.ToDouble(context.UnpaidLeaveDays);
            expr.Parameters["HalfDays"] = Convert.ToDouble(context.HalfDays);
            expr.Parameters["LoanInstallmentAmount"] = Convert.ToDouble(context.LoanInstallmentAmount);
            expr.Parameters["AdvanceRecoveryAmount"] = Convert.ToDouble(context.AdvanceRecoveryAmount);

            foreach (var kvp in context.ComponentValues)
            {
                expr.Parameters[kvp.Key] = Convert.ToDouble(kvp.Value);
            }

            var result = expr.Evaluate();

            if (result is null)
                throw new FormulaEvaluationException("NullResult", expression, "Formula returned a null value.");

            return Convert.ToDecimal(result);
        }
        catch (FormulaEvaluationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FormulaEvaluationException(
                "EvaluationFailed",
                expression,
                $"Failed to evaluate formula '{expression}': {ex.Message}");
        }
    }

    public IReadOnlyList<string> Validate(string expression, IEnumerable<string> knownVariables)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(expression))
        {
            errors.Add("Formula expression cannot be empty.");
            return errors;
        }

        if (expression.Length > 1000)
        {
            errors.Add("Formula expression exceeds maximum allowed length of 1000 characters.");
            return errors;
        }

        // 1. Check syntax via NCalc
        Expression expr;
        try
        {
            expr = new Expression(expression);
            if (expr.HasErrors())
            {
                errors.Add($"Syntax error: {expr.Error}");
                return errors;
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Failed to parse expression: {ex.Message}");
            return errors;
        }

        // 2. Validate function whitelist
        // Match function calls like FunctionName(...)
        var functionMatches = Regex.Matches(expression, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\s*\(");
        foreach (Match match in functionMatches)
        {
            var functionName = match.Groups[1].Value;
            if (!ApprovedFunctions.Contains(functionName))
            {
                errors.Add($"Function '{functionName}' is not allowed. Approved functions: Abs, Round, Floor, Ceiling, Min, Max, If.");
            }
        }

        // 3. Check undefined variables
        var validVariableSet = new HashSet<string>(DefaultContextVariables, StringComparer.OrdinalIgnoreCase);
        foreach (var v in knownVariables)
        {
            validVariableSet.Add(v);
        }

        // Extract identifier tokens (ignoring numbers, strings, and approved function names)
        var identifierMatches = Regex.Matches(expression, @"\b[a-zA-Z_][a-zA-Z0-9_]*\b");
        foreach (Match match in identifierMatches)
        {
            var token = match.Value;

            // Skip approved function names and boolean literals
            if (ApprovedFunctions.Contains(token) ||
                string.Equals(token, "true", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(token, "false", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!validVariableSet.Contains(token))
            {
                errors.Add($"Undefined variable or unapproved component reference: '{token}'.");
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        // 4. Dry-run evaluation with dummy values (BaseSalary = 100000, CalendarWorkingDays = 22, WorkingDays = 22)
        try
        {
            var dryContext = new FormulaContext
            {
                BaseSalary = 100000m,
                CalendarWorkingDays = 22,
                WorkingDays = 22,
                AbsentDays = 0,
                LateDays = 0,
                LateMinutes = 0,
                OvertimeHours = 0m,
                PaidLeaveDays = 0m,
                UnpaidLeaveDays = 0m,
                HalfDays = 0m,
                LoanInstallmentAmount = 0m,
                AdvanceRecoveryAmount = 0m
            };

            foreach (var varName in knownVariables)
            {
                if (!DefaultContextVariables.Contains(varName))
                {
                    dryContext.ComponentValues[varName] = 10000m;
                }
            }

            Evaluate(expression, dryContext);
        }
        catch (Exception ex)
        {
            errors.Add($"Dry-run evaluation failed: {ex.Message}");
        }

        return errors;
    }
}
