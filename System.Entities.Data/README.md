# ??? System.Entities.Data

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Entity Framework Core Repository** - Production-ready implementation of the repository pattern using EF Core with DataContext, automatic entity tracking, and bulk operations support.

---

## ?? Overview

`System.Entities.Data` provides a complete Entity Framework Core implementation of the repository pattern with `DataContext` base class, automatic entity lifecycle tracking (CreatedOn, UpdatedOn, DeletedOn), and support for bulk operations through `EntityUpdater`.

### ? Key Features

- ?? **DataContext** - Extended DbContext with automatic entity tracking
- ?? **Repository<TDataContext>** - Generic EF Core repository implementation
- ? **Bulk Operations** - Efficient batch updates with EntityUpdater
- ?? **Entity Lifecycle** - Automatic CreatedOn/UpdatedOn/DeletedOn timestamps
- ?? **LINQ Support** - Full IQueryable support with async enumeration
- ?? **Unit of Work** - Optional unit of work mode for batched operations
- ? **Type Safe** - Strongly typed with DynamicallyAccessedMembers attributes

---

## ?? Quick Start

### Installation

```bash
dotnet add package System.Data.EntityFramework
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### Basic Setup

```csharp
using System.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Define your DbContext (inherits from DataContext)
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
        
        // Configure entities
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

// Define entity (implements IEntity for automatic tracking)
public class User : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Automatic tracking properties
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public EntityStatus Status { get; set; }
}

// Register services
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repository
builder.Services.AddScoped<IRepository<AppDataContext>, Repository<AppDataContext>>();

// Usage
public class UserService
{
    private readonly IRepository<AppDataContext> _repository;

    public UserService(IRepository<AppDataContext> repository) 
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
// Simple query with AsNoTracking for read-only operations
var users = await _repository
    .FetchAsync<User, User>(q => q
        .Where(u => u.IsActive)
        .AsNoTracking())
    .ToListAsync();

// Projection to DTO
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

// Join queries with Include
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

// Async enumeration for large datasets
await foreach (var user in _repository
    .FetchAsync<User, User>(q => q.Where(u => u.IsActive)))
{
    await ProcessUserAsync(user);
}
```

### Insert Operations

```csharp
// Single insert (CreatedOn is set automatically)
var user = new User 
{ 
    Id = Guid.NewGuid(),
    Name = "John Doe", 
    Email = "john@example.com",
    IsActive = true
};
await _repository.AddAsync(cancellationToken, user);

// Bulk insert
var users = new[]
{
    new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" },
    new User { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@example.com" },
    new User { Id = Guid.NewGuid(), Name = "Charlie", Email = "charlie@example.com" }
};
await _repository.AddAsync(cancellationToken, users);
```

### Update Operations

#### Update by Entity (UpdatedOn is set automatically)

```csharp
// Load and update
var user = await _repository
    .FetchAsync<User, User>(q => q.Where(u => u.Id == userId))
    .FirstOrDefaultAsync();

if (user != null)
{
    user.Name = "Updated Name";
    // UpdatedOn will be set automatically to DateTime.UtcNow
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
        Status = EntityStatus.INACTIVE,
        // Note: Bulk updates bypass automatic tracking
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
        // UpdatedOn will be set automatically
    });
```

#### Bulk Update with EntityUpdater (Fluent API)

```csharp
var updater = EntityUpdater<User>
    .Create()
    .SetProperty(u => u.IsActive, true)
    .SetProperty(u => u.LoginCount, u => u.LoginCount + 1);

// With Unit of Work disabled - executes immediately as SQL UPDATE
_repository.IsUnitOfWorkEnabled = false;
await _repository.UpdateAsync(
    q => q.Where(u => u.Email.EndsWith("@example.com")),
    updater);

// With Unit of Work enabled - tracks changes for batch commit
_repository.IsUnitOfWorkEnabled = true;
await _repository.UpdateAsync(
    q => q.Where(u => u.Email.EndsWith("@example.com")),
    updater);
// Later: await context.SaveChangesAsync();
```

### Delete Operations

```csharp
// Soft delete - sets DeletedOn and Status to DELETED
await _repository.DeleteAsync<User>(
    q => q.Where(u => !u.IsActive && u.CreatedOn < oldDate));

// With complex conditions
await _repository.DeleteAsync<Order>(
    q => q
        .Where(o => o.Status == OrderStatus.Cancelled)
        .Where(o => o.CreatedOn < DateTime.UtcNow.AddMonths(-6)));
```

---

## ?? Unit of Work Pattern

```csharp
// Enable unit of work mode - changes are tracked but not saved
_repository.IsUnitOfWorkEnabled = true;

try
{
    // All operations are batched
    await _repository.AddAsync(cancellationToken, user);
    
    await _repository.UpdateAsync<Order>(
        q => q.Where(o => o.UserId == user.Id),
        order => order.Status = OrderStatus.Active);

    await _repository.DeleteAsync<TempData>(
        q => q.Where(t => t.IsExpired));

    // Must manually save changes when unit of work is enabled
    // Access the underlying DbContext to save
    // Note: Repository doesn't expose SaveChanges directly
    // Use IUnitOfWork pattern from System.Data.Repositories instead
}
finally
{
    _repository.IsUnitOfWorkEnabled = false;
}
```

---

## ?? DataContext Automatic Tracking

The `DataContext` base class automatically tracks entity lifecycle:

```csharp
public class DataContext : DbContext
{
    protected DataContext(DbContextOptions options) : base(options)
    {
        // Automatically tracks entity changes
        ChangeTracker.Tracked += OnEntityTracked;
        ChangeTracker.StateChanged += OnEntityStateChanged;
    }
    
