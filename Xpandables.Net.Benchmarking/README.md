# Xpandables.Net Benchmarking

This project contains performance benchmarks for the Xpandables.Net library components using BenchmarkDotNet.

## ?? Available Benchmarks

### AsyncPagedEnumerableBenchmark

Compares the performance of `IAsyncPagedEnumerable` serialization and deserialization against the framework's `IAsyncEnumerable`.

#### Scenarios Tested

**Serialization:**
- Framework `IAsyncEnumerable` serialization using `JsonSerializer.SerializeAsync`
- Custom `IAsyncPagedEnumerable` serialization using `JsonSerializer.SerializeAsyncPaged`

**Deserialization:**
- Framework `IAsyncEnumerable` deserialization from `HttpContent` using `ReadFromJsonAsAsyncEnumerable`
- Custom `IAsyncPagedEnumerable` deserialization from `HttpContent` using `ReadFromJsonAsAsyncPagedEnumerable`

#### Data Volumes

Benchmarks are run against three different data set sizes:
- **Small**: 100 items
- **Medium**: 1,000 items
- **Large**: 10,000 items

#### Metrics Measured

- **Execution Time**: Mean, median, min, max execution times
- **Memory Allocations**: Total bytes allocated, GC collections
- **Throughput**: Operations per second
- **Relative Performance**: Comparison ratios between approaches

## ?? Running the Benchmarks

### Prerequisites

- .NET 10 SDK or later
- Visual Studio 2022/2026 or JetBrains Rider (optional)
- At least 4GB of available RAM for large datasets

### Quick Start

1. Navigate to the benchmarking project directory:
   ```bash
   cd Xpandables.Net.Benchmarking
   ```

2. Build in Release mode (required for accurate benchmarks):
   ```bash
   dotnet build -c Release
   ```

3. Run all benchmarks:
   ```bash
   dotnet run -c Release
   ```

4. Run specific benchmark:
   ```bash
   dotnet run -c Release --filter *AsyncPagedEnumerable*
   ```

### Advanced Options

#### Filter by Method

Run only serialization benchmarks:
```bash
dotnet run -c Release --filter *Serialize*
```

Run only deserialization benchmarks:
```bash
dotnet run -c Release --filter *Deserialize*
```

#### Filter by Data Size

Run only benchmarks with 100 items:
```bash
dotnet run -c Release --filter *ItemCount=100*
```

#### Export Results

Export results to various formats:

**HTML Report:**
```bash
dotnet run -c Release --exporters html
```

**CSV:**
```bash
dotnet run -c Release --exporters csv
```

**Markdown:**
```bash
dotnet run -c Release --exporters markdown
```

**JSON:**
```bash
dotnet run -c Release --exporters json
```

#### Memory Profiler

Enable detailed memory diagnostics:
```bash
dotnet run -c Release --memory
```

#### Disable Memory Diagnoser

For faster runs without memory profiling:
```bash
dotnet run -c Release --disableMemoryDiagnoser
```

## ?? Interpreting Results

### Example Output

```
BenchmarkDotNet v0.15.6, Windows 11
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0, X64 RyuJIT AVX2
  Job-ABCDEF : .NET 8.0.0, X64 RyuJIT AVX2

| Method                                         | ItemCount | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated |
|----------------------------------------------- |---------- |----------:|---------:|---------:|------:|-------:|----------:|
| Framework IAsyncEnumerable Serialization       | 100       | 1.234 ms  | 0.012 ms | 0.011 ms | 1.00  | 15.625 | 128.5 KB  |
| Custom IAsyncPagedEnumerable Serialization     | 100       | 1.456 ms  | 0.015 ms | 0.014 ms | 1.18  | 17.578 | 145.2 KB  |
```

### Key Columns

- **Mean**: Average execution time
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation of all measurements
- **Ratio**: Performance relative to baseline (lower is better)
- **Gen0/Gen1/Gen2**: Number of GC collections per 1000 operations
- **Allocated**: Total memory allocated

### Performance Considerations

**Expected Overhead for IAsyncPagedEnumerable:**

The `IAsyncPagedEnumerable` implementation includes additional features:
- Pagination metadata extraction and storage
- Envelope format handling (pagination + items)
- Lazy computation of pagination information

This means a small performance overhead (typically 10-20%) is expected compared to the framework implementation, but with the benefit of:
- Rich pagination metadata
- Standardized envelope format
- Seamless integration with paginated APIs

## ?? Sample Data Model

The benchmark uses a realistic data model (`SampleData`) that includes:

```csharp
public sealed record SampleData
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required int Value { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required bool IsActive { get; init; }
    public required string[] Tags { get; init; }
}
```

This model represents a typical domain entity with:
- Primary key (Guid)
- String properties
- Numeric properties
- DateTime properties
- Boolean flags
- Collections (arrays)

## ?? Best Practices

1. **Always run in Release mode**: Debug builds can be 10-100x slower
2. **Close other applications**: Minimize background processes
3. **Multiple runs**: Run benchmarks multiple times to ensure consistency
4. **Baseline comparison**: Use the `Baseline = true` attribute to compare implementations
5. **Memory profiling**: Use `MemoryDiagnoser` to identify allocation hotspots

## ?? Adding New Benchmarks

To add a new benchmark:

1. Create a new class in the `Xpandables.Net.Benchmarking` project
2. Add the `[MemoryDiagnoser]` attribute
3. Add `[SimpleJob(RuntimeMoniker.Net80)]` for the runtime
4. Mark benchmark methods with `[Benchmark]`
5. Add the class to `Program.cs`:

```csharp
BenchmarkRunner.Run(
[
    typeof(AsyncPagedEnumerableBenchmark),
    typeof(YourNewBenchmark)  // Add here
]);
```

## ?? Resources

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [BenchmarkDotNet Best Practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)
- [.NET Performance Tips](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/performance-tips)

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
