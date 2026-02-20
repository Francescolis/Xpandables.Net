# 📡 System.AsyncPaged.Json

[![NuGet Version](https://img.shields.io/nuget/v/Xpandables.AsyncPaged.Json.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Json)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.AsyncPaged.Json.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Json)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **High-Performance JSON Serialization for Async Pagination** — Efficient, streaming JSON serialization and deserialization for `IAsyncPagedEnumerable<T>` with source generation, adaptive flushing, and minimal allocations.

---

## 🎯 Overview

`Xpandables.AsyncPaged.Json` provides native `System.Text.Json` integration for serializing and deserializing `IAsyncPagedEnumerable<T>` sequences with pagination metadata. It enables efficient handling of large JSON payloads through streaming serialization, memory-aware buffer management, and full cancellation support.

Built for .NET 10 with AOT compatibility, this library seamlessly integrates pagination metadata into JSON output while maintaining high throughput and minimal memory overhead.

### ✨ Key Features

-  **Streaming Serialization** — Asynchronously serialize large `IAsyncPagedEnumerable<T>` to JSON with adaptive flushing
-  **Streaming Deserialization** — Efficiently deserialize JSON into `IAsyncPagedEnumerable<T>` with pagination metadata
-  **Pagination Metadata Preservation** — Automatically includes pagination information in serialized output
-  **Adaptive Buffer Management** — Memory-aware flushing strategy based on dataset size and buffer pressure
-  **Source Generation Ready** — Compatible with source-generated JSON serialization for AOT
-  **PipeWriter & Stream Support** — Works with both System.IO.Pipelines and Stream-based I/O
-  **Full Cancellation Support** — All operations support `CancellationToken` for graceful shutdown
-  **High Performance** — Optimized for minimal allocations and maximum throughput
-  **Type-Safe** — Generic and non-generic overloads for maximum flexibility

---

## 📦 Installation

```bash
dotnet add package Xpandables.AsyncPaged.Json
```

Or via NuGet Package Manager:

```powershell
Install-Package Xpandables.AsyncPaged.Json
```

### Prerequisites

- `Xpandables.AsyncPaged` (automatically installed as a dependency)
- .NET 10.0 or later

---

## 🚀 Quick Start

### 📤 Serializing Paged Data to JSON

```csharp
using System.Text.Json;
using System.IO.Pipelines;

// Get paged data
IAsyncPagedEnumerable<Product> products = GetProductsAsync();

// Serialize to stream
using var stream = new FileStream("products.json", FileMode.Create);
var options = new JsonSerializerOptions { WriteIndented = true };

await JsonSerializer.SerializeAsyncPaged(
    stream,
    products,
    options,
    cancellationToken: CancellationToken.None);
```

### 📥 Deserializing JSON to Paged Enumerable

```csharp
using System.Text.Json;

// Deserialize from stream
using var stream = new FileStream("products.json", FileMode.Open);
var options = new JsonSerializerOptions();

IAsyncPagedEnumerable<Product> products = JsonSerializer.DeserializeAsyncPagedEnumerable<Product>(
    stream,
    options,
    PaginationStrategy.None);

// Enumerate with pagination
await foreach (var product in products)
{
    Console.WriteLine(product.Name);
}

var pagination = await products.GetPaginationAsync();
Console.WriteLine($"Total: {pagination.TotalCount}");
```

### 🔄 Round-Trip Serialization

```csharp
// Original paged enumerable
var original = _context.Products
    .Where(p => p.IsActive)
    .ToAsyncPagedEnumerable();

// Serialize to JSON
using var stream = new MemoryStream();
var options = JsonSerializerOptions.Default;

await JsonSerializer.SerializeAsyncPaged(stream, original, options);

// Deserialize back
stream.Seek(0, SeekOrigin.Begin);
var restored = JsonSerializer.DeserializeAsyncPagedEnumerable<Product>(
    stream,
    options,
    PaginationStrategy.None);

// Both have the same pagination metadata
var originalPagination = await original.GetPaginationAsync();
var restoredPagination = await restored.GetPaginationAsync();
```

---

## 📚 Core Concepts

### 📋 JSON Structure

When serialized, `IAsyncPagedEnumerable<T>` produces the following JSON structure:

```json
{
  "pagination": {
    "totalCount": 500,
    "pageSize": 20,
    "currentPage": 1,
    "continuationToken": null
  },
  "items": [
    { "id": 1, "name": "Product A" },
    { "id": 2, "name": "Product B" },
    ....
  ]
}
```

The `pagination` object contains all metadata needed to reconstruct page navigation, while `items` contains the serialized enumerable elements.

### 🔄 Serialization Modes

**Stream-Based:**
```csharp
// Serialize to Stream
await JsonSerializer.SerializeAsyncPaged(
    utf8Stream,
    pagedEnumerable,
    options);
```

**PipeWriter-Based (System.IO.Pipelines):**
```csharp
// Serialize to PipeWriter for high-performance scenarios
PipeWriter writer = PipeWriter.Create(stream);
await JsonSerializer.SerializeAsyncPaged(
    writer,
    pagedEnumerable,
    options);
```

### 📥 Deserialization Modes

**From Stream:**
```csharp
IAsyncPagedEnumerable<T> result = JsonSerializer.DeserializeAsyncPagedEnumerable<T>(
    stream,
    options,
    PaginationStrategy.None);
```

**From PipeReader:**
```csharp
PipeReader reader = PipeReader.Create(stream);
IAsyncPagedEnumerable<T> result = JsonSerializer.DeserializeAsyncPagedEnumerable<T>(
    reader,
    options,
    PaginationStrategy.None);
```

### 🧠 Adaptive Flushing Strategy

The serializer uses an intelligent flushing strategy that adapts based on dataset size:

| Dataset Size | Batch Size | Behavior |
|-------------|-----------|----------|
| Unknown | 100 items | Default balanced approach |
| < 1,000 items | 200 items | Less frequent flushes for small datasets |
| 1,000 - 10,000 items | 100 items | Standard balanced flushing |
| 10,000 - 100,000 items | 50 items | More frequent for memory management |
| > 100,000 items | 25 items | Aggressive flushing for very large datasets |

Additionally, flushing occurs whenever the pending bytes exceed 32 KB, ensuring memory pressure is managed.

---

## 💡 Common Patterns

### 📊 REST API Response with Pagination

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task GetProductsAsync(int pageNumber = 1, int pageSize = 20)
    {
        var products = _context.Products
            .Where(p => p.IsActive)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToAsyncPagedEnumerable();

        Response.ContentType = "application/json";
        await JsonSerializer.SerializeAsyncPaged(
            Response.Body,
            products,
            _jsonOptions);
    }
}
```

### 💾 Large Dataset Export

```csharp
public async Task ExportProductsAsync(string filePath, CancellationToken cancellationToken)
{
    var products = GetAllProductsAsync();

    using var stream = new FileStream(
        filePath,
        FileMode.Create,
        FileAccess.Write,
        FileShare.None,
        bufferSize: 65536,
        useAsync: true);

    var options = new JsonSerializerOptions { WriteIndented = false };

    await JsonSerializer.SerializeAsyncPaged(
        stream,
        products,
        options,
        cancellationToken);

    Console.WriteLine($"Exported {products.GetPaginationAsync().Result.TotalCount} products");
}
```

### 📦 Import with Validation

```csharp
public async Task ImportProductsAsync(string filePath, CancellationToken cancellationToken)
{
    using var stream = new FileStream(filePath, FileMode.Open);
    var options = new JsonSerializerOptions();

    var products = JsonSerializer.DeserializeAsyncPagedEnumerable<Product>(
        stream,
        options,
        PaginationStrategy.None,
        cancellationToken);

    await foreach (var product in products.WithCancellation(cancellationToken))
    {
        if (product is not null)
        {
            // Validate each product
            if (await ValidateProductAsync(product, cancellationToken))
            {
                await _repository.AddAsync(product, cancellationToken);
            }
        }
    }

    var pagination = await products.GetPaginationAsync(cancellationToken);
    Console.WriteLine($"Imported {pagination.TotalCount} products");
}
```

### 🔄 Data Transformation Pipeline

```csharp
public async Task TransformAndExportAsync(
    string inputPath,
    string outputPath,
    CancellationToken cancellationToken)
{
    // Deserialize
    using var inputStream = new FileStream(inputPath, FileMode.Open);
    var inputOptions = new JsonSerializerOptions();
    var input = JsonSerializer.DeserializeAsyncPagedEnumerable<ProductDto>(
        inputStream,
        inputOptions,
        PaginationStrategy.None,
        cancellationToken);

    // Transform using LINQ
    var transformed = input.SelectPagedAsync(async product =>
    {
        if (product is not null)
        {
            product.Price *= 1.1m; // 10% markup
            product.LastUpdated = DateTime.UtcNow;
        }
        return product;
    });

    // Serialize
    using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, useAsync: true);
    var outputOptions = new JsonSerializerOptions { WriteIndented = false };

    await JsonSerializer.SerializeAsyncPaged(
        outputStream,
        transformed,
        outputOptions,
        cancellationToken);
}
```

### 🌐 HTTP Response Streaming

```csharp
public async Task<IAsyncResult> GetProductsStreamAsync(int pageNumber = 1, int pageSize = 20)
{
    var products = _context.Products
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToAsyncPagedEnumerable();

    return new StreamingResult(async (outputStream, cancellationToken) =>
    {
        var options = JsonSerializerOptions.Default;
        await JsonSerializer.SerializeAsyncPaged(
            outputStream,
            products,
            options,
            cancellationToken);
    });
}
```

---

## ⚙️ Performance Benchmarks

The following benchmarks compare `System.AsyncPaged.Json` serialization/deserialization with standard `IAsyncEnumerable<T>` approaches:

### Serialization Performance (Write to JSON)

| Scenario | Dataset | Custom AsyncPaged | Framework IAsyncEnumerable | Improvement |
|----------|---------|------------------:|---------------------------:|------------:|
| Small    | 100     | **109 μs**         | 121 μs                     | **+10%**     |
| Medium   | 1,000   | **1,290 μs**       | 1,445 μs                   | **+11%**     |
| Large    | 10,000  | **8,732 μs**       | 9,975 μs                   | **+12%**     |

### Deserialization Performance (Read from JSON)

| Scenario | Dataset | Custom AsyncPaged | Framework IAsyncEnumerable | Improvement |
|----------|---------|------------------:|---------------------------:|------------:|
| Small    | 100     | **80 μs**          | 122 μs                     | **+34%**     |
| Medium   | 1,000   | **460 μs**         | 1,174 μs                   | **+61%**     |
| Large    | 10,000  | **4,310 μs**       | 11,759 μs                  | **+63%**     |

### Memory Allocation

| Scenario | Dataset | Custom AsyncPaged | Framework IAsyncEnumerable | Reduction |
|----------|---------|------------------:|---------------------------:|----------:|
| Small    | 100     | **24.23 KB**       | 60.80 KB                   | **−60%**   |
| Medium   | 1,000   | **25.45 KB**       | 574.08 KB                  | **−96%**   |
| Large    | 10,000  | **37.70 KB**       | 5,706.89 KB                | **−99%**   |

*Memory figures are for deserialization (read from JSON), where the difference is most significant.*

### Raw BenchmarkDotNet Results

<details>
<summary>Click to expand full benchmark results</summary>

| Method                                                       | ItemCount | Mean         | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0     | Gen1     | Gen2     | Allocated  | Alloc Ratio |
|------------------------------------------------------------- |---------- |-------------:|----------:|----------:|------:|--------:|-----:|---------:|---------:|---------:|-----------:|------------:|
| 'Framework IAsyncEnumerable Serialization'                   | 100       |    121.26 us |  0.616 us |  0.576 us |  1.00 |    0.01 |    3 |   6.8359 |   0.4883 |        - |   74.01 KB |        1.00 |
| 'Custom IAsyncPagedEnumerable Serialization'                 | 100       |    108.53 us |  1.581 us |  1.479 us |  0.90 |    0.01 |    2 |   5.9814 |   0.6104 |        - |   72.12 KB |        0.97 |
| 'Framework IAsyncEnumerable Deserialization (HttpContent)'   | 100       |    121.96 us |  1.218 us |  1.080 us |  1.01 |    0.01 |    3 |   4.8828 |   0.2441 |        - |    60.8 KB |        0.82 |
| 'Custom IAsyncPagedEnumerable Deserialization (HttpContent)' | 100       |     80.07 us |  0.405 us |  0.379 us |  0.66 |    0.00 |    1 |   1.9531 |        - |        - |   24.23 KB |        0.33 |
|                                                              |           |              |           |           |       |         |      |          |          |          |            |             |
| 'Framework IAsyncEnumerable Serialization'                   | 1000      |  1,444.90 us | 27.481 us | 30.545 us |  1.00 |    0.03 |    4 | 117.1875 | 117.1875 | 117.1875 |  599.92 KB |        1.00 |
| 'Custom IAsyncPagedEnumerable Serialization'                 | 1000      |  1,290.02 us | 21.372 us | 19.991 us |  0.89 |    0.02 |    3 | 117.1875 | 117.1875 | 117.1875 |  616.86 KB |        1.03 |
| 'Framework IAsyncEnumerable Deserialization (HttpContent)'   | 1000      |  1,173.81 us |  7.631 us |  7.138 us |  0.81 |    0.02 |    2 |  45.8984 |   1.9531 |        - |  574.08 KB |        0.96 |
| 'Custom IAsyncPagedEnumerable Deserialization (HttpContent)' | 1000      |    460.10 us |  2.103 us |  1.967 us |  0.32 |    0.01 |    1 |   1.9531 |        - |        - |   25.45 KB |        0.04 |
|                                                              |           |              |           |           |       |         |      |          |          |          |            |             |
| 'Framework IAsyncEnumerable Serialization'                   | 10000     |  9,975.05 us | 55.857 us | 52.249 us |  1.00 |    0.01 |    3 | 718.7500 | 640.6250 | 640.6250 | 5026.28 KB |       1.000 |
| 'Custom IAsyncPagedEnumerable Serialization'                 | 10000     |  8,731.93 us | 36.779 us | 32.604 us |  0.88 |    0.01 |    2 | 601.5625 | 500.0000 | 492.1875 | 5192.73 KB |       1.033 |
| 'Framework IAsyncEnumerable Deserialization (HttpContent)'   | 10000     | 11,759.42 us | 59.528 us | 55.683 us |  1.18 |    0.01 |    4 | 460.9375 |  23.4375 |        - | 5706.89 KB |       1.135 |
| 'Custom IAsyncPagedEnumerable Deserialization (HttpContent)' | 10000     |  4,309.54 us | 17.159 us | 16.051 us |  0.43 |    0.00 |    1 |        - |        - |        - |    37.7 KB |       0.007 |

</details>

**Key Findings:**
- ⚡ **Serialization:** 10–12% faster across all dataset sizes with comparable memory usage
- ⚡ **Deserialization:** 34–63% faster, with the advantage growing for larger datasets
- 💾 **Memory Allocation:** Up to 99% reduction in deserialization memory (37.7 KB vs 5,707 KB at 10K items) — zero Gen0/Gen1/Gen2 GC pressure at scale
- 📈 **Scalability:** Performance and memory advantages increase with dataset size due to streaming architecture and adaptive flushing

---

## 📖 API Reference

### 🔤 Generic Serialization Extensions

```csharp
// Serialize to Stream with JsonTypeInfo
Task SerializeAsyncPaged<TValue>(
    Stream utf8Json,
    IAsyncPagedEnumerable<TValue> pagedEnumerable,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken = default);

