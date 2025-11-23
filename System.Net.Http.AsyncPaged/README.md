# 🌐 System.Net.Http.AsyncPaged

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/System.Net.Http.AsyncPaged)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **REST API Pagination for HttpClient** — Seamless `IAsyncPagedEnumerable<T>` deserialization from HTTP responses with full pagination metadata support and zero memory buffering.

---

## 🎯 Overview

`System.Net.Http.AsyncPaged` provides native integration between `HttpClient` and `IAsyncPagedEnumerable<T>` for consuming paginated REST APIs. It deserializes JSON HTTP responses directly into paged enumerables while preserving pagination metadata, enabling efficient streaming of large API responses without buffering entire payloads.

Built for .NET 10 with full async support and character encoding handling, this library simplifies pagination consumption from modern REST APIs while maintaining memory efficiency and performance.

### ✨ Key Features

- 📥 **Direct HTTP Deserialization** — Deserialize `HttpContent` directly to `IAsyncPagedEnumerable<T>`
- 🌍 **REST API Pagination** — Consume paginated API responses with automatic metadata extraction
- 💾 **Zero Buffering** — Stream large responses without loading entire payloads into memory
- 🔄 **Full Pagination Metadata** — Automatic preservation of `Pagination` info from API responses
- 📝 **Character Encoding Support** — Automatic transcoding from any source encoding to UTF-8
- 🔐 **Type-Safe** — Strongly-typed generic and non-generic deserialization
- 🧵 **Full Cancellation Support** — Built-in `CancellationToken` support for all operations
- ⚡ **High Performance** — Stream-based processing with minimal allocations
- 🔗 **PipeReader Integration** — Uses System.IO.Pipelines for efficient I/O
- 🎯 **AOT Compatible** — Works with source-generated JSON serialization

---

## 📦 Installation

```bash
dotnet add package System.Net.Http.AsyncPaged
```

Or via NuGet Package Manager:

```powershell
Install-Package System.Net.Http.AsyncPaged
```

### Prerequisites

- `System.Text.Json.AsyncPaged` (automatically installed as a dependency)
- `System.Collections.AsyncPaged` (transitive dependency)
- .NET 10.0 or later

---

## 🚀 Quick Start

### 📥 Basic API Response Deserialization

```csharp
using System.Net.Http;
using System.Net.Http.Json;

// Create HTTP client
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };

// Fetch paginated endpoint
using var response = await client.GetAsync("/api/products?page=1&size=50");
response.EnsureSuccessStatusCode();

// Deserialize directly to paged enumerable
var options = new JsonSerializerOptions();
IAsyncPagedEnumerable<Product> products = response.Content
    .ReadFromJsonAsAsyncPagedEnumerable<Product>(options);

// Enumerate items asynchronously
await foreach (var product in products)
{
    Console.WriteLine($"{product.Id}: {product.Name}");
}

// Access pagination metadata
var pagination = await products.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
Console.WriteLine($"Total products: {pagination.TotalCount}");
```

### 🔄 Multi-Page API Pagination

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };

for (int page = 1; page <= totalPages; page++)
{
    // Fetch each page
    using var response = await client.GetAsync($"/api/products?page={page}&size=100");
    response.EnsureSuccessStatusCode();

    // Deserialize with type metadata for AOT compatibility
    var typeInfo = JsonSerializerContext.Default.GetTypeInfo(typeof(Product));
    IAsyncPagedEnumerable<Product> pageData = response.Content
        .ReadFromJsonAsAsyncPagedEnumerable<Product>(typeInfo);

    // Process page items
    await foreach (var product in pageData)
    {
        await ProcessProductAsync(product);
    }

    // Get pagination for next iteration
    var pagination = await pageData.GetPaginationAsync();
    totalPages = pagination.TotalPages ?? 1;
}

