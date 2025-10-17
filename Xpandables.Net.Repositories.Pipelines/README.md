# ?? Xpandables.Net.Repositories.Pipelines

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Repository Pipeline Decorators** - Pipeline decorators for repository operations including caching, logging, and transaction management.

---

## ?? Overview

Provides pipeline decorators for repository operations, enabling cross-cutting concerns like caching, logging, and performance monitoring without modifying repository implementations.

### ? Key Features

- ?? **Decorator Pattern** - Non-invasive enhancements
- ?? **Caching** - Automatic query caching
- ?? **Logging** - Operation logging
- ? **Performance** - Monitoring and optimization

---

## ?? Quick Start

```csharp
services.AddXRepository<AppDbContext>()
    .AddRepositoryCaching()
    .AddRepositoryLogging()
    .AddRepositoryPerformanceMonitoring();
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
