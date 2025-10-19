# 🔄 Xpandables.Net.Async

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Async Utilities & Pagination** - Powerful extensions for `IAsyncPagedEnumerable<T>` with built-in pagination support for efficient data streaming.

---

## 📋 Overview

Comprehensive async enumerable extensions providing LINQ-like operations, pagination, grouping, and transformations for asynchronous data streams with full support for `IAsyncPagedEnumerable<T>`.

### 🎯 Key Features

- 🔄 **Async LINQ** - Full async enumerable support with `*Paged` methods
- 📄 **Pagination** - Built-in paging with `IAsyncPagedEnumerable<T>`
- ⚡ **Streaming** - Memory-efficient data processing
- 📦 **Type-Safe** - Strongly-typed operations
- 🎨 **Rich Extensions** - Filter, Project, Group, Order, and Aggregate

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package Xpandables.Net.Async
```

### Basic Usage

```csharp
using Xpandables.Net.Async;

// Convert IQueryable to IAsyncPagedEnumerable
IAsyncPagedEnumerable<User> users = dbContext.Users
    .Where(u => u.IsActive)
    .OrderBy(u => u.Name)
    .Skip(0)
    .Take(20)
    .ToAsyncPagedEnumerable(); // Automatically extracts pagination from query

// Enumerate with cancellation
await foreach (var user in users.WithCancellation(cancellationToken))
{
    Console.WriteLine(user.Name);
}

// Get pagination metadata
Pagination pagination = await users.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
Console.WriteLine($"Total: {pagination.TotalCount} items");
```

---

## 💎 Core Operations

### 1. Filtering Operations

```csharp
using Xpandables.Net.Async;

// Simple filter
var activeUsers = users.WherePaged(u => u.IsActive);

// Filter with index
var firstTenActive = users.WherePaged((u, index) => u.IsActive && index < 10);

// Take operations
var firstFive = users.TakePaged(5);
var withoutFirst10 = users.SkipPaged(10);

// Conditional take/skip
var whileTrue = users.TakeWhilePaged(u => u.Age < 30);
var afterCondition = users.SkipWhilePaged(u => u.Age < 18);

// Last operations
var lastThree = users.TakeLastPaged(3);
var withoutLast5 = users.SkipLastPaged(5);

// Distinct
var uniqueUsers = users.DistinctPaged();
var uniqueByEmail = users.DistinctByPaged(u => u.Email);
```

### 2. Projection Operations

```csharp
using Xpandables.Net.Async;

// Synchronous projection
var userDtos = users.SelectPaged(u => new UserDto
{
    Id = u.Id,
    FullName = $"{u.FirstName} {u.LastName}",
    Email = u.Email
});

// Asynchronous projection
var enrichedUsers = users.SelectPagedAsync(async u =>
{
    var profile = await _profileService.GetProfileAsync(u.Id);
    return new EnrichedUser(u, profile);
});

// With cancellation token
var processedUsers = users.SelectPagedAsync(async (u, ct) =>
{
    var data = await _externalService.FetchDataAsync(u.Id, ct);
    return new ProcessedUser(u, data);
});

// Flattening (SelectMany)
var userOrders = users.SelectManyPaged(u => u.Orders);

// With result selector
var orderDetails = users.SelectManyPaged(
    u => u.Orders,
    (user, order) => new OrderDetail
    {
        UserName = user.Name,
        OrderId = order.Id,
        Total = order.Total
    });

// Async flattening
var asyncOrders = users.SelectManyPagedAsync(async u =>
{
    var orders = await _orderService.GetOrdersAsync(u.Id);
    return orders.ToAsync();
});
```

### 3. Ordering Operations

```csharp
using Xpandables.Net.Async;

// Order by ascending
var byName = users.OrderByPaged(u => u.Name);
var byNameWithComparer = users.OrderByPaged(
    u => u.Name, 
    StringComparer.OrdinalIgnoreCase);

// Order by descending
var byAgeDescending = users.OrderByDescendingPaged(u => u.Age);

// Reverse order
var reversed = users.ReversePaged();
```

### 4. Grouping Operations

```csharp
using Xpandables.Net.Async;

// Simple grouping
IAsyncPagedEnumerable<IGrouping<string, User>> usersByCountry = 
    users.GroupByPaged(u => u.Country);

await foreach (var group in usersByCountry)
{
    Console.WriteLine($"Country: {group.Key}, Count: {group.Count()}");
}

// Group with element selector
var groupedEmails = users.GroupByPaged(
    u => u.Country,
    u => u.Email);

// Group with result selector
var countrySummary = users.GroupByPaged(
    u => u.Country,
    (country, userList) => new CountrySummary
    {
        Country = country,
        Count = userList.Count(),
        AverageAge = userList.Average(u => u.Age)
    });

// ToLookup for in-memory grouping
ILookup<string, User> userLookup = await users
    .ToLookupPagedAsync(u => u.Country);

var usUsers = userLookup["US"];
```

### 5. Aggregation Operations

```csharp
using Xpandables.Net.Async;

// Count operations
int totalUsers = await users.CountPagedAsync();
int activeCount = await users.CountPagedAsync(u => u.IsActive);
long longCount = await users.LongCountPagedAsync();

// Existence checks
bool hasAny = await users.AnyPagedAsync();
bool hasAdults = await users.AnyPagedAsync(u => u.Age >= 18);
bool allActive = await users.AllPagedAsync(u => u.IsActive);
bool containsUser = await users.ContainsPagedAsync(specificUser);

// Min/Max operations
User youngest = await users.MinByPagedAsync(u => u.Age);
User oldest = await users.MaxByPagedAsync(u => u.Age);
int minAge = await users.MinPagedAsync(u => u.Age);
int maxAge = await users.MaxPagedAsync(u => u.Age);

