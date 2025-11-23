# ?? Xpandables.Net.Primitives.Composition

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **DI Composition** - Service export and composition utilities for Microsoft.Extensions.DependencyInjection with automatic service discovery and registration.

---

## ?? Overview

`Xpandables.Net.Primitives.Composition` provides utilities for automatic service discovery and registration in the Microsoft DI container. It includes the `IAddServiceExport` interface for marking services for auto-registration and extension methods for scanning assemblies.

### ?? Key Features

- ??? **EF Core Integration** - Full DbContext support
- ?? **Unit of Work** - Transaction management across operations
- ? **Bulk Operations** - Efficient batch insert/update/delete
- ?? **LINQ Support** - Full queryable support with async enumeration
- ?? **Event Store** - Built-in event sourcing with EventStoreDataContext
- ?? **Outbox Pattern** - Reliable event publishing with OutboxStoreDataContext
- ? **Testable** - Easy to mock and test

---

## ?? Quick Start

### Installation

```bash
dotnet add package Xpandables.Net.EntityFramework
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### Basic Setup

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Repositories;

// Define your DbContext
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

// Register services
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repository
builder.Services.AddXRepository<AppDbContext>();

// Usage
public class UserService
{
    private readonly IRepository<AppDbContext> _repository;

    public UserService(IRepository<AppDbContext> repository) 
        => _repository = repository;

    public async Task<List<User>> GetActiveUsersAsync(
        CancellationToken cancellationToken)
    {
        return await _repository
            .FetchAsync<User, User>(q => q.Where(u => u.IsActive))
            .ToListAsync(cancellationToken);
    }
}
```

---

## ?? Core Operations

### Query Operations

```csharp
// Simple query
var users = await _repository
    .FetchAsync<User, User>(q => q.Where(u => u.IsActive))
    .ToListAsync();

// Projection
var userDtos = await _repository
    .FetchAsync<User, UserDto>(q => q
        .Where(u => u.IsActive)
        .Select(u => new UserDto { Id = u.Id, Name = u.Name }))
    .ToListAsync();

// With ordering and pagination
var pagedUsers = await _repository
    .FetchAsync<User, User>(q => q
        .Where(u => u.Age >= 18)
        .OrderBy(u => u.Name)
        .Skip(page * pageSize)
        .Take(pageSize))
    .ToListAsync();

// Join queries
var ordersWithUsers = await _repository
    .FetchAsync<Order, OrderDto>(q => q
        .Include(o => o.User)
        .Include(o => o.Items)
        .Where(o => o.Status == OrderStatus.Pending)
        .Select(o => new OrderDto
        {
            OrderId = o.Id,
            UserName = o.User.Name,
            TotalItems = o.Items.Count
        }))
    .ToListAsync();

// Async enumeration
await foreach (var user in _repository
    .FetchAsync<User, User>(q => q.Where(u => u.IsActive)))
{
    await ProcessUserAsync(user);
}
```

### Insert Operations

```csharp
// Single insert
var user = new User 
{ 
    Name = "John Doe", 
    Email = "john@example.com" 
};
await _repository.AddAsync(cancellationToken, user);

// Bulk insert
var users = new []
{
    new User { Name = "Alice", Email = "alice@example.com" },
    new User { Name = "Bob", Email = "bob@example.com" },
    new User { Name = "Charlie", Email = "charlie@example.com" }
};
await _repository.AddAsync(cancellationToken, users);
```

### Update Operations

#### Update by Entity

```csharp
// Load and update
var users = await _repository
    .FetchAsync<User, User>(q => q.Where(u => u.Id == userId))
    .FirstOrDefaultAsync();

if (user != null)
{
    user.Name = "Updated Name";
    user.LastModifiedDate = DateTime.UtcNow;
    await _repository.UpdateAsync(cancellationToken, user);
}
```

#### Bulk Update with Expression

```csharp
// Update all matching records
await _repository.UpdateAsync<User>(
    q => q.Where(u => u.Age < 18),
    u => new User 
    { 
        Status = "Minor",
        LastModifiedDate = DateTime.UtcNow
    });
```

#### Bulk Update with Action

```csharp
await _repository.UpdateAsync<User>(
    q => q.Where(u => u.IsActive),
    user =>
    {
        user.LastLoginDate = DateTime.UtcNow;
        user.LoginCount++;
    });
```

#### Bulk Update with Fluent API

```csharp
var updater = EntityUpdater<User>
    .Create()
    .SetProperty(u => u.Status, "Active")
    .SetProperty(u => u.LastModifiedDate, DateTime.UtcNow)
    .SetProperty(u => u.LoginCount, u => u.LoginCount + 1);

await _repository.UpdateAsync(
    q => q.Where(u => u.Email.Contains("@example.com")),
    updater);
```

### Delete Operations

```csharp
// Delete by filter
await _repository.DeleteAsync<User>(
    q => q.Where(u => !u.IsActive && u.CreatedDate < oldDate));

// Delete with complex conditions
await _repository.DeleteAsync<Order>(
    q => q
        .Where(o => o.Status == OrderStatus.Cancelled)
        .Where(o => o.CreatedDate < DateTime.UtcNow.AddMonths(-6)));
```

---

## ?? Transactions & Unit of Work

### Basic Transactions

