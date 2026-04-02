namespace Application.Queries;

using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

// ─── Get Product ────────────────────────────────────────

public record GetProductQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IProductRepository _repo;
    public GetProductHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException("Product", query.Id);
        return ProductDto.FromEntity(product);
    }
}

// ─── Get Products Paged ─────────────────────────────────

public record GetProductsQuery(int Page = 1, int PageSize = 20, string? Search = null, Guid? CategoryId = null) : IRequest<PagedResult<ProductDto>>;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repo;
    public GetProductsHandler(IProductRepository repo) => _repo = repo;

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery query, CancellationToken ct)
    {
        var (items, total) = await _repo.GetPagedAsync(query.Page, query.PageSize, query.Search, query.CategoryId, ct);
        var dtos = items.Select(ProductDto.FromEntity).ToList();
        return new PagedResult<ProductDto>(dtos, total, query.Page, query.PageSize, (int)Math.Ceiling(total / (double)query.PageSize));
    }
}

// ─── Get Order ──────────────────────────────────────────

public record GetOrderQuery(Guid Id) : IRequest<OrderDto>;

public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IOrderRepository _repo;
    public GetOrderHandler(IOrderRepository repo) => _repo = repo;

    public async Task<OrderDto> Handle(GetOrderQuery query, CancellationToken ct)
    {
        var order = await _repo.GetWithItemsAsync(query.Id, ct)
            ?? throw new NotFoundException("Order", query.Id);
        return OrderDto.FromEntity(order);
    }
}

// ─── Get Customer Orders ────────────────────────────────

public record GetCustomerOrdersQuery(Guid CustomerId) : IRequest<IReadOnlyList<OrderDto>>;

public class GetCustomerOrdersHandler : IRequestHandler<GetCustomerOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _repo;
    public GetCustomerOrdersHandler(IOrderRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<OrderDto>> Handle(GetCustomerOrdersQuery query, CancellationToken ct)
    {
        var orders = await _repo.GetByCustomerAsync(query.CustomerId, ct);
        return orders.Select(OrderDto.FromEntity).ToList();
    }
}

// ─── Get Categories ─────────────────────────────────────

public record GetCategoriesQuery() : IRequest<IReadOnlyList<CategoryDto>>;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryRepository _repo;
    public GetCategoriesHandler(ICategoryRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery query, CancellationToken ct)
    {
        var categories = await _repo.GetRootCategoriesAsync(ct);
        return categories.Select(CategoryDto.FromEntity).ToList();
    }
}
