# 🗃️ System.Entities.Data

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/System.Entities.Data)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **Entity Framework Core Repository & Unit of Work** — Production-ready EF Core implementation with `DataContext`, automatic entity lifecycle tracking, `EntityUpdater` bulk operations, and transaction management.

---

## 📋 Overview

`System.Entities.Data` provides a complete Entity Framework Core implementation of the repository and unit of work patterns. The library includes `DataContext` with automatic entity lifecycle tracking (`CreatedOn`, `UpdatedOn`, `DeletedOn`), `Repository` for CRUD operations, `UnitOfWork` for transaction management, and `EntityUpdater` for efficient bulk updates using EF Core 10's `ExecuteUpdate` API.

Built for .NET 10 with C# 14 extension members, this package simplifies data access while maintaining full control over database operations.

### ✨ Key Features

- 🗄️ **`DataContext`** — Extended `DbContext` with automatic entity lifecycle tracking
- 📦 **`Repository<TDataContext>`** — Generic EF Core repository with async LINQ support
- 🔄 **`UnitOfWork<TDataContext>`** — Transaction management with repository coordination
- ⚡ **`EntityUpdater<T>`** — Fluent API for bulk updates using `ExecuteUpdate`/`ExecuteDelete`
- 📅 **Entity Lifecycle** — Automatic `CreatedOn`/`UpdatedOn`/`DeletedOn` timestamps via `IEntity`
- 🔧 **Value Converters** — `JsonDocument` and `ReadOnlyMemory<byte>` EF Core converters
- 🎯 **Type Safe** — Full `DynamicallyAccessedMembers` attribute support for trimming

---

## 📦 Installation

```bash
dotnet add package System.Entities.Data
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

Or via NuGet Package Manager:

```powershell
Install-Package System.Entities.Data
Install-Package Microsoft.EntityFrameworkCore.SqlServer
```

---

## 🚀 Quick Start

### Define Your DataContext

```csharp
using System.Entities.Data;
using Microsoft.EntityFrameworkCore;

public class AppDataContext : DataContext
{
    public AppDataContext(DbContextOptions<AppDataContext> options) 
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId);
        });
    }
}
```

### Define Entities with IEntity

```csharp
using System.Entities;

public class User : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Automatic tracking properties from IEntity
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public EntityStatus Status { get; set; }
    
    // Navigation
    public ICollection<Order> Orders { get; set; } = [];
}

public class Order : IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Total { get; set; }
    public OrderStatus OrderStatus { get; set; }
    
    // IEntity properties
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public EntityStatus Status { get; set; }
    
    // Navigation
    public User User { get; set; } = null!;
}
```

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register DataContext
builder.Services.AddXDataContext<AppDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repository and UnitOfWork
builder.Services.AddXEntityFrameworkRepositories<AppDataContext>();

var app = builder.Build();
app.Run();
```

---

## 📦 Repository Operations

### Query Operations

```csharp
using System.Entities;
using Microsoft.EntityFrameworkCore;

public class UserService(IRepository<AppDataContext> repository)
{
    // Simple query with AsNoTracking (applied by default in FetchAsync)
    public async Task<List<User>> GetActiveUsersAsync(CancellationToken ct)
    {
        return await repository
            .FetchAsync<User, User>(q => q.Where(u => u.IsActive))
            .ToListAsync(ct);
    }

    // Projection to DTO
    public async Task<List<UserDto>> GetUserDtosAsync(CancellationToken ct)
    {
        return await repository
            .FetchAsync<User, UserDto>(q => q
                .Where(u => u.IsActive)
                .Select(u => new UserDto(u.Id, u.Name, u.Email)))
            .ToListAsync(ct);
    }

    // With ordering and pagination
    public async Task<List<User>> GetPagedUsersAsync(int page, int pageSize, CancellationToken ct)
    {
        return await repository
            .FetchAsync<User, User>(q => q
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .Skip(page * pageSize)
                .Take(pageSize))
            .ToListAsync(ct);
    }

    // Join queries with Include (remove AsNoTracking if tracking needed)
    public async Task<List<OrderDto>> GetOrdersWithUsersAsync(CancellationToken ct)
    {
        return await repository
            .FetchAsync<Order, OrderDto>(q => q
                .Include(o => o.User)
                .Where(o => o.OrderStatus == OrderStatus.Pending)
                .Select(o => new OrderDto
                {
                    OrderId = o.Id,
                    UserName = o.User.Name,
                    Total = o.Total
                }))
            .ToListAsync(ct);
    }

    // Async enumeration for large datasets
    public async Task ProcessAllUsersAsync(CancellationToken ct)
    {
        await foreach (var user in repository
            .FetchAsync<User, User>(q => q.Where(u => u.IsActive))
            .WithCancellation(ct))
        {
            await ProcessUserAsync(user, ct);
        }
    }
}
```

