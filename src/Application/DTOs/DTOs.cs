namespace Application.DTOs;

using Domain.Entities;

public record ProductDto(
    Guid Id, string Name, string Description, string Sku,
    decimal Price, int StockQuantity, Guid CategoryId,
    string Status, DateTime CreatedAt
)
{
    public static ProductDto FromEntity(Product p) => new(
        p.Id, p.Name, p.Description, p.Sku,
        p.Price, p.StockQuantity, p.CategoryId,
        p.Status.ToString(), p.CreatedAt
    );
}

public record OrderDto(
    Guid Id, string OrderNumber, Guid CustomerId,
    string Status, decimal Subtotal, decimal Tax, decimal Total,
    string ShippingAddress, List<OrderItemDto> Items, DateTime CreatedAt
)
{
    public static OrderDto FromEntity(Order o) => new(
        o.Id, o.OrderNumber, o.CustomerId,
        o.Status.ToString(), o.Subtotal, o.Tax, o.Total,
        o.ShippingAddress,
        o.Items.Select(i => OrderItemDto.FromEntity(i)).ToList(),
        o.CreatedAt
    );
}

public record OrderItemDto(
    Guid Id, Guid ProductId, string ProductName,
    int Quantity, decimal UnitPrice, decimal TotalPrice
)
{
    public static OrderItemDto FromEntity(OrderItem i) => new(
        i.Id, i.ProductId, i.ProductName,
        i.Quantity, i.UnitPrice, i.TotalPrice
    );
}

public record CategoryDto(
    Guid Id, string Name, string Slug, string? Description,
    Guid? ParentId, int ProductCount
)
{
    public static CategoryDto FromEntity(Category c) => new(
        c.Id, c.Name, c.Slug, c.Description,
        c.ParentId, c.Products?.Count ?? 0
    );
}

public record PagedResult<T>(
    IReadOnlyList<T> Items, int TotalCount,
    int Page, int PageSize, int TotalPages
);
