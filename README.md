# dotnet-webapi-clean

![CI](https://github.com/Shaisolaris/dotnet-webapi-clean/actions/workflows/ci.yml/badge.svg)

ASP.NET Core 8 Web API implementing Clean Architecture with CQRS via MediatR, Entity Framework Core, domain-driven design patterns (entities, value objects, domain events), repository pattern with Unit of Work, global exception handling, and Swagger documentation.

## Stack

- **Framework:** ASP.NET Core 8, .NET 8
- **ORM:** Entity Framework Core 8 (SQL Server)
- **CQRS:** MediatR 12
- **Validation:** FluentValidation
- **Docs:** Swagger/OpenAPI via Swashbuckle

## Architecture Layers

```
┌─────────────────────────────────────────┐
│              API Layer                  │
│  Controllers · Middleware · Program.cs  │
├─────────────────────────────────────────┤
│          Application Layer              │
│  Commands · Queries · DTOs · Interfaces │
├─────────────────────────────────────────┤
│            Domain Layer                 │
│  Entities · Value Objects · Events      │
│  Enums · Exceptions                     │
├─────────────────────────────────────────┤
│        Infrastructure Layer             │
│  DbContext · Repositories · Services    │
└─────────────────────────────────────────┘
```

Dependencies flow inward: API → Application → Domain. Infrastructure implements Application interfaces.

## Domain Model

- **Product** — SKU, price (Money value object), stock with domain event on update, soft delete, category relation
- **Category** — Hierarchical (self-referencing parent/children), slug, products collection
- **Order** — Order number generation, status state machine (Pending→Confirmed→Shipped→Delivered), line items with price calculation, tax computation
- **OrderItem** — Product snapshot (name, price at time of order), quantity, totals
- **Customer** — Email (unique), orders collection

## CQRS Commands & Queries

### Commands (write operations via MediatR)
| Command | Handler | Description |
|---|---|---|
| `CreateProductCommand` | `CreateProductHandler` | Create product with SKU uniqueness check |
| `CreateOrderCommand` | `CreateOrderHandler` | Create order with stock verification and reservation |
| `ConfirmOrderCommand` | `ConfirmOrderHandler` | Confirm order (state machine guard) |
| `UpdateStockCommand` | `UpdateStockHandler` | Adjust stock with domain event |

### Queries (read operations via MediatR)
| Query | Handler | Description |
|---|---|---|
| `GetProductQuery` | `GetProductHandler` | Single product by ID |
| `GetProductsQuery` | `GetProductsHandler` | Paged products with search and category filter |
| `GetOrderQuery` | `GetOrderHandler` | Order with items |
| `GetCustomerOrdersQuery` | `GetCustomerOrdersHandler` | Customer's order history |
| `GetCategoriesQuery` | `GetCategoriesHandler` | Root categories |

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/products` | Paged products (search, categoryId, page, pageSize) |
| GET | `/api/products/{id}` | Single product |
| POST | `/api/products` | Create product |
| PATCH | `/api/products/{id}/stock` | Update stock quantity |
| GET | `/api/orders/{id}` | Order with items |
| GET | `/api/orders/customer/{customerId}` | Customer's orders |
| POST | `/api/orders` | Create order (reserves stock) |
| POST | `/api/orders/{id}/confirm` | Confirm order |
| GET | `/api/categories` | All root categories |

## File Structure

```
src/
├── Api/
│   ├── Controllers/Controllers.cs      # Products, Orders, Categories controllers
│   ├── Middleware/ExceptionHandler.cs   # Global exception → HTTP status mapping
│   ├── Program.cs                      # DI, MediatR, EF Core, pipeline setup
│   ├── Api.csproj
│   └── appsettings.json
├── Application/
│   ├── Commands/Commands.cs            # CreateProduct, CreateOrder, ConfirmOrder, UpdateStock
│   ├── Queries/Queries.cs              # GetProduct(s), GetOrder, GetCustomerOrders, GetCategories
│   ├── DTOs/DTOs.cs                    # ProductDto, OrderDto, CategoryDto, PagedResult<T>
│   └── Interfaces/Interfaces.cs        # IRepository<T>, IProductRepository, IOrderRepository, IUnitOfWork
├── Domain/
│   ├── Entities/Entities.cs            # BaseEntity, Product, Category, Order, OrderItem, Customer
│   ├── Enums/Enums.cs                  # ProductStatus, OrderStatus
│   └── ValueObjects/ValueObjects.cs    # Money, Address, DomainException, NotFoundException, Events
└── Infrastructure/
    └── Data/DataAccess.cs              # AppDbContext (Fluent API config), ProductRepository, OrderRepository, UnitOfWork
```

## Setup

```bash
git clone https://github.com/Shaisolaris/dotnet-webapi-clean.git
cd dotnet-webapi-clean
dotnet restore
dotnet ef database update --project src/Api
dotnet run --project src/Api
# → https://localhost:5001/swagger
```

## Key Design Decisions

**MediatR for CQRS.** Commands and queries are separate record types handled by dedicated handlers. This decouples controllers from business logic and makes each operation independently testable. The controller's only job is HTTP mapping.

**Domain events on entities.** `BaseEntity` tracks domain events via `AddDomainEvent()`. When `Product.UpdateStock()` modifies quantity, it records a `ProductStockUpdatedEvent`. Events are dispatched after `SaveChangesAsync()` for eventual consistency patterns.

**Repository + Unit of Work.** Repositories handle individual aggregate queries. `IUnitOfWork.SaveChangesAsync()` commits all changes in a single transaction. Commands inject both the repository and UoW, ensuring atomicity across multiple aggregate modifications (e.g., order creation reserves stock across multiple products).

**Global exception middleware.** `ExceptionHandlerMiddleware` maps domain exceptions to HTTP status codes: `NotFoundException` → 404, `DomainException` → 422, `UnauthorizedAccessException` → 401. This eliminates try/catch in controllers and provides consistent error responses.

**Soft delete via query filter.** Products use `IsDeleted` with EF Core's `HasQueryFilter`. Deleted products are automatically excluded from all queries without requiring `Where(!IsDeleted)` everywhere.

## License

MIT
