using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Common.ValueObjects;

public record Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }
    public string ZipCode { get; }

    private Address(string street, string city, string state, string country, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
    }

    public static Address Create(string street, string city, string state, string country, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new BusinessRuleViolationException("StreetRequired", "Street is required.");
        if (string.IsNullOrWhiteSpace(city))
            throw new BusinessRuleViolationException("CityRequired", "City is required.");
        if (string.IsNullOrWhiteSpace(state))
            throw new BusinessRuleViolationException("StateRequired", "State is required.");
        if (string.IsNullOrWhiteSpace(country))
            throw new BusinessRuleViolationException("CountryRequired", "Country is required.");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new BusinessRuleViolationException("ZipCodeRequired", "ZipCode is required.");

        return new Address(street.Trim(), city.Trim(), state.Trim(), country.Trim(), zipCode.Trim());
    }

    public override string ToString() => $"{Street}, {City}, {State}, {Country} - {ZipCode}";
}
