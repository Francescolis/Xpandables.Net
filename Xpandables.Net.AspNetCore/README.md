# ?? Xpandables.Net.AspNetCore

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **ASP.NET Core Integration** - Core ASP.NET Core integrations and utilities for Xpandables.Net packages.

---

## ?? Overview

Provides foundational ASP.NET Core integrations including middleware, filters, and extension methods for seamless integration of Xpandables.Net features into ASP.NET Core applications.

### ? Key Features

- ?? **Middleware** - Custom middleware components
- ?? **Filters** - Action and exception filters
- ?? **Extensions** - Service registration helpers
- ?? **Configuration** - ASP.NET Core configuration support

---

## ?? Quick Start

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Xpandables.Net services
builder.Services.AddXpandablesNet();

var app = builder.Build();
app.Run();
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
