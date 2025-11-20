# IAsyncPagedEnumerable Test Suite

Comprehensive unit, integration, and performance tests for the `IAsyncPagedEnumerable` interface and related components in the Xpandables.Net library.

## Test Files Overview

### 1. **IAsyncPagedEnumerableTests.cs**
Unit tests for the core `IAsyncPagedEnumerable` functionality.

#### Test Classes

**IAsyncPagedEnumerableTests**
- Tests factory methods (`Create`, `Empty`)
- Tests pagination metadata computation
- Tests lazy evaluation and caching
- Tests cancellation token propagation
- Tests resource cleanup

Key test scenarios:
- Creating paged enumerables from async sources
- Using custom pagination factories
- Handling empty enumerables
- Multiple concurrent enumerations
- Pagination caching and thread safety

**PaginationTests**
- Tests `Pagination` record creation and validation
- Tests pagination navigation (`NextPage`, `PreviousPage`)
- Tests pagination calculation (`Skip`, `Take`, `TotalPages`)
- Tests pagination state queries (`HasNextPage`, `IsLastPage`, `HasContinuation`)
- Tests edge cases (null total count, zero page size)

### 2. **AsyncEnumerableExtensionsTests.cs**
Unit and integration tests for async enumerable extension methods.

#### Test Classes

**AsyncEnumerableExtensionsTests**
- Tests `ToAsyncPagedEnumerable()` conversion methods
- Tests conversion with pagination factories
- Tests conversion with explicit pagination
- Tests conversion with total count
- Tests error handling and validation

**IAsyncPagedEnumerableExtensionsTests**
- Tests `GetArgumentType()` for type information extraction
- Tests support for various element types

**AsyncPagedEnumerableIntegrationTests**
- Tests large dataset handling (10,000 items)
- Tests multiple concurrent consumers
- Tests cancellation token respect
- Tests concurrent enumeration with different tokens
- Tests exception propagation from pagination factories
- Tests pagination factory caching across concurrent calls
- Tests thread-safe pagination computation

### 3. **JsonSerializationTests.cs**
Tests for JSON serialization/deserialization of paged enumerables.

#### Key Features
- Tests serialization to streams and pipe writers
- Tests deserialization from JSON
- Tests large dataset serialization (1,000 items)
- Tests round-trip serialization/deserialization
- Tests adaptive flushing strategy
- AOT-compliant serialization with source-generated contexts

### 4. **AsyncPagedEnumerableRealWorldTests.cs**
Integration tests simulating real-world usage scenarios.

#### Test Scenarios
- **Database Query Pagination**: Simulates server-side pagination with SQL-like queries
- **Streaming Large Datasets**: Tests efficient memory usage with large data volumes
- **Multiple Concurrent Consumers**: Tests independent processing of the same data
- **Dynamic Pagination Filtering**: Adapts pagination to filtered results
- **Rate-Limited API Consumption**: Respects timing constraints
- **Transformation with Pagination**: Applies LINQ transformations efficiently
- **Error Recovery**: Handles transient errors gracefully
- **Cursor-Based Pagination**: Implements continuation token-based paging
- **Monitoring and Metrics**: Tracks processing statistics

### 5. **AsyncPagedEnumerablePerformanceTests.cs**
Performance and stress tests for production scenarios.

#### Test Classes

**AsyncPagedEnumerablePerformanceTests**
- **Large Dataset Test**: 100,000 items with memory efficiency validation
- **Concurrent Pagination Computation**: Verifies serialization of pagination factory calls
- **Enumeration Speed**: Tests processing throughput
- **High Concurrency**: 10 concurrent consumers on 10,000 items
- **Rapid Creation/Disposal**: 1,000 iterations of create-process-dispose cycles
- **Rapid Cancellation**: Tests cancellation resilience
- **Garbage Collection Pressure**: Tests behavior under GC pressure
- **Task Scheduling Efficiency**: Large number of async awaits
- **Nested Pagination**: Tests pagination composition

**AsyncPagedEnumerableStressTests**
- **Extreme Concurrency**: 100 concurrent consumers
- **Delayed Computation**: Pagination factory with 100ms delay
- **Concurrent Access Serialization**: Verifies thread safety under load

## Test Characteristics

### .NET 10 and C# 14 Features
- Uses latest async/await patterns
- Leverages nullable reference types
- Uses record types for data models
- Employs `required` keyword for property initialization

### AOT Compliance
- All tests compatible with Ahead-of-Time compilation
- Uses source-generated JSON serialization contexts
- Avoids runtime reflection where possible
- Explicitly provides type metadata

### Best Practices
- **Async/Await**: Proper async context usage
- **Resource Management**: Correct disposal patterns
- **Cancellation**: Proper cancellation token handling
- **Thread Safety**: Tests thread-safe operations
- **Error Handling**: Tests exception scenarios
- **Memory Efficiency**: Validates memory usage patterns

## Running the Tests

### Run All Tests
```bash
dotnet test Xpandables.Net.UnitTests
```

### Run Specific Test Class
```bash
dotnet test Xpandables.Net.UnitTests --filter "FullyQualifiedName~IAsyncPagedEnumerableTests"
```

### Run Performance Tests Only
```bash
dotnet test Xpandables.Net.UnitTests --filter "FullyQualifiedName~Performance"
```

### Run with Code Coverage
```bash
dotnet test Xpandables.Net.UnitTests /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

## Test Coverage

### Core Functionality
- ? Factory methods and initialization
- ? Async enumeration
- ? Pagination metadata
- ? Lazy evaluation
- ? Caching and memoization
- ? Thread safety
- ? Resource cleanup

### Extension Methods
- ? Conversion methods
- ? Type information extraction
- ? Null validation

### Serialization
- ? Stream-based serialization
- ? Pipe-based serialization
- ? Large dataset handling
- ? Round-trip preservation
- ? Pagination metadata serialization

### Real-World Scenarios
- ? Database pagination
- ? Streaming data
- ? Concurrent processing
- ? Dynamic filtering
- ? Rate limiting
- ? Cursor-based pagination
- ? Error recovery

### Performance
- ? Memory efficiency
- ? Throughput
- ? Concurrency handling
- ? Resource cleanup
- ? Garbage collection resilience

## Edge Cases and Error Scenarios

The test suite covers:
- **Empty collections**: Proper handling of zero-item enumerables
- **Null values**: Validation of null arguments
- **Negative values**: Rejection of invalid parameters
- **Large datasets**: Efficiency with 100K+ items
- **Cancellation**: Proper cancellation propagation
- **Exceptions**: Error propagation and recovery
- **Concurrent access**: Thread safety verification
- **Resource limits**: Memory and task limits

## Performance Benchmarks

Key performance targets:
- **100K items**: Complete enumeration in < 5 seconds
- **Concurrent pagination**: Single computation for multiple calls
- **Memory usage**: < 10% of dataset size for streaming
- **Task scheduling**: Handle 10K+ concurrent consumers

## Integration with CI/CD

The tests are designed for:
- Continuous Integration environments
- Automated test runs on commit
- Coverage reporting
- Performance regression detection
- AOT compilation verification

## Dependencies

- **xUnit**: Test framework
- **System.Text.Json**: Serialization
- **.NET 10**: Target runtime
- **Xpandables.Net.Collections**: Library under test

## Future Enhancements

Potential test additions:
- **Database benchmarks**: Real database pagination scenarios
- **HTTP client tests**: Integration with HTTP APIs
- **Large file handling**: Streaming from files
- **Memory pressure tests**: Behavior under memory constraints
- **Distributed scenarios**: Multi-process pagination