### Insert Operations

```csharp
public class UserService(IRepository<AppDataContext> repository)
{
    // Single insert - CreatedOn is set automatically
    public async Task<User> CreateUserAsync(string name, string email, CancellationToken ct)
    {
        var user = new User 
        { 
            Id = Guid.NewGuid(),
            Name = name, 
            Email = email,
            IsActive = true
            // CreatedOn will be set automatically by DataContext
        };
        
        // When IsUnitOfWorkEnabled = true (default), changes are tracked but not saved
        // Set to false for immediate persistence
        repository.IsUnitOfWorkEnabled = false;
        await repository.AddAsync(ct, user);
        
        return user;
    }

    // Bulk insert
    public async Task CreateUsersAsync(IEnumerable<CreateUserRequest> requests, CancellationToken ct)
    {
        var users = requests.Select(r => new User
        {
            Id = Guid.NewGuid(),
            Name = r.Name,
            Email = r.Email,
            IsActive = true
        }).ToArray();

        repository.IsUnitOfWorkEnabled = false;
        await repository.AddAsync(ct, users);
    }
}
```

### Update Operations

#### Update by Entity

```csharp
public async Task UpdateUserNameAsync(Guid userId, string newName, CancellationToken ct)
{
    // Load entity
    var user = await repository
        .FetchAsync<User, User>(q => q.Where(u => u.Id == userId))
        .FirstOrDefaultAsync(ct);

    if (user is null) return;

    user.Name = newName;
    // UpdatedOn will be set automatically by DataContext
    
    repository.IsUnitOfWorkEnabled = false;
    await repository.UpdateAsync(ct, user);
}
```

#### Bulk Update with Expression

```csharp
public async Task DeactivateOldUsersAsync(DateTime cutoffDate, CancellationToken ct)
{
    repository.IsUnitOfWorkEnabled = false;
    
    await repository.UpdateAsync<User>(
        q => q.Where(u => u.CreatedOn < cutoffDate && u.IsActive),
        u => new User 
        { 
            IsActive = false,
            Status = EntityStatus.INACTIVE
        },
        ct);
}
```

#### Bulk Update with Action

```csharp
public async Task UpdateLastLoginAsync(IEnumerable<Guid> userIds, CancellationToken ct)
{
    repository.IsUnitOfWorkEnabled = false;
    
    await repository.UpdateAsync<User>(
        q => q.Where(u => userIds.Contains(u.Id)),
        user =>
        {
            user.LastLoginDate = DateTime.UtcNow;
            user.LoginCount++;
            // UpdatedOn will be set automatically
        },
        ct);
}
```

#### Bulk Update with EntityUpdater (EF Core 10 ExecuteUpdate)

```csharp
using System.Entities;

public async Task UpdateUserStatusesAsync(CancellationToken ct)
{
    var updater = EntityUpdater<User>
        .Create()
        .SetProperty(u => u.IsActive, true)
        .SetProperty(u => u.LoginCount, u => u.LoginCount + 1)
        .SetProperty(u => u.UpdatedOn, DateTime.UtcNow);

    // When IsUnitOfWorkEnabled = false, uses ExecuteUpdateAsync (single SQL statement)
    repository.IsUnitOfWorkEnabled = false;
    
    await repository.UpdateAsync(
        q => q.Where(u => u.Email.EndsWith("@company.com")),
        updater,
        ct);
    // Generates: UPDATE Users SET IsActive = 1, LoginCount = LoginCount + 1, UpdatedOn = @p0 
    //            WHERE Email LIKE '%@company.com'
}
```

### Delete Operations

