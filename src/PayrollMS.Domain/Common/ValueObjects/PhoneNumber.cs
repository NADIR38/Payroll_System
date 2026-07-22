using System.Text.RegularExpressions;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Common.ValueObjects;

public record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{7,14}$|^0\d{9,11}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleViolationException("PhoneNumberRequired", "Phone number cannot be empty.");

        var trimmed = value.Trim().Replace(" ", "").Replace("-", "");
        if (!PhoneRegex.IsMatch(trimmed))
            throw new BusinessRuleViolationException("InvalidPhoneFormat", $"The phone number '{value}' is invalid.");

        return new PhoneNumber(trimmed);
    }

    public override string ToString() => Value;
}