```

### 🔗 Continuation Token Pagination

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };

string? continuationToken = null;

while (true)
{
    // Build URL with continuation token
    var url = continuationToken != null
        ? $"/api/activities?token={Uri.EscapeDataString(continuationToken)}"
        : "/api/activities";

    using var response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();

    // Deserialize page
    var options = new JsonSerializerOptions();
    IAsyncPagedEnumerable<Activity> activities = response.Content
        .ReadFromJsonAsAsyncPagedEnumerable<Activity>(options);

    // Process activities
    await foreach (var activity in activities)
    {
        await LogActivityAsync(activity);
    }

    // Check for next page
    var pagination = await activities.GetPaginationAsync();
    if (!pagination.HasContinuation)
        break;

    continuationToken = pagination.ContinuationToken;
}
```

---

## 📚 Core Concepts

### 🏗️ Integration Points

**HttpContent → PipeReader → IAsyncPagedEnumerable:**

```
HTTP Response
     ↓
HttpContent.ReadFromJsonAsAsyncPagedEnumerable<T>()
     ↓
System.IO.Pipelines.PipeReader
     ↓
JsonDeserializer.DeserializeAsyncPagedEnumerable<T>()
     ↓
IAsyncPagedEnumerable<T>
     ↓
Pagination metadata preserved
```

### 📋 Response Format

The library expects JSON responses in the standard pagination format:

```json
{
  "pagination": {
    "totalCount": 1500,
    "pageSize": 50,
    "currentPage": 1,
    "continuationToken": "eyJpZCI6IDUwfQ=="
  },
  "items": [
    { "id": 1, "name": "Item 1" },
    { "id": 2, "name": "Item 2" },
    ...
  ]
}
```

### 🔤 Encoding Handling

The extension automatically handles character encoding:

```csharp
// Response with ISO-8859-1 encoding
// Content-Type: application/json; charset=iso-8859-1

var response = await client.GetAsync("/api/data");

// Automatically transcodes to UTF-8
IAsyncPagedEnumerable<Data> data = response.Content
    .ReadFromJsonAsAsyncPagedEnumerable<Data>(options);

// Stream is seamlessly decoded and available
```

---

## 💡 Common Patterns

### 🌐 Paginated API Client

```csharp
public class ProductApiClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductApiClient(HttpClient client)
    {
        _client = client;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async IAsyncEnumerable<Product> GetAllProductsAsync(
        int pageSize = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int page = 1;

        while (true)
        {
            // Fetch page
            var url = $"/products?page={page}&pageSize={pageSize}";
            using var response = await _client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Deserialize
            IAsyncPagedEnumerable<Product> pageData = response.Content
                .ReadFromJsonAsAsyncPagedEnumerable<Product>(_jsonOptions, cancellationToken);

            // Enumerate items
            await foreach (var item in pageData.WithCancellation(cancellationToken))
            {
                yield return item;
            }

            // Check for next page
            var pagination = await pageData.GetPaginationAsync(cancellationToken);
            if (!pagination.HasNextPage)
                break;

            page++;
        }
    }
}

// Usage
var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
var apiClient = new ProductApiClient(client);

await foreach (var product in apiClient.GetAllProductsAsync())
{
    await ProcessProductAsync(product);
}
```

### 🔍 Search Results with Pagination

```csharp
public class SearchService
{
    private readonly HttpClient _client;

    public async IAsyncEnumerable<SearchResult> SearchAsync(
        string query,
        int pageSize = 20,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var encodedQuery = Uri.EscapeDataString(query);
        var url = $"/search?q={encodedQuery}&pageSize={pageSize}";

        using var response = await _client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        IAsyncPagedEnumerable<SearchResult> results = response.Content
            .ReadFromJsonAsAsyncPagedEnumerable<SearchResult>(options, cancellationToken);

        // Stream results as they're deserialized
        await foreach (var result in results.WithCancellation(cancellationToken))
        {
            yield return result;
        }

        // Get pagination metadata
        var pagination = await results.GetPaginationAsync(cancellationToken);
        Console.WriteLine($"Found {pagination.TotalCount} results");
    }
}
```

### 📊 Aggregating Paginated Data

