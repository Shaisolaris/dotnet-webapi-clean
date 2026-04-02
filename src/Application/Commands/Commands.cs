namespace Application.Commands;

using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

// ─── Create Product ─────────────────────────────────────

public record CreateProductCommand(
    string Name, string Description, string Sku,
    decimal Price, int StockQuantity, Guid CategoryId
) : IRequest<ProductDto>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repo;
    private readonly IUnitOfWork _uow;

    public CreateProductHandler(IProductRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<ProductDto> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var existing = await _repo.GetBySkuAsync(cmd.Sku, ct);
        if (existing != null)
            throw new DomainException($"Product with SKU '{cmd.Sku}' already exists");

        var product = new Product
        {
            Name = cmd.Name,
            Description = cmd.Description,
            Sku = cmd.Sku,
            Price = cmd.Price,
            StockQuantity = cmd.StockQuantity,
            CategoryId = cmd.CategoryId,
        };

        await _repo.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        return ProductDto.FromEntity(product);
    }
}

// ─── Create Order ───────────────────────────────────────

public record CreateOrderCommand(
    Guid CustomerId, string ShippingAddress,
    List<OrderItemInput> Items
) : IRequest<OrderDto>;

public record OrderItemInput(Guid ProductId, int Quantity);

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IUnitOfWork _uow;

    public CreateOrderHandler(IOrderRepository orderRepo, IProductRepository productRepo, IUnitOfWork uow)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _uow = uow;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = new Order
        {
            OrderNumber = await _orderRepo.GenerateOrderNumberAsync(ct),
            CustomerId = cmd.CustomerId,
            ShippingAddress = cmd.ShippingAddress,
        };

        foreach (var item in cmd.Items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId, ct)
                ?? throw new NotFoundException("Product", item.ProductId);

            if (product.StockQuantity < item.Quantity)
                throw new DomainException($"Insufficient stock for {product.Name}");

            order.AddItem(product, item.Quantity);
            product.UpdateStock(-item.Quantity);
            await _productRepo.UpdateAsync(product, ct);
        }

        await _orderRepo.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        return OrderDto.FromEntity(order);
    }
}

// ─── Confirm Order ──────────────────────────────────────

public record ConfirmOrderCommand(Guid OrderId) : IRequest<OrderDto>;

public class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repo;
    private readonly IUnitOfWork _uow;

    public ConfirmOrderHandler(IOrderRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<OrderDto> Handle(ConfirmOrderCommand cmd, CancellationToken ct)
    {
        var order = await _repo.GetWithItemsAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException("Order", cmd.OrderId);

        order.Confirm();
        await _repo.UpdateAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        return OrderDto.FromEntity(order);
    }
}

// ─── Update Product Stock ───────────────────────────────

public record UpdateStockCommand(Guid ProductId, int QuantityChange) : IRequest<ProductDto>;

public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, ProductDto>
{
    private readonly IProductRepository _repo;
    private readonly IUnitOfWork _uow;

    public UpdateStockHandler(IProductRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<ProductDto> Handle(UpdateStockCommand cmd, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new NotFoundException("Product", cmd.ProductId);

        product.UpdateStock(cmd.QuantityChange);
        await _repo.UpdateAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        return ProductDto.FromEntity(product);
    }
}
