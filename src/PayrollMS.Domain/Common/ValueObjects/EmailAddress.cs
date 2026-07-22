using System.Text.RegularExpressions;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Common.ValueObjects;

public record EmailAddress
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleViolationException("EmailRequired", "Email address cannot be empty.");

        var trimmed = value.Trim();
        if (!EmailRegex.IsMatch(trimmed))
            throw new BusinessRuleViolationException("InvalidEmailFormat", $"The email address '{value}' is invalid.");

        return new EmailAddress(trimmed);
    }

    public override string ToString() => Value;
}