```csharp
public async Task<ApiStatistics> GetApiStatisticsAsync(CancellationToken cancellationToken)
{
    var stats = new ApiStatistics();

    using var response = await _httpClient.GetAsync("/api/events", cancellationToken);
    response.EnsureSuccessStatusCode();

    var options = new JsonSerializerOptions();
    IAsyncPagedEnumerable<Event> events = response.Content
        .ReadFromJsonAsAsyncPagedEnumerable<Event>(options, cancellationToken);

    // Process and aggregate
    int count = 0;
    DateTime latestTimestamp = DateTime.MinValue;

    await foreach (var evt in events.WithCancellation(cancellationToken))
    {
        count++;
        if (evt.Timestamp > latestTimestamp)
            latestTimestamp = evt.Timestamp;
    }

    var pagination = await events.GetPaginationAsync(cancellationToken);

    stats.TotalEventCount = pagination.TotalCount ?? count;
    stats.LatestEventTime = latestTimestamp;
    stats.ProcessedInPage = count;

    return stats;
}
```

### 🔐 Source-Generated JSON (AOT Compatible)

```csharp
// Define source-generated JSON context
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(Pagination))]
public partial class ApiJsonContext : JsonSerializerContext { }

// Use with extension
using var response = await client.GetAsync("/api/products");
response.EnsureSuccessStatusCode();

var typeInfo = ApiJsonContext.Default.Product;
IAsyncPagedEnumerable<Product> products = response.Content
    .ReadFromJsonAsAsyncPagedEnumerable<Product>(typeInfo);

// Works with native AOT
await foreach (var product in products)
{
    Console.WriteLine(product.Name);
}
```

### 🛡️ Error Handling & Resilience

```csharp
public async Task<IAsyncPagedEnumerable<Item>?> SafeGetItemsAsync(
    string endpoint,
    CancellationToken cancellationToken)
{
    try
    {
        using var response = await _client.GetAsync(endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"API returned {response.StatusCode}: {response.ReasonPhrase}");
            return null;
        }

        // Check content type
        if (response.Content.Headers.ContentType?.MediaType != "application/json")
        {
            _logger.LogError("Unexpected content type");
            return null;
        }

        var options = new JsonSerializerOptions();
        return response.Content
            .ReadFromJsonAsAsyncPagedEnumerable<Item>(options, cancellationToken);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP request failed");
        return null;
    }
    catch (JsonException ex)
    {
        _logger.LogError(ex, "JSON deserialization failed");
        return null;
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Operation canceled");
        throw;
    }
}
```

---

## 🎯 Advanced Examples

### 🔄 Streaming Large Exports

```csharp
public async Task ExportLargeDatasetAsync(string endpoint, string filePath)
{
    // Fetch large dataset from API
    using var response = await _client.GetAsync(endpoint);
    response.EnsureSuccessStatusCode();

    var options = new JsonSerializerOptions();
    IAsyncPagedEnumerable<DataItem> data = response.Content
        .ReadFromJsonAsAsyncPagedEnumerable<DataItem>(options);

    // Stream to file without buffering
    using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
    await JsonSerializer.SerializeAsyncPaged(fileStream, data, options);
}
```

### 📈 Monitoring API Quotas

```csharp
public class ApiQuotaMonitor
{
    public async ValueTask<ApiQuota> CheckQuotaAsync(HttpResponseMessage response)
    {
        var quota = new ApiQuota();

        // Extract quota from headers
        if (response.Headers.TryGetValues("X-RateLimit-Limit", out var limit))
            quota.Limit = int.Parse(limit.First());

        if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining))
            quota.Remaining = int.Parse(remaining.First());

        // Get pagination for data size awareness
        if (response.Content is not null)
        {
            try
            {
                var options = new JsonSerializerOptions();
                var data = response.Content
                    .ReadFromJsonAsAsyncPagedEnumerable<ApiData>(options);

                var pagination = await data.GetPaginationAsync();
                quota.ResponseSize = pagination.TotalCount ?? 0;
            }
            catch
            {
                // Ignore pagination errors
            }
        }

        return quota;
    }
}
```

### 🔗 Chaining Multiple Endpoints