// Serialize to Stream with JsonSerializerOptions
Task SerializeAsyncPaged<TValue>(
    Stream utf8Json,
    IAsyncPagedEnumerable<TValue> pagedEnumerable,
    JsonSerializerOptions options,
    CancellationToken cancellationToken = default);

// Serialize to PipeWriter with JsonTypeInfo
Task SerializeAsyncPaged<TValue>(
    PipeWriter utf8Json,
    IAsyncPagedEnumerable<TValue> pagedEnumerable,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken = default);

// Serialize to PipeWriter with JsonSerializerOptions
Task SerializeAsyncPaged<TValue>(
    PipeWriter utf8Json,
    IAsyncPagedEnumerable<TValue> pagedEnumerable,
    JsonSerializerOptions options,
    CancellationToken cancellationToken = default);
```

### 🔤 Non-Generic Serialization Extensions

```csharp
// Serialize (non-generic) to Stream with JsonTypeInfo
Task SerializeAsyncPaged(
    Stream utf8Json,
    IAsyncPagedEnumerable pagedEnumerable,
    JsonTypeInfo jsonTypeInfo,
    CancellationToken cancellationToken = default);

// Serialize to Stream with JsonSerializerContext
Task SerializeAsyncPaged(
    Stream utf8Json,
    IAsyncPagedEnumerable pagedEnumerable,
    JsonSerializerContext context,
    CancellationToken cancellationToken = default);