```csharp
public async Task DeleteInactiveUsersAsync(DateTime cutoffDate, CancellationToken ct)
{
    // When IsUnitOfWorkEnabled = false, uses ExecuteDeleteAsync (single SQL statement)
    repository.IsUnitOfWorkEnabled = false;
    
    await repository.DeleteAsync<User>(
        q => q.Where(u => !u.IsActive && u.CreatedOn < cutoffDate),
        ct);
    // Generates: DELETE FROM Users WHERE IsActive = 0 AND CreatedOn < @p0
}

public async Task DeleteCancelledOrdersAsync(CancellationToken ct)
{
    repository.IsUnitOfWorkEnabled = false;
    
    await repository.DeleteAsync<Order>(
        q => q
            .Where(o => o.OrderStatus == OrderStatus.Cancelled)
            .Where(o => o.CreatedOn < DateTime.UtcNow.AddMonths(-6)),
        ct);
}
```

---

## 🔄 Unit of Work Pattern

### Basic Usage

```csharp
using System.Entities;
using System.Entities.Data;

public class OrderProcessingService(IUnitOfWork<AppDataContext> unitOfWork)
{
    public async Task ProcessOrderAsync(CreateOrderRequest request, CancellationToken ct)
    {
        // Get repository from unit of work
        var repository = unitOfWork.GetRepository<IRepository>();
        
        // All operations use the same DbContext
        var user = await repository
            .FetchAsync<User, User>(q => q.Where(u => u.Id == request.UserId))
            .FirstOrDefaultAsync(ct);

        if (user is null)
            throw new InvalidOperationException("User not found");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Total = request.Total,
            OrderStatus = OrderStatus.Pending
        };

        await repository.AddAsync(ct, order);
        
        // Explicitly save all changes
        await unitOfWork.SaveChangesAsync(ct);
    }
}
```

### With Transactions

```csharp
public async Task TransferFundsAsync(Guid fromUserId, Guid toUserId, decimal amount, CancellationToken ct)
{
    await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
    
    try
    {
        var repository = unitOfWork.GetRepository<IRepository>();
        
        // Debit from source
        await repository.UpdateAsync<User>(
            q => q.Where(u => u.Id == fromUserId),
            user => user.Balance -= amount,
            ct);

        // Credit to destination
        await repository.UpdateAsync<User>(
            q => q.Where(u => u.Id == toUserId),
            user => user.Balance += amount,
            ct);

        // Save and commit
        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitTransactionAsync(ct);
    }
    catch
    {
        await transaction.RollbackTransactionAsync(ct);
        throw;
    }
}
```

### Using External Transaction

```csharp
public async Task ProcessWithExternalTransactionAsync(
    DbTransaction externalTransaction,
    CancellationToken ct)
{
    // Use existing transaction from another DbContext or ADO.NET
    await using var transaction = await unitOfWork.UseTransactionAsync(externalTransaction, ct);
    
    var repository = unitOfWork.GetRepository<IRepository>();
    // ... perform operations
    
    await unitOfWork.SaveChangesAsync(ct);
    // Don't commit - external transaction owner commits
}
```

---

## 📅 Automatic Entity Lifecycle Tracking

The `DataContext` automatically tracks entity lifecycle events via `ChangeTracker` events:

```csharp
public class DataContext : DbContext
{
    protected DataContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.Tracked += static (sender, e) => OnEntityTracked(e);
        ChangeTracker.StateChanged += static (sender, e) => OnEntityStateChanged(e);
    }

    // When entity is added to context (not from query)
    private static void OnEntityTracked(EntityTrackedEventArgs e)
    {
        if (e is { FromQuery: false, Entry: { State: EntityState.Added, Entity: IEntity entity } })
        {
            entity.CreatedOn = DateTime.UtcNow;
        }
    }

    // When entity state changes
    private static void OnEntityStateChanged(EntityStateChangedEventArgs e)
    {
        if (e is { NewState: EntityState.Modified, Entry.Entity: IEntity entity })
        {
            entity.UpdatedOn = DateTime.UtcNow;
        }

        if (e is { NewState: EntityState.Deleted, Entry.Entity: IEntity deletedEntity })
        {
            deletedEntity.DeletedOn = DateTime.UtcNow;
            deletedEntity.Status = EntityStatus.DELETED;
        }
    }
}
```

### Usage

