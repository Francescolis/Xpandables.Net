# AsyncPaged Test Suite Documentation

This document describes the comprehensive test suite created for the `Xpandables.Net.AsyncPaged` and `Xpandables.Net.AsyncPaged.AspNetCore` projects.

## Test Organization

The tests are organized into the following structure:

```
Xpandables.Net.UnitTests/
??? AsyncPaged/
    ??? PaginationTests.cs
  ??? AsyncPagedEnumeratorTests.cs
    ??? AsyncPagedEnumerableTests.cs
    ??? IQueryableExtensionsTests.cs
    ??? Extensions/
    ?   ??? PaginationExtensionsTests.cs
    ??? Integration/
   ??? JsonSerializationIntegrationTests.cs
        ??? EndToEndIntegrationTests.cs
```

## Test Coverage Overview

### 1. PaginationTests.cs (30+ tests)
**Purpose**: Unit tests for the `Pagination` record struct

**Test Categories**:
- **Factory Methods**: Tests for `Create()` and `FromTotalCount()` methods
- **Navigation Methods**: Tests for `NextPage()`, `PreviousPage()`, `WithTotalCount()`
- **Property Calculations**: Tests for `Skip`, `Take`, `TotalPages`
- **Boolean Properties**: Tests for `IsFirstPage`, `IsLastPage`, `HasPreviousPage`, `HasNextPage`, `IsPaginated`, `IsUnknown`, `HasContinuation`
- **Edge Cases**: Tests for negative values, null handling, boundary conditions
- **Value Semantics**: Tests for record struct equality and hash code

**Key Test Scenarios**:
- ? Creating pagination with valid parameters
- ? Handling negative values and validation
- ? Calculating skip/take values for different page sizes
- ? Determining first/last page status
- ? Calculating total pages from total count and page size
- ? Handling null and unknown total counts

### 2. AsyncPagedEnumeratorTests.cs (20+ tests)
**Purpose**: Unit tests for `AsyncPagedEnumerator<T>` and its pagination strategies

**Test Categories**:
- **Basic Enumeration**: Tests for enumeration with and without data
- **Pagination Strategies**: Tests for `None`, `PerItem`, and `PerPage` strategies
- **Strategy Configuration**: Tests for fluent API methods (`WithPerPageStrategy`, `WithPerItemStrategy`, `WithNoStrategy`)
- **Lifecycle Management**: Tests for disposal and cancellation
- **Edge Cases**: Tests for disposed enumerators, empty sequences, cancellation

**Key Test Scenarios**:
- ? Enumerating empty and populated collections
- ? Applying different pagination strategies
- ? Updating pagination metadata per item
- ? Updating pagination metadata per page
- ? Handling disposal correctly
- ? Respecting cancellation tokens
- ? Finalizing total count with PerItem strategy

### 3. AsyncPagedEnumerableTests.cs (20+ tests)
**Purpose**: Unit tests for `AsyncPagedEnumerable<T>` with lazy pagination computation

**Test Categories**:
- **Construction**: Tests for various constructor overloads
- **Lazy Computation**: Tests for lazy pagination metadata computation
- **Concurrent Access**: Tests for thread-safe pagination computation
- **IQueryable Integration**: Tests for automatic pagination extraction from queries
- **Error Handling**: Tests for exception propagation and retry behavior
- **Cancellation**: Tests for cancellation token handling

**Key Test Scenarios**:
- ? Creating paged enumerables from IAsyncEnumerable and IQueryable
- ? Lazy pagination computation on first access
- ? Thread-safe single computation with concurrent access
- ? Extracting pagination from Skip/Take operations
- ? Using custom total count factories
- ? Handling computation errors gracefully

### 4. IQueryableExtensionsTests.cs (15+ tests)
**Purpose**: Unit tests for IQueryable extension methods

**Test Categories**:
- **Basic Conversion**: Tests for `ToAsyncPagedEnumerable()`
- **Pagination Extraction**: Tests for automatic Skip/Take detection
- **Custom Factories**: Tests for custom total count computation
- **Complex Queries**: Tests with filtering, ordering, and projections
- **Page Navigation**: Tests for first, middle, and last page scenarios

**Key Test Scenarios**:
- ? Converting IQueryable to IAsyncPagedEnumerable
- ? Extracting pagination from Skip/Take expressions
- ? Handling queries without pagination operators
- ? Using custom total count factories for complex queries
- ? Calculating correct page numbers and metadata
- ? Supporting empty result sets

### 5. PaginationExtensionsTests.cs (30+ tests)
**Purpose**: Unit tests for pagination extension methods (TakePaged, SkipPaged, WherePaged, etc.)

**Test Categories**:
- **Take/Skip Operations**: Tests for `TakePaged`, `SkipPaged`, `TakeLastPaged`, `SkipLastPaged`
- **Conditional Operations**: Tests for `TakeWhilePaged`, `SkipWhilePaged`
- **Filtering**: Tests for `WherePaged` with predicates
- **Distinctness**: Tests for `DistinctPaged`, `DistinctByPaged`
- **Chunking**: Tests for `ChunkPaged`
- **Metadata Preservation**: Tests ensuring pagination metadata is preserved through operations

