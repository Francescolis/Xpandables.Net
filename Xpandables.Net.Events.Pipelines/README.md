# ?? Xpandables.Net.Events.Pipelines

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Event Pipeline Decorators** - Pipeline decorators for event handling including publishing, logging, and retry logic.

---

## ?? Overview

Provides pipeline decorators for event handling operations, enabling robust event processing with automatic retries, logging, and integration event publishing.

### ? Key Features

- ?? **Event Publishing** - Automatic event dispatch
- ?? **Retry Logic** - Resilient event processing
- ?? **Logging** - Event tracking and debugging
- ?? **Integration Events** - Cross-boundary event handling

---

## ?? Quick Start

```csharp
services.AddXEventPipeline()
    .AddEventPublishing()
    .AddEventRetry()
    .AddEventLogging();
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
