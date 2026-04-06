namespace Domain.Events;

using Domain.Entities;
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