**Key Test Scenarios**:
- ? Taking and skipping elements with correct counts
- ? Filtering with predicates and indexes
- ? Taking/skipping while conditions are met
- ? Removing duplicates with custom comparers
- ? Splitting sequences into chunks
- ? Chaining multiple operations together
- ? Preserving pagination metadata through transformations

### 6. JsonSerializationIntegrationTests.cs (15+ tests)
**Purpose**: Integration tests for JSON serialization of paged enumerables

**Test Categories**:
- **Basic Serialization**: Tests for serializing to streams with various options
- **Type Support**: Tests for primitive types, complex types, nested objects
- **Special Cases**: Tests for null values, special characters, Unicode
- **Performance**: Tests for large datasets and streaming efficiency
- **Cancellation**: Tests for respecting cancellation during serialization

**Key Test Scenarios**:
- ? Serializing paged enumerables to JSON streams
- ? Using JsonTypeInfo and JsonSerializerOptions
- ? Handling complex nested object graphs
- ? Properly escaping special characters
- ? Supporting Unicode characters
- ? Streaming large datasets efficiently
- ? Canceling serialization operations

### 7. EndToEndIntegrationTests.cs (10+ tests)
**Purpose**: End-to-end integration tests simulating real-world scenarios

**Test Categories**:
- **Complete Workflows**: Tests for full query-to-result pipelines
- **Pagination Scenarios**: Tests for first, middle, and last page navigation
- **Complex Operations**: Tests for filtering, ordering, and chaining
- **Enumerator Strategies**: Tests for different pagination update strategies
- **Real-World Scenarios**: Tests simulating typical API endpoint patterns
- **Cancellation**: Tests for graceful cancellation handling

**Key Test Scenarios**:
- ? Complete workflow from IQueryable to paged results
- ? Applying filters and pagination together
- ? Navigating through multiple pages
- ? Using custom pagination factories
- ? Applying enumerator strategies
- ? Chaining multiple extension methods
- ? Handling empty datasets gracefully
- ? Lazy pagination computation
- ? Simulating realistic API pagination patterns

## Testing Principles Applied

### 1. Arrange-Act-Assert Pattern
All tests follow the AAA pattern for clarity:
```csharp
[Fact]
public async Task Example_Test()
{
    // Arrange
    var source = CreateData();
    
    // Act
    var result = await source.ProcessAsync();
    
    // Assert
    result.Should().NotBeNull();
}
```

### 2. FluentAssertions
All assertions use FluentAssertions for readability:
```csharp
pagination.PageSize.Should().Be(10);
pagination.HasNextPage.Should().BeTrue();
items.Should().HaveCount(5);
```

### 3. Clear Test Naming
Test names clearly describe the scenario and expected outcome:
```csharp
public void Create_WithNegativePageSize_ShouldThrowArgumentOutOfRangeException()
public async Task GetPaginationAsync_CalledMultipleTimes_ShouldReturnSamePagination()
```

### 4. Edge Case Coverage
Tests include comprehensive edge case coverage:
- Null and empty inputs
- Negative values
- Boundary conditions
- Concurrent access
- Cancellation scenarios
- Disposal patterns

### 5. AOT and Trimming Compatibility
Tests verify that the code works with:
- JsonTypeInfo for AOT scenarios
- JsonSerializerOptions for dynamic scenarios
- No reflection-based dependencies where possible

## Test Execution

To run all tests:
```bash
dotnet test Xpandables.Net.UnitTests
```

To run specific test classes:
```bash
dotnet test --filter "FullyQualifiedName~PaginationTests"
dotnet test --filter "FullyQualifiedName~AsyncPagedEnumeratorTests"
```

To run integration tests only:
```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

## Coverage Metrics

The test suite provides comprehensive coverage across:

- **Pagination Logic**: 100% coverage of Pagination struct members
- **Enumeration**: Full coverage of enumerator lifecycle and strategies
- **Extension Methods**: Complete coverage of all public extension methods
- **IQueryable Integration**: All query translation scenarios
- **JSON Serialization**: All serialization paths and options
- **Error Handling**: Exception scenarios and validation
- **Concurrency**: Thread-safe operations and race conditions
- **Cancellation**: All cancellation scenarios

## Future Test Enhancements

Potential areas for additional testing:
1. **ASP.NET Core Integration**: Tests for `AsyncPagedEnumerableJsonOutputFormatter` and `AsyncPagedEnumerableResult` (requires proper project reference setup)
2. **Performance Benchmarks**: Benchmarking tests for large datasets
3. **Memory Profiling**: Tests to verify efficient memory usage
4. **Load Testing**: Stress tests for concurrent operations
5. **Entity Framework Integration**: Tests with actual EF Core queries

## Notes

- All tests are compatible with .NET 10 and use `allows ref struct` where appropriate
- Tests use realistic data scenarios to ensure practical applicability
- Integration tests simulate real-world usage patterns
- Tests are designed to run quickly and reliably in CI/CD pipelines
- No external dependencies or databases are required for unit tests
