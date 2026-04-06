namespace Infrastructure.Repositories;

using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct) =>
        await _db.Products.Include(p => p.Category).ToListAsync(ct);

    public async Task<Product> AddAsync(Product entity, CancellationToken ct)
    {
        await _db.Products.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(Product entity, CancellationToken ct)
    {
        _db.Products.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product entity, CancellationToken ct)
    {
        entity.IsDeleted = true;
        return UpdateAsync(entity, ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct) =>
        await _db.Products.Where(p => p.CategoryId == categoryId).ToListAsync(ct);

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct) =>
        await _db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, Guid? categoryId, CancellationToken ct)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Sku.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }
}

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct) =>
        await _db.Orders.Include(o => o.Items).OrderByDescending(o => o.CreatedAt).ToListAsync(ct);

    public async Task<Order> AddAsync(Order entity, CancellationToken ct)
    {
        await _db.Orders.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(Order entity, CancellationToken ct)
    {
        _db.Orders.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Order entity, CancellationToken ct)
    {
        entity.IsDeleted = true;
        return UpdateAsync(entity, ct);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct) =>
        await _db.Orders.Where(o => o.CustomerId == customerId).Include(o => o.Items).OrderByDescending(o => o.CreatedAt).ToListAsync(ct);

    public async Task<Order?> GetWithItemsAsync(Guid id, CancellationToken ct) =>
        await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
    {
        var count = await _db.Orders.CountAsync(ct);
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{count + 1:D5}";
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public UnitOfWork(AppDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
