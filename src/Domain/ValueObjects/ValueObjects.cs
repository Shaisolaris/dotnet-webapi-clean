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

namespace Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"Entity \"{entity}\" ({key}) was not found.") { }
}

namespace Domain.Events;

using Domain.Entities;

public class OrderConfirmedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public OrderConfirmedEvent(Guid orderId, string orderNumber)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
    }
}

public class ProductStockUpdatedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public int NewQuantity { get; }
    public ProductStockUpdatedEvent(Guid productId, int newQuantity)
    {
        ProductId = productId;
        NewQuantity = newQuantity;
    }
}