```csharp
public async Task TransferFundsAsync(
    Guid fromAccountId, 
    Guid toAccountId, 
    decimal amount)
{
    using var transaction = await _repository.BeginTransactionAsync();

    try
    {
        // Debit from account
        await _repository.UpdateAsync<Account>(
            q => q.Where(a => a.Id == fromAccountId),
            a => new Account { Balance = a.Balance - amount });

        // Credit to account
        await _repository.UpdateAsync<Account>(
            q => q.Where(a => a.Id == toAccountId),
            a => new Account { Balance = a.Balance + amount });

        // Create transaction record
        await _repository.AddAsync(default, new Transaction
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Date = DateTime.UtcNow
        });

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### Unit of Work Pattern

```csharp
// Enable unit of work mode
_repository.IsUnitOfWorkEnabled = true;

try
{
    // All operations are batched
    await _repository.AddAsync(cancellationToken, user);
    
    await _repository.UpdateAsync<Order>(
        q => q.Where(o => o.UserId == user.Id),
        o => new Order { Status = OrderStatus.Active });

    await _repository.DeleteAsync<TempData>(
        q => q.Where(t => t.IsExpired));

    // Commit all changes at once
    await _repository.PersistAsync(cancellationToken);
}
catch
{
    // Changes are rolled back automatically
    throw;
}
finally
{
    _repository.IsUnitOfWorkEnabled = false;
}
```

---

## ?? Event Sourcing Integration

### Event Store Setup

```csharp
using Xpandables.Net.Events;
using Microsoft.EntityFrameworkCore;

// Configure Event Store
builder.Services.AddXEventStoreDataContext(options =>
    options
        .UseSqlServer(
            builder.Configuration.GetConnectionString("EventStoreDb"),
            sqlOptions => sqlOptions
                .EnableRetryOnFailure()
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                .MigrationsHistoryTable("__EventStoreMigrations")
                .MigrationsAssembly("MyApp"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging());

// Register event sourcing services
builder.Services
    .AddXAggregateStore()
    .AddXEventStore()
    .AddXPublisher();
```

### Using Event Store

```csharp
public sealed class OrderService
{
    private readonly IAggregateStore _aggregateStore;

    public OrderService(IAggregateStore aggregateStore) 
        => _aggregateStore = aggregateStore;

    public async Task<ExecutionResult<OrderAggregate>> CreateOrderAsync(
        CreateOrderCommand command)
    {
        // Create aggregate (generates events)
        var order = OrderAggregate.Create(
            command.CustomerId,
            command.Items);

        // Persist events
        await _aggregateStore.AppendAsync(order);

        return ExecutionResult.Created(order);
    }

    public async Task<OrderAggregate> GetOrderAsync(Guid orderId)
    {
        // Rebuild from events
        return await _aggregateStore
            .ReadAsync<OrderAggregate>(orderId);
    }
}
```

### Outbox Pattern

```csharp
// Configure Outbox Store
builder.Services.AddXOutboxStoreDataContext(options =>
    options
        .UseSqlServer(
            builder.Configuration.GetConnectionString("EventStoreDb"),
            sqlOptions => sqlOptions
                .EnableRetryOnFailure()
                .MigrationsHistoryTable("__OutboxStoreMigrations")
                .MigrationsAssembly("MyApp")));

// Register outbox services
builder.Services.AddXOutboxStore();

// Events are automatically stored in outbox
// and published reliably by background service
```

---

## ?? Advanced Configuration

### Custom Model Configuration

```csharp
public class EventStoreModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        // Configure event entity
        modelBuilder.Entity<EntityDomainEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AggregateId);
            entity.HasIndex(e => e.AggregateName);
            entity.Property(e => e.EventData).HasColumnType("nvarchar(max)");
        });

        // Configure snapshot entity
        modelBuilder.Entity<EntitySnapshotEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AggregateId).IsUnique();
            entity.Property(e => e.SnapshotData).HasColumnType("nvarchar(max)");
        });
    }
}
```

### Repository with Specifications

```csharp
using Xpandables.Net.Validators;

// Define specifications
var activeUsersSpec = Specification
    .Equal<User, bool>(u => u.IsActive, true);

var adultsSpec = Specification
    .GreaterThan<User, int>(u => u.Age, 18);

var combinedSpec = activeUsersSpec.And(adultsSpec);

// Use in repository
var users = await _repository
    .FetchAsync<User, User>(q => q.Where(combinedSpec))
    .ToListAsync();
```

---

## ?? Best Practices

1. **Use projections** for read operations to reduce data transfer
2. **Enable Unit of Work** when performing multiple related operations
3. **Use transactions** for operations that must succeed or fail together
4. **Leverage bulk operations** for better performance
5. **Apply specifications** to encapsulate business rules
6. **Use async enumeration** for large result sets
7. **Configure indexes** properly for frequently queried fields

---

## ?? Performance Tips

```csharp
// Use AsNoTracking for read-only queries
var users = await _repository
    .FetchAsync<User, User>(q => q
        .AsNoTracking()
        .Where(u => u.IsActive))
    .ToListAsync();

// Use projections instead of loading full entities
var userNames = await _repository
    .FetchAsync<User, string>(q => q
        .Where(u => u.IsActive)
        .Select(u => u.Name))
    .ToListAsync();

// Batch operations
var updater = EntityUpdater<User>
    .Create()
    .SetProperty(u => u.LastUpdated, DateTime.UtcNow);

await _repository.UpdateAsync(
    q => q.Where(u => u.IsActive),
    updater);  // Single SQL UPDATE statement
```

---

## ?? Related Packages

- **Xpandables.Net** - Core library with abstractions
- **Xpandables.Net.AspNetCore** - ASP.NET Core integrations
- **Xpandables.Net.SampleApi** - Complete working example

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
