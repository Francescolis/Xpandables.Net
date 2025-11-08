# AsyncPaged Benchmarks

This directory contains comprehensive benchmarks comparing `IAsyncPagedEnumerable` serialization/deserialization performance with standard `JsonSerializer` operations.

## Benchmark Categories

### 1. AsyncPagedSerializationBenchmarks
Compares serialization performance across different dataset sizes and approaches:

#### Categories:
- **Small-Serialize** (10 items): Tests overhead of serialization framework
- **Medium-Serialize** (100 items): Tests typical API response sizes
- **Large-Serialize** (1000 items): Tests larger dataset handling
- **Memory-Efficiency** (10,000 items): Tests memory usage with very large datasets

#### Scenarios:
1. **StandardJsonSerializer** (Baseline): Traditional `JsonSerializer.SerializeAsync` with wrapper object
2. **AsyncPagedEnumerable_WithOptions**: Using `SerializeAsyncPaged` with `JsonSerializerOptions`
3. **AsyncPagedEnumerable_WithTypeInfo**: Using `SerializeAsyncPaged` with source-generated `JsonTypeInfo` (AOT-friendly)

### 2. AsyncPagedDeserializationBenchmarks
Compares deserialization performance and demonstrates streaming consumption:

#### Categories:
- **Small-Deserialize** (10 items)
- **Medium-Deserialize** (100 items)
- **Large-Deserialize** (1000 items)
- **Memory-Efficiency**: Compares full materialization vs streaming consumption

#### Scenarios:
1. **StandardJsonDeserializer** (Baseline): Traditional deserialization to wrapper object
2. **StandardJsonDeserializer_SourceGen**: Using source-generated context (AOT-friendly)
3. **ManualAsyncEnumerable**: Deserializing to `IAsyncPagedEnumerable` for streaming

### 3. AsyncPagedPipeWriterBenchmarks
Compares high-performance `PipeWriter`-based serialization vs `Stream`-based:

#### Categories:
- **Small-Transport** (10 items)
- **Medium-Transport** (100 items)
- **Large-Transport** (1000 items)
- **Comparison-Medium**: Direct comparison between PipeWriter and Stream approaches

#### Scenarios:
1. **Stream_AsyncPaged** (Baseline): Serialization to `MemoryStream`
2. **PipeWriter_AsyncPaged**: Serialization using `System.IO.Pipelines`
3. **Standard**: Traditional `JsonSerializer` for comparison

## Running the Benchmarks

### Run all benchmarks:
```bash
dotnet run -c Release --project Xpandables.Net.Benchmarking
```

### Run specific benchmark:
```bash
dotnet run -c Release --project Xpandables.Net.Benchmarking --filter "*AsyncPagedSerializationBenchmarks*"
```

### Run specific category:
```bash
dotnet run -c Release --project Xpandables.Net.Benchmarking --filter "*category=Medium-Serialize*"
```

## Expected Performance Characteristics

### Serialization
- **Small datasets**: Similar performance, slight overhead from streaming infrastructure
- **Medium datasets**: `AsyncPagedEnumerable` matches or slightly exceeds standard serializer
- **Large datasets**: `AsyncPagedEnumerable` shows better memory efficiency due to adaptive flushing
- **Source-generated TypeInfo**: ~10-20% faster than reflection-based options

### Deserialization
- **Standard approach**: Faster for immediate full materialization
- **Streaming approach**: Better for processing items individually without full materialization
- **Source generation**: ~15-30% improvement over reflection-based deserialization

### PipeWriter vs Stream
- **Small datasets**: Stream slightly faster due to lower setup overhead
- **Large datasets**: PipeWriter shines with better backpressure handling and memory usage
- **High-throughput scenarios**: PipeWriter provides superior scalability

## Key Findings

### Memory Efficiency
The `IAsyncPagedEnumerable` approach includes:
- **Adaptive flushing**: Automatically adjusts batch size based on dataset size
- **Memory-aware batching**: Flushes when byte threshold is reached (32KB default)
- **Streaming enumeration**: Items can be processed without full materialization

### AOT Compatibility
All benchmarks include source-generated JSON serialization contexts for:
- Native AOT compatibility
- Reduced startup time
- Smaller binary size
- Better performance

### Configuration
Benchmark parameters can be adjusted in each class:
- `[Params(10, 100, 1000, 10000)]` for item counts
- Dataset sizes (small, medium, large)
- Memory thresholds in flush strategies

## Test Model
All benchmarks use the `TestProduct` model:
```csharp
public sealed class TestProduct
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public required string Category { get; set; }
    public bool InStock { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

## Output
Benchmarks generate detailed reports including:
- Mean execution time
- Memory allocation statistics
- Standard deviation
- Comparison to baseline
- Allocation breakdown

Results are saved to `BenchmarkDotNet.Artifacts/results/` directory.