// Serialize to Stream with JsonSerializerOptions
Task SerializeAsyncPaged(
    Stream utf8Json,
    IAsyncPagedEnumerable pagedEnumerable,
    JsonSerializerOptions options,
    CancellationToken cancellationToken = default);

// Serialize to PipeWriter with JsonTypeInfo
Task SerializeAsyncPaged(
    PipeWriter utf8Json,
    IAsyncPagedEnumerable pagedEnumerable,
    JsonTypeInfo jsonTypeInfo,
    CancellationToken cancellationToken = default);

// Serialize to PipeWriter with JsonSerializerContext
Task SerializeAsyncPaged(
    PipeWriter utf8Json,
    IAsyncPagedEnumerable pagedEnumerable,
    JsonSerializerContext context,
    CancellationToken cancellationToken = default);

// Serialize to PipeWriter with JsonSerializerOptions
Task SerializeAsyncPaged(
    PipeWriter utf8Json,
    IAsyncPagedEnumerable pagedEnumerable,
    JsonSerializerOptions options,
    CancellationToken cancellationToken = default);
```

### 🔤 Generic Deserialization Extensions

```csharp
// Deserialize from Stream with JsonTypeInfo
IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
    Stream utf8Json,
    JsonTypeInfo<TValue> jsonTypeInfo,
    PaginationStrategy strategy = PaginationStrategy.None,
    CancellationToken cancellationToken = default);