    // When entity is added
    private static void OnEntityTracked(EntityTrackedEventArgs e)
    {
        if (e is { FromQuery: false, Entry: { State: EntityState.Added, Entity: IEntity entity } })
        {
            entity.CreatedOn = DateTime.UtcNow;
        }
    }
    
    // When entity is modified
    private static void OnEntityStateChanged(EntityStateChangedEventArgs e)
    {
        if (e is { NewState: EntityState.Modified, Entry.Entity: IEntity entity })
        {
            entity.UpdatedOn = DateTime.UtcNow;
        }
        
        // Soft delete
        if (e is { NewState: EntityState.Deleted, Entry.Entity: IEntity deletedEntity })
        {
            deletedEntity.DeletedOn = DateTime.UtcNow;
            deletedEntity.Status = EntityStatus.DELETED;
        }
    }
}
```

---

## ? Best Practices

1. **Inherit from DataContext** - Get automatic entity tracking
2. **Implement IEntity** - Enable automatic CreatedOn/UpdatedOn/DeletedOn
3. **Use AsNoTracking** - For read-only queries to improve performance
4. **Disable Unit of Work** - For immediate SQL execution (bulk operations)
5. **Enable Unit of Work** - For batched operations with explicit save
6. **Use EntityUpdater** - For efficient bulk updates
7. **Use projections** - Select only needed columns with DTOs

---

## ? Performance Tips

```csharp
// AsNoTracking for read-only queries
var users = await _repository
    .FetchAsync<User, User>(q => q
        .AsNoTracking()
        .Where(u => u.IsActive))
    .ToListAsync();

// Projections reduce data transfer
var userNames = await _repository
    .FetchAsync<User, string>(q => q
        .Where(u => u.IsActive)
        .Select(u => u.Name))
    .ToListAsync();

// Bulk operations with EntityUpdater
_repository.IsUnitOfWorkEnabled = false; // Execute immediately
var updater = EntityUpdater<User>
    .Create()
    .SetProperty(u => u.LastUpdated, DateTime.UtcNow);

await _repository.UpdateAsync(
    q => q.Where(u => u.IsActive),
    updater);  // Single SQL UPDATE statement
```

---

## ?? Related Packages

- **System.Data.Repositories** - Repository abstractions (IRepository, IUnitOfWork)
- **System.Events.EntityFramework** - Event Store and Outbox implementations
- **Microsoft.EntityFrameworkCore** - EF Core framework

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