// Aggregate operation
decimal totalSalary = await users.AggregatePagedAsync(
    seed: 0m,
    func: (sum, user) => sum + user.Salary);

// With result selector
var summary = await users.AggregatePagedAsync(
    seed: new { Count = 0, TotalAge = 0 },
    func: (acc, user) => new { Count = acc.Count + 1, TotalAge = acc.TotalAge + user.Age },
    resultSelector: acc => new { Average = acc.TotalAge / (double)acc.Count });
```

### 6. Numerical Operations

```csharp
using Xpandables.Net.Async;

// Sum operations
int totalAge = await users.SumPagedAsync(u => u.Age);
decimal totalSalary = await users.SumPagedAsync(u => u.Salary);
int? nullableSum = await users.SumPagedAsync(u => u.OptionalValue);

// Average operations
double avgAge = await users.AveragePagedAsync(u => u.Age);
decimal avgSalary = await users.AveragePagedAsync(u => u.Salary);
double? nullableAvg = await users.AveragePagedAsync(u => u.OptionalValue);
```

### 7. Collection Operations

```csharp
using Xpandables.Net.Async;

// Chunk into batches
IAsyncPagedEnumerable<User[]> batches = users.ChunkPaged(100);

await foreach (var batch in batches)
{
    await ProcessBatchAsync(batch);
}

// Convert to collections
List<User> userList = await users.ToListPagedAsync();
User[] userArray = await users.ToArrayPagedAsync();
```

---

## 🔧 Creating IAsyncPagedEnumerable

### From IQueryable (Recommended for EF Core)

```csharp
using Xpandables.Net.Async;

// Automatic pagination extraction
IAsyncPagedEnumerable<Product> products = dbContext.Products
    .Where(p => p.IsAvailable)
    .Skip(20)
    .Take(10)
    .ToAsyncPagedEnumerable(); // Extracts Skip/Take and computes total count

// With custom total count factory
var complexProducts = dbContext.Products
    .Include(p => p.Category)
    .Skip(0)
    .Take(50)
    .ToAsyncPagedEnumerable(
        totalFactory: async ct =>
        {
            return await cacheService.GetOrSetAsync(
                "products:total",
                () => dbContext.Products.LongCountAsync(ct));
        });
```

### From IAsyncEnumerable

```csharp
using Xpandables.Net.Async;

// With pagination factory
IAsyncPagedEnumerable<LogEntry> logs = logStream.ToAsyncPagedEnumerable(
    paginationFactory: async ct =>
    {
        int total = await CountLogsAsync(ct);
        return Pagination.Create(
            pageSize: 100,
            currentPage: 1,
            totalCount: total);
    });

// With explicit pagination
var pagedData = dataStream.ToAsyncPagedEnumerable(
    pagination: Pagination.Create(
        pageSize: 50,
        currentPage: 2,
        totalCount: 500));

// With just total count
var simplePagedData = dataStream.ToAsyncPagedEnumerable(totalCount: 1000);
```

---

## 📊 Pagination Strategies

### Per-Page Strategy

```csharp
using Xpandables.Net.Async;

var paged = dataStream.ToAsyncPagedEnumerable(
    paginationFactory: ct => ValueTask.FromResult(
        Pagination.Create(pageSize: 20, currentPage: 1, totalCount: 200)));

var enumerator = (IAsyncPagedEnumerator<Item>)paged.GetAsyncEnumerator();
enumerator.WithPerPageStrategy();

await foreach (var item in paged)
{
    // Pagination.CurrentPage increments every 20 items
    var currentPagination = enumerator.Pagination;
    Console.WriteLine($"Page: {currentPagination.CurrentPage}");
}
```

### Per-Item Strategy

```csharp
using Xpandables.Net.Async;

var paged = dataStream.ToAsyncPagedEnumerable(
    paginationFactory: ct => ValueTask.FromResult(
        Pagination.Create(pageSize: 1, currentPage: 1, totalCount: null)));

var enumerator = (IAsyncPagedEnumerator<Item>)paged.GetAsyncEnumerator();
enumerator.WithPerItemStrategy();

await foreach (var item in paged)
{
    // Pagination.CurrentPage increments with each item
    var currentPagination = enumerator.Pagination;
    Console.WriteLine($"Item: {currentPagination.CurrentPage}");
}
```

---

## 💡 Real-World Example: User Management

```csharp
using Xpandables.Net.Async;

public class UserService
{
    private readonly AppDbContext _dbContext;
    
    public async Task<(List<UserDto> Users, Pagination Pagination)> SearchUsersAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsQueryable();
        
        // Apply search filter
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => 
                u.Name.Contains(searchTerm) || 
                u.Email.Contains(searchTerm));
        }
        
        // Apply pagination and convert to paged enumerable
        IAsyncPagedEnumerable<User> pagedUsers = query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToAsyncPagedEnumerable();
        
        // Project to DTO
        var userDtos = pagedUsers.SelectPaged(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email
        });
        
        // Materialize and get pagination
        var users = await userDtos.ToListPagedAsync(cancellationToken);
        var pagination = await pagedUsers.GetPaginationAsync(cancellationToken);
        
        return (users, pagination);
    }
    
    public async Task<Dictionary<string, int>> GetUserCountByCountryAsync(
        CancellationToken cancellationToken)
    {
        IAsyncPagedEnumerable<User> allUsers = _dbContext.Users
            .ToAsyncPagedEnumerable();
        
        var grouped = allUsers.GroupByPaged(
            u => u.Country,
            (country, users) => new { Country = country, Count = users.Count() });
        
        var result = new Dictionary<string, int>();
        await foreach (var group in grouped.WithCancellation(cancellationToken))
        {
            result[group.Country] = group.Count;
        }
        
        return result;
    }
}
```

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
