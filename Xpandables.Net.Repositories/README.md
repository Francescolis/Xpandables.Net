# ?? Xpandables.Net.Repositories

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Generic Repository Pattern** - Abstract data access with a flexible, testable repository implementation supporting CRUD operations and Unit of Work.

---

## ?? Overview

Provides a generic repository pattern implementation with support for advanced querying, bulk operations, and transaction management through Unit of Work.

### ?? Key Features

- ?? **Generic Repository** - Type-safe data access
- ?? **Unit of Work** - Transaction management
- ?? **LINQ Support** - Queryable filters
- ? **Bulk Operations** - Efficient updates and deletes
- ? **Testable** - Easy mocking and testing

---

## ?? Quick Start

```csharp
public sealed class UserService
{
    private readonly IRepository _repository;
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _repository
            .FetchAsync<User, User>(q => q.Where(u => u.Email == email))
            .FirstOrDefaultAsync();
        
        return user;
    }
    
    public async Task CreateUserAsync(User user)
    {
        await _repository.AddAsync<User>(default, user);
    }
    
    public async Task UpdateUserEmailAsync(Guid userId, string newEmail)
    {
        await _repository.UpdateAsync<User>(
            filter: q => q.Where(u => u.Id == userId),
            updateAction: u => u.Email = newEmail);
    }
}
```

---

## ?? Unit of Work

```csharp
public async Task TransferFundsAsync(Guid fromId, Guid toId, decimal amount)
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