// Deserialize from Stream with JsonSerializerOptions (returns nullable)
IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
    Stream utf8Json,
    JsonSerializerOptions options,
    PaginationStrategy strategy = PaginationStrategy.None,
    CancellationToken cancellationToken = default);

// Deserialize from PipeReader with JsonTypeInfo
IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
    PipeReader utf8Json,
    JsonTypeInfo<TValue> jsonTypeInfo,
    PaginationStrategy strategy = PaginationStrategy.None,
    CancellationToken cancellationToken = default);

// Deserialize from PipeReader with JsonSerializerOptions (returns nullable)
IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
    PipeReader utf8Json,
    JsonSerializerOptions options,
    PaginationStrategy strategy = PaginationStrategy.None,
    CancellationToken cancellationToken = default);
```

---

## ✅ Best Practices

### ✅ Do

- **Use source-generated JSON** — Leverage `JsonTypeInfo<T>` and `JsonSerializerContext` for AOT compatibility
- **Apply streaming for large datasets** — Don't materialize when serializing/deserializing
- **Handle cancellation** — Always pass `CancellationToken` through the pipeline
- **Use PipeWriter for high-throughput scenarios** — Better for server-side streaming
- **Monitor pagination metadata** — Use pagination info to handle multi-page results correctly
- **Validate after deserialization** — Check items and pagination before processing
- **Specify PaginationStrategy** — Use appropriate strategy (None, PerPage, PerItem) for deserialization based on your needs
- **Handle nullable values** — When using `JsonSerializerOptions` overload, items may be nullable

### ❌ Don't

- **Block on async operations** — Never use `.Result` or `.Wait()` on serialization tasks
- **Ignore cancellation tokens** — Support graceful shutdown and timeouts
- **Materialize entire streams into memory** — Process items as they're deserialized
- **Use default JsonSerializerOptions** — Provide explicit options for consistent behavior
- **Assume pagination is always present** — Handle cases where pagination metadata may be unknown
- **Reuse streams without resetting** — Always reset position or create new streams for new operations
- **Forget to check for null** — When using options overload, returned items can be null

---

## 🔧 Advanced Configuration

### Custom Serialization Options

```csharp
var options = new JsonSerializerOptions
{
    WriteIndented = false,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = { new JsonStringEnumConverter() }
};

