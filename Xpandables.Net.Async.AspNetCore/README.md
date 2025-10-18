# 🔄 Xpandables.Net.Async.AspNetCore

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Async ASP.NET Core Integration** - Extensions for `IAsyncPagedEnumerable<T>` and pagination in ASP.NET Core responses with automatic JSON serialization.

---

## 📋 Overview

Provides ASP.NET Core-specific extensions for streaming async enumerables and paginated responses, enabling efficient data transfer in web APIs with built-in pagination metadata.

### 🎯 Key Features

- 🌊 **Streaming Responses** - Stream `IAsyncPagedEnumerable<T>` to HTTP efficiently
- 📄 **Paginated APIs** - Built-in pagination support with metadata
- ⚡ **Performance** - Memory-efficient data transfer with low allocations
- 📦 **Type-Safe** - Strongly-typed responses with source generators
- 🔧 **MVC Integration** - Works with both Minimal APIs and MVC controllers
- 📝 **JSON Serialization** - Automatic JSON formatting with `System.Text.Json`

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package Xpandables.Net.Async
dotnet add package Xpandables.Net.Async.AspNetCore
```

### Configuration

```csharp
using Xpandables.Net.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// For MVC Controllers - configure pagination support
builder.Services.ConfigureIAsyncPagedEnumerableMvcOptions();

var app = builder.Build();
app.Run();
```

---

## 💎 Core Concepts

### Basic Pagination Response

```csharp
using Xpandables.Net.Async;

// Minimal API endpoint
app.MapGet("/api/users", async (IUserRepository repository) =>
{
    // Convert IQueryable to IAsyncPagedEnumerable
    IAsyncPagedEnumerable<User> users = repository
        .GetAllUsersQuery()
        .Skip(0)
        .Take(20)
        .ToAsyncPagedEnumerable(); // Automatically extracts pagination from Skip/Take
    
    // Return as streaming JSON with pagination metadata
    return users.ToResult();
});

// Response format:
// {
//   "pagination": {
//     "pageSize": 20,
//     "currentPage": 1,
//     "totalCount": 150,
//     "totalPages": 8,
//     "hasNextPage": true,
//     "hasPreviousPage": false
//   },
//   "items": [
//     { "id": 1, "name": "John Doe", "email": "john@example.com" },
//     { "id": 2, "name": "Jane Smith", "email": "jane@example.com" }
//     // ... more items
//   ]
