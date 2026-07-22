namespace PayrollMS.Domain.Exceptions;

public class BusinessRuleViolationException : DomainException
{
    public string ErrorCode { get; }

    public BusinessRuleViolationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