await JsonSerializer.SerializeAsyncPaged(stream, products, options);
```

### Source-Generated JSON

```csharp
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(Pagination))]
public partial class AppJsonContext : JsonSerializerContext { }

// Use for serialization
var context = new AppJsonContext();
await JsonSerializer.SerializeAsyncPaged(
    stream,
    products,
    context);
```

### With Compression

```csharp
using var fileStream = new FileStream("products.json.gz", FileMode.Create);
using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);

var options = JsonSerializerOptions.Default;
await JsonSerializer.SerializeAsyncPaged(
    gzipStream,
    products,
    options);
```

---

## 🔗 Dependency

This library depends on:
- **System.AsyncPaged** — Core pagination types and interfaces

---

## 📚 Related Packages

- **[System.AsyncPaged](https://www.nuget.org/packages/System.Collections.AsyncPaged)** — Core async pagination library
- **[System.AsyncPaged.Linq](https://www.nuget.org/packages/System.Linq.AsyncPaged)** — LINQ operators for async paged enumerables
- **[System.AsyncPaged.Json](https://www.nuget.org/packages/System.Net.Http.AsyncPaged)** — REST API pagination support

---

## 📄 License & Contributing

Licensed under the **Apache License 2.0**. Copyright © Kamersoft 2025.

Contributions are welcome! Please visit [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net) to contribute, report issues, or request features.