```csharp
public async IAsyncEnumerable<CombinedData> FetchFromMultipleEndpointsAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var endpoints = new[] { "/api/users", "/api/products", "/api/orders" };
    var options = new JsonSerializerOptions();

    foreach (var endpoint in endpoints)
    {
        using var response = await _client.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        IAsyncPagedEnumerable<ApiItem> items = response.Content
            .ReadFromJsonAsAsyncPagedEnumerable<ApiItem>(options, cancellationToken);

        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            yield return new CombinedData { Source = endpoint, Item = item };
        }
    }
}
```

---

## ✅ Best Practices

### ✅ Do

- **Use typed extensions** — Leverage `JsonTypeInfo<T>` for AOT compatibility
- **Handle encoding properly** — Let the library manage character transcoding
- **Support cancellation throughout** — Pass `CancellationToken` in all calls
- **Check pagination metadata** — Verify `HasNextPage` before making additional requests
- **Use PipeReader** — The library uses `System.IO.Pipelines` for efficiency
- **Stream responses** — Don't materialize entire API responses into memory
- **Handle errors gracefully** — Implement proper HTTP and JSON error handling
- **Validate response format** — Ensure API responses match expected JSON structure

### ❌ Don't

- **Buffer entire responses** — Avoid calling `ReadAsStringAsync()` before deserialization
- **Block on async** — Never use `.Result` or `.Wait()` on HTTP operations
- **Ignore cancellation** — Always pass `CancellationToken` through the chain
- **Assume pagination exists** — Check response structure before assuming pagination metadata
- **Mix encoding handling** — Let the extension handle character encoding transcoding
- **Reuse disposed responses** — Always create new `HttpResponseMessage` for each request
- **Ignore rate limits** — Monitor API quotas and respect rate-limit headers
- **Deserialize without validation** — Validate deserialized objects before processing

---

## 📖 API Reference

### 🔤 Extension Methods

```csharp
// Deserialize with JsonSerializerOptions
public static IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
    this HttpContent content,
    JsonSerializerOptions options,
    CancellationToken cancellationToken = default);

// Deserialize with JsonTypeInfo (source-generated)
public static IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
    this HttpContent content,
    JsonTypeInfo<TValue> jsonTypeInfo,
    CancellationToken cancellationToken = default);
```

### 🔤 Overloads

Both overloads are available as:
- **Extension on `HttpContent`** — Use directly on response content
- **Generic `<TValue>`** — Strongly-typed deserialization with automatic type casting
- **Non-buffered** — Uses `PipeReader` for memory efficiency
- **Encoding-aware** — Automatic transcoding from response charset to UTF-8

---

## ⚙️ Performance Characteristics

- **Memory Efficiency** — Uses `PipeReader` and streaming deserialization; no full response buffering
- **Character Encoding** — Automatic transcoding with minimal overhead via `Encoding.CreateTranscodingStream`
- **I/O Pattern** — Asynchronous, non-blocking with natural async/await semantics
- **Cancellation** — Full end-to-end cancellation support for graceful shutdown
- **Large Responses** — Optimized for multi-megabyte API responses
- **Network Efficiency** — Single HTTP round-trip per call; pagination handled in-memory

---

## 🔗 Dependencies

This library depends on:
- **System.Text.Json.AsyncPaged** — JSON serialization with pagination support
- **System.Collections.AsyncPaged** — Core pagination types
- **System.Net.Http** — Standard HTTP client (built-in)

---

## 📚 Related Packages

- **[System.Collections.AsyncPaged](https://www.nuget.org/packages/System.Collections.AsyncPaged)** — Core async pagination library
- **[System.Linq.AsyncPaged](https://www.nuget.org/packages/System.Linq.AsyncPaged)** — LINQ operators for async paged enumerables
- **[System.Text.Json.AsyncPaged](https://www.nuget.org/packages/System.Text.Json.AsyncPaged)** — JSON serialization support
- **[System.Net.Http.Rests](https://www.nuget.org/packages/System.Net.Http.Rests)** — REST API client patterns

---

## 📄 License & Contributing

Licensed under the **Apache License 2.0**. Copyright © Kamersoft 2025.

Contributions are welcome! Please visit [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net) to contribute, report issues, or request features.
