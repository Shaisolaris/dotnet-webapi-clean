namespace Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "USD")
{
    public static Money Zero => new(0);
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
    public override string ToString() => $"{Currency} {Amount:F2}";
}

public record Address(string Street, string City, string State, string ZipCode, string Country = "US")
{
    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}";
}