```csharp
// CreatedOn set automatically when added
var user = new User { Name = "John", Email = "john@example.com" };
await repository.AddAsync(ct, user);
// user.CreatedOn == DateTime.UtcNow

// UpdatedOn set automatically when modified
user.Name = "John Updated";
await repository.UpdateAsync(ct, user);
// user.UpdatedOn == DateTime.UtcNow

// DeletedOn and Status set automatically when deleted (soft delete)
// Note: This triggers on EntityState.Deleted, not ExecuteDeleteAsync
```

---

## 🔧 Value Converters

### JsonDocument Converter

```csharp
using System.Entities.Data.Converters;
using System.Text.Json;

public class AuditLog : IEntity
{
    public Guid Id { get; set; }
    public JsonDocument Data { get; set; } = null!;
    
    // IEntity properties...
}

// In DataContext.OnModelCreating
modelBuilder.Entity<AuditLog>(entity =>
{
    entity.Property(e => e.Data)
        .HasJsonDocumentConversion()
        .HasJsonDocumentComparer();
});
```

### ReadOnlyMemory<byte> Converter

```csharp
using System.Entities.Data.Converters;

public class BinaryData : IEntity
{
    public Guid Id { get; set; }
    public ReadOnlyMemory<byte> Content { get; set; }
    
    // IEntity properties...
}

// In DataContext.OnModelCreating
modelBuilder.Entity<BinaryData>(entity =>
{
    entity.Property(e => e.Content)
        .HasReadOnlyMemoryToByteArrayConversion();
});
```

---

## 📊 Extension Methods Summary

### IServiceCollection Extensions

| Method | Description |
|--------|-------------|
| `AddXDataContext<T>()` | Registers DataContext with DbContextOptions |
| `AddXDataContextFactory<T>()` | Registers IDbContextFactory for factory pattern |
| `AddXEntityFrameworkRepositories()` | Registers base IRepository and IUnitOfWork |
| `AddXEntityFrameworkRepositories<T>()` | Registers typed IRepository<T> and IUnitOfWork<T> |

### PropertyBuilder Extensions

| Method | Description |
|--------|-------------|
| `HasJsonDocumentConversion()` | Configures JsonDocument value converter |
| `HasJsonDocumentComparer()` | Configures JsonDocument value comparer |
| `HasReadOnlyMemoryToByteArrayConversion()` | Configures ReadOnlyMemory<byte> converter |

---

## ✅ Best Practices

### ✅ Do

- **Inherit from `DataContext`** — Get automatic entity lifecycle tracking
- **Implement `IEntity`** — Enable automatic `CreatedOn`/`UpdatedOn`/`DeletedOn`
- **Use `FetchAsync` with projections** — Select only needed columns with DTOs
- **Set `IsUnitOfWorkEnabled = false`** — For immediate SQL execution (bulk operations)
- **Use `EntityUpdater`** — For efficient bulk updates via `ExecuteUpdate`
- **Use `IUnitOfWork` for transactions** — Coordinate multiple operations

### ❌ Don't

- **Mix repository and direct DbContext access** — Choose one approach
- **Forget `IsUnitOfWorkEnabled`** — Default is `true`, requiring explicit save
- **Use `ExecuteUpdate`/`ExecuteDelete` with tracking** — These bypass change tracking

---

## ⚡ Performance Tips

```csharp
// FetchAsync applies AsNoTracking by default for read-only queries
var users = await repository
    .FetchAsync<User, User>(q => q.Where(u => u.IsActive))
    .ToListAsync(ct);

// Projections reduce data transfer
var names = await repository
    .FetchAsync<User, string>(q => q
        .Where(u => u.IsActive)
        .Select(u => u.Name))
    .ToListAsync(ct);

// EntityUpdater with IsUnitOfWorkEnabled = false for single SQL statement
repository.IsUnitOfWorkEnabled = false;
var updater = EntityUpdater<User>
    .Create()
    .SetProperty(u => u.LastUpdated, DateTime.UtcNow);

await repository.UpdateAsync(
    q => q.Where(u => u.IsActive),
    updater,
    ct);
// Single UPDATE statement, no entity loading
```

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **System.Entities** | Core entity abstractions (`IEntity`, `IRepository`, `IUnitOfWork`) |
| **System.Events.Data** | Event Store and Outbox EF Core implementations |
| **Microsoft.EntityFrameworkCore** | EF Core framework |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).

