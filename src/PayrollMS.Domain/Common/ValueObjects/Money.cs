namespace PayrollMS.Domain.Common.ValueObjects;

public record Money(decimal Amount, string Currency = "PKR")
{
    public static Money Zero() => new(0m, "PKR");
    
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies.");
        
        return new Money(left.Amount + right.Amount, left.Currency);
    }
    
    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies.");
            
        return new Money(left.Amount - right.Amount, left.Currency);
    }
    
    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }
}
