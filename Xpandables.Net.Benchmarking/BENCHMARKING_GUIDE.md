# AsyncPaged Benchmarking Guide

This document describes the comprehensive benchmark suite for the AsyncPaged and AsyncPaged.AspNetCore projects.

## Overview

The benchmark suite has been completely rewritten to test the new methods and patterns in the AsyncPaged library. It now includes two main benchmark classes:

### 1. JsonStreamingBenchmarks.cs
Tests serialization and deserialization performance for core AsyncPaged functionality.

#### Deserialization Benchmarks

- **Baseline: IAsyncEnumerable from HttpContent** - Standard .NET deserialization using `ReadFromJsonAsAsyncEnumerable<T>`
- **IAsyncPagedEnumerable from HttpContent (array)** - Using `ReadFromJsonAsAsyncPagedEnumerable<T>` with standard JSON arrays
- **IAsyncPagedEnumerable from HttpContent (paged format)** - Deserializing from the paged JSON format: `{ "pagination": {...}, "items": [...] }`
- **IAsyncPagedEnumerable from Stream** - Using `JsonSerializer.DeserializeAsyncPagedEnumerable` with Stream
- **IAsyncPagedEnumerable from PipeReader** - Using `JsonSerializer.DeserializeAsyncPagedEnumerable` with PipeReader

#### Serialization Benchmarks

- **IAsyncEnumerable to Stream (baseline)** - Standard JSON serialization
- **IAsyncPagedEnumerable to Stream** - Using `JsonSerializer.SerializeAsyncPaged` with Stream
- **IAsyncPagedEnumerable to PipeWriter** - Using `JsonSerializer.SerializeAsyncPaged` with PipeWriter

**Test Data:** 1,000 and 100,000 items

### 2. AspNetCoreStreamingBenchmarks.cs
Tests ASP.NET Core integration scenarios for both Controller and Minimal API patterns.

#### Controller-based Benchmarks

- **Baseline: Standard JSON array serialization** - Traditional JSON serialization without pagination
- **AsyncPagedEnumerableJsonOutputFormatter** - Using the custom output formatter for controllers
- **Direct Stream serialization** - Direct use of `JsonSerializer.SerializeAsyncPaged`

#### Minimal API Benchmarks

- **AsyncPagedEnumerableResult with JsonTypeInfo** - Using source-generated type info (AOT-compatible)
- **AsyncPagedEnumerableResult with JsonOptions** - Using runtime JSON options
- **AsyncPagedEnumerableResult auto-resolving** - Auto-resolution from DI container
- **PipeWriter with BodyWriter** - Direct use of Response.BodyWriter for streaming

#### Memory Allocation Benchmarks

- **IAsyncEnumerable enumeration** - Baseline memory usage
- **IAsyncPagedEnumerable enumeration** - Memory overhead of pagination metadata

**Test Data:** 1,000 and 50,000 items

## Key Features Tested

### Serialization Methods
- **Stream-based:** `JsonSerializer.SerializeAsyncPaged(Stream, ...)`
- **PipeWriter-based:** `JsonSerializer.SerializeAsyncPaged(PipeWriter, ...)`
- Both generic and non-generic overloads
- JsonTypeInfo (AOT-compatible) and JsonSerializerOptions variants

### Deserialization Methods
- **Stream-based:** `JsonSerializer.DeserializeAsyncPagedEnumerable(Stream, ...)`
- **PipeReader-based:** `JsonSerializer.DeserializeAsyncPagedEnumerable(PipeReader, ...)`
- **HttpContent extension:** `ReadFromJsonAsAsyncPagedEnumerable<T>()`
- Support for both array-root and paged JSON formats

### ASP.NET Core Integration
- **Controller pattern:** `AsyncPagedEnumerableJsonOutputFormatter`
- **Minimal API pattern:** `AsyncPagedEnumerableResult<T>`
- Automatic pagination metadata injection
- Compatible with both MVC and minimal APIs

## Output Format

The AsyncPaged serialization produces JSON in the following format:

```json
{
  "pagination": {
    "totalCount": 100000,
    "pageSize": 100000,
    "currentPage": 1,
    "continuationToken": null
  },
  "items": [
    { "id": 1, "name": "Item_1", ... },
    { "id": 2, "name": "Item_2", ... }
  ]
}
```

## Running the Benchmarks

To run all benchmarks:
```bash
dotnet run -c Release --project Xpandables.Net.Benchmarking
```

To run specific benchmarks:
```bash
dotnet run -c Release --project Xpandables.Net.Benchmarking --filter "*JsonStreaming*"
dotnet run -c Release --project Xpandables.Net.Benchmarking --filter "*AspNetCore*"
```

## Expected Performance Characteristics

### Memory Efficiency
- **IAsyncPagedEnumerable** should have minimal overhead compared to **IAsyncEnumerable**
- Streaming serialization should maintain low memory footprint regardless of collection size
- PipeWriter-based serialization should show better memory usage for large datasets

### Throughput
- Source-generated JsonTypeInfo should outperform runtime JsonSerializerOptions
- PipeReader/PipeWriter should show better performance for streaming scenarios
- AspNetCore integration should have minimal overhead compared to direct serialization

### AspNetCore Scenarios
- Minimal API with JsonTypeInfo should be fastest (AOT-compatible)
- Controller-based formatter should be comparable to minimal API
- Auto-resolving from DI should have slight overhead but better developer experience

## Notes

- All benchmarks use source-generated JSON serialization contexts for best performance
- The benchmarks use realistic data models with multiple property types
- Memory diagnostics and threading diagnostics are enabled for comprehensive analysis
- Baseline comparisons are included to measure the overhead of pagination features
