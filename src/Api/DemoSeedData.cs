namespace Api;

using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Products.Any()) return;

        db.Products.AddRange(
            new Product { Id = Guid.NewGuid(), Name = "Pro Plan", Description = "25 projects, 10 team members, priority support", Price = 29.00m, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow },
            new Product { Id = Guid.NewGuid(), Name = "Enterprise Plan", Description = "Unlimited projects, SSO, audit logs, SLA", Price = 99.00m, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow },
            new Product { Id = Guid.NewGuid(), Name = "API Access Add-on", Description = "REST + GraphQL API access with rate limiting", Price = 15.00m, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow },
            new Product { Id = Guid.NewGuid(), Name = "Storage Upgrade", Description = "Additional 100 GB cloud storage", Price = 9.00m, Status = ProductStatus.Inactive, CreatedAt = DateTime.UtcNow }
        );
        db.SaveChanges();
        Console.WriteLine("🌱 Seeded 4 demo products");
    }
}
