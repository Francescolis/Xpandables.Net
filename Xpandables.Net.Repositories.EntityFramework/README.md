# ??? Xpandables.Net.Repositories.EntityFramework

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Entity Framework Core Repository** - Production-ready implementation of the repository pattern using EF Core with support for Unit of Work, transactions, and advanced querying.

---

## ?? Overview

Provides a complete Entity Framework Core implementation of `IRepository` with support for CRUD operations, bulk updates, transactions, and the Unit of Work pattern.

### ? Key Features

- ??? **EF Core Integration** - Full DbContext support
- ?? **Unit of Work** - Transaction management
- ? **Bulk Operations** - Efficient batch operations
- ?? **LINQ Support** - Full queryable support
- ?? **Testable** - Easy to mock and test

---

## ?? Quick Start

```csharp
// Setup
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

services.AddXRepository<AppDbContext>();

// Usage
public sealed class UserService
{
    private readonly IRepository<AppDbContext> _repository;
    
    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _repository
            .FetchAsync<User, User>(q => q.Where(u => u.IsActive))
            .ToListAsync();
    }
    
    public async Task CreateUserAsync(User user)
    {
        await _repository.AddAsync<User>(default, user);
    }
}
```

---

## ?? Transactions

```csharp
public async Task TransferAsync(Guid fromId, Guid toId, decimal amount)
{
    using var transaction = await _repository.BeginTransactionAsync();
    
    try
    {
        await _repository.UpdateAsync<Account>(
            q => q.Where(a => a.Id == fromId),
            a => a.Balance -= amount);
        
        await _repository.UpdateAsync<Account>(
            q => q.Where(a => a.Id == toId),
            a => a.Balance += amount);
        
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
