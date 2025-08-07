# StoredProcedureBuilder - Comprehensive Database Stored Procedure Support

The `StoredProcedureBuilder` provides a fluent, type-safe interface for executing database stored procedures with comprehensive parameter support, advanced execution features, and robust error handling. This implementation follows .NET 9 patterns and integrates seamlessly with the existing Xpandables.Net architecture.

## Key Features

### 🚀 **Comprehensive Parameter Support**
- **Input Parameters**: Enhanced with type specification and validation
- **Output Parameters**: Support for retrieving output values after execution
- **Input/Output Parameters**: Bidirectional parameter support
- **Return Values**: Support for stored procedure return values
- **Table-Valued Parameters (TVP)**: Support for User-Defined Table Types
- **Parameter Size and Precision**: Explicit control over parameter metadata

### ⚡ **Advanced Execution Features**
- **Async Execution**: Full async/await pattern support with CancellationToken
- **Timeout Management**: Configurable command timeouts
- **Retry Policies**: Built-in retry mechanisms for transient failures
- **Connection Management**: Better integration with connection and transaction handling
- **Multiple Result Sets**: Support for procedures returning multiple result sets

### 🔒 **Type-Safe Parameter Binding**
- **Object Parameter Binding**: Automatic parameter extraction from objects
- **Expression-Based Binding**: Type-safe parameter binding using expressions
- **Validation and Error Handling**: Comprehensive parameter validation

### 📊 **Result Handling Enhancements**
- **Structured Results**: Comprehensive result object with all execution details
- **Output Parameter Retrieval**: Easy access to output parameters and return values
- **DataSet Support**: Enhanced DataSet handling for multiple result sets
- **Scalar and Non-Query Execution**: Specialized execution methods

## Quick Start

### Basic Usage

```csharp
using Xpandables.Net.Repositories.Sql;

// Create and execute a simple stored procedure
var result = await SqlBuilder.StoredProcedure("GetCustomer")
    .WithInputParameter("customerId", 123)
    .ExecuteAsync(connection);

// Access results
if (result.IsSuccess)
{
    DataTable customers = result.FirstTable;
    // Process results...
}
```

### Enhanced Parameter Support

```csharp
// Comprehensive parameter types
var result = await SqlBuilder.StoredProcedure("GetCustomerOrders")
    .WithInputParameter("customerId", 123)
    .WithInputParameter("startDate", DateTime.Now.AddDays(-30), SqlDbType.DateTime2)
    .WithOutputParameter("totalAmount", SqlDbType.Decimal, precision: 18, scale: 2)
    .WithInputOutputParameter("recordCount", 0, SqlDbType.Int)
    .WithReturnValue()
    .WithTableParameter("statusFilters", statusList, "dbo.StatusTableType")
    .ExecuteAsync(connection);

// Access output parameters and return value
decimal? totalAmount = result.GetOutputParameter<decimal>("totalAmount");
int? returnValue = result.GetReturnValue<int>();
int recordCount = result.GetOutputParameter<int>("recordCount");
```

### Advanced Execution Options

```csharp
// Advanced execution with timeout and retry policy
var result = await SqlBuilder.StoredProcedure("ProcessLargeDataset")
    .WithParameters(new { BatchSize = 1000, ProcessDate = DateTime.Today })
    .WithTimeout(TimeSpan.FromMinutes(5))
    .WithRetryPolicy(maxRetries: 3, delay: TimeSpan.FromSeconds(2))
    .ExecuteAsync(connection, cancellationToken);

// Check execution details
Console.WriteLine($"Execution time: {result.ExecutionTime}");
Console.WriteLine($"Rows affected: {result.RowsAffected}");
```

## Advanced Scenarios

### Object-Based Parameter Binding

```csharp
// Automatic parameter extraction from objects
var searchCriteria = new 
{ 
    CustomerId = 123, 
    Status = "Active", 
    MaxResults = 50,
    IncludeDetails = true 
};

var customers = await SqlBuilder.StoredProcedure("SearchCustomers")
    .WithParameters(searchCriteria)
    .ExecuteAsync(connection);
```

### Expression-Based Binding

```csharp
// Type-safe parameter binding using expressions
int customerId = 123;
string customerName = "John Doe";
bool isActive = true;

await SqlBuilder.StoredProcedure("UpdateCustomer")
    .WithParametersFromExpression(() => new { customerId, customerName, isActive })
    .ExecuteAsync(connection);
```

### Specialized Execution Methods

```csharp
// Scalar execution
var totalCount = await SqlBuilder.StoredProcedure("GetCustomerCount")
    .WithInputParameter("status", "Active")
    .ExecuteScalarAsync<int>(connection);

// Non-query execution
var rowsAffected = await SqlBuilder.StoredProcedure("UpdateCustomerStatus")
    .WithParameters(new { Status = "Inactive", LastUpdated = DateTime.Now })
    .ExecuteNonQueryAsync(connection);

// DataSet execution for multiple result sets
var dataSet = await SqlBuilder.StoredProcedure("GetCustomerReport")
    .WithInputParameter("reportDate", DateTime.Today)
    .ExecuteDataSetAsync(connection);
```

### Table-Valued Parameters

```csharp
// Create a collection of data
var orders = new[]
{
    new { OrderId = 1, CustomerId = 100, Amount = 150.50m },
    new { OrderId = 2, CustomerId = 101, Amount = 275.25m },
    new { OrderId = 3, CustomerId = 102, Amount = 99.99m }
};

// Execute with table-valued parameter
await SqlBuilder.StoredProcedure("ProcessOrders")
    .WithTableParameter("orders", orders, "dbo.OrderTableType")
    .WithInputParameter("processDate", DateTime.Today)
    .ExecuteAsync(connection);

// Or use DataTable directly
DataTable ordersTable = CreateOrdersDataTable();
await SqlBuilder.StoredProcedure("ProcessOrders")
    .WithStructuredParameter("orders", ordersTable, "dbo.OrderTableType")
    .ExecuteAsync(connection);
```

## Extension Methods for Common Scenarios

### Backward Compatibility

```csharp
// Legacy WithParameter method still works
var result = await SqlBuilder.StoredProcedure("GetCustomer")
    .WithParameter("customerId", 123) // Maps to WithInputParameter
    .ExecuteAsync(connection);
```

### Common Patterns

```csharp
// Common output parameters
await SqlBuilder.StoredProcedure("ProcessData")
    .WithInputParameter("dataId", 123)
    .WithCommonOutputs() // Adds RecordCount and ErrorMessage outputs
    .ExecuteAsync(connection);

// Pagination support
await SqlBuilder.StoredProcedure("GetCustomers")
    .WithPagination(pageIndex: 0, pageSize: 25)
    .ExecuteAsync(connection);

// Search functionality
await SqlBuilder.StoredProcedure("SearchProducts")
    .WithSearch("laptop", "name", "description", "category")
    .ExecuteAsync(connection);

// Date range filtering
await SqlBuilder.StoredProcedure("GetOrdersByDate")
    .WithDateRange(startDate: DateTime.Today.AddDays(-30), 
                   endDate: DateTime.Today, 
                   dateFieldPrefix: "Order")
    .ExecuteAsync(connection);

// Standard options (timeout + retry policy)
await SqlBuilder.StoredProcedure("ComplexOperation")
    .WithStandardOptions(timeoutMinutes: 10, maxRetries: 5, retryDelaySeconds: 3)
    .ExecuteAsync(connection);
```

## Error Handling and Retry Policies

### Built-in Retry Logic

```csharp
var result = await SqlBuilder.StoredProcedure("SometimesFailsProc")
    .WithRetryPolicy(maxRetries: 3, delay: TimeSpan.FromSeconds(2))
    .ExecuteAsync(connection);

if (result.HasErrors)
{
    Console.WriteLine("Execution failed after retries:");
    foreach (var error in result.ErrorMessages)
    {
        Console.WriteLine($"- {error}");
    }
}
```

### Custom Error Handling

```csharp
try
{
    var result = await SqlBuilder.StoredProcedure("RiskyOperation")
        .WithInputParameter("dataId", 123)
        .ExecuteAsync(connection);
        
    if (!result.IsSuccess)
    {
        // Handle stored procedure errors
        var errorMessage = result.GetOutputParameter<string>("ErrorMessage");
        throw new InvalidOperationException($"Stored procedure failed: {errorMessage}");
    }
}
catch (Exception ex) when (ex.Message.Contains("timeout"))
{
    // Handle timeout specifically
    Console.WriteLine("Operation timed out, retrying with longer timeout...");
}
```

## Connection and Transaction Management

### Using with Transactions

```csharp
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

using var transaction = await connection.BeginTransactionAsync();

try
{
    // Execute multiple procedures in the same transaction
    await SqlBuilder.StoredProcedure("CreateOrder")
        .WithParameters(new { CustomerId = 123, OrderDate = DateTime.Today })
        .WithTransaction(transaction)
        .ExecuteAsync(connection);

    await SqlBuilder.StoredProcedure("UpdateInventory")
        .WithParameters(new { ProductId = 456, Quantity = -1 })
        .WithTransaction(transaction)
        .ExecuteAsync(connection);

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Connection Management

```csharp
// Builder manages connection lifecycle
var result = await SqlBuilder.StoredProcedure("GetData")
    .WithConnection(connection) // Connection set on builder
    .WithInputParameter("id", 123)
    .ExecuteAsync(); // No connection parameter needed

// Or pass connection at execution time
var result = await SqlBuilder.StoredProcedure("GetData")
    .WithInputParameter("id", 123)
    .ExecuteAsync(connection); // Connection passed to Execute
```

## Performance Considerations

### Connection Pooling
The builder works seamlessly with .NET's connection pooling. It only opens connections when needed and closes them appropriately.

### Parameter Caching
Parameters are created efficiently with minimal allocations in hot paths.

### Async Best Practices
All methods use `ConfigureAwait(false)` to prevent deadlocks in ASP.NET applications.

```csharp
// Efficient execution with proper async patterns
var result = await SqlBuilder.StoredProcedure("HighVolumeProc")
    .WithInputParameter("batchId", batchId)
    .WithTimeout(TimeSpan.FromMinutes(30)) // Long-running operation
    .ExecuteAsync(connection, cancellationToken);
```

## Testing and Mocking

The builder is designed to be easily testable with dependency injection and mocking frameworks:

```csharp
// In your tests, you can mock the DbConnection
Mock<DbConnection> connectionMock = new Mock<DbConnection>();
Mock<DbCommand> commandMock = new Mock<DbCommand>();

connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);
// ... setup other mocks

var result = await SqlBuilder.StoredProcedure("TestProc")
    .WithInputParameter("testParam", 123)
    .ExecuteAsync(connectionMock.Object);
```

## Migration from Existing Code

### Backward Compatibility

The `WithParameter` method provides backward compatibility with existing code:

```csharp
// Old code continues to work
var oldResult = await someBuilder.WithParameter("param1", value1);

// But new code can use enhanced features
var newResult = await SqlBuilder.StoredProcedure("SameProc")
    .WithInputParameter("param1", value1, SqlDbType.NVarChar, size: 100)
    .WithOutputParameter("result", SqlDbType.Int)
    .ExecuteAsync(connection);
```

### Migration Steps

1. **Replace basic calls**: Change `WithParameter` to `WithInputParameter` for new code
2. **Add type specifications**: Include `SqlDbType` for better performance and type safety
3. **Use output parameters**: Replace separate queries with output parameters where appropriate
4. **Implement retry policies**: Add retry logic for production resilience
5. **Use structured results**: Take advantage of the comprehensive result object

## Best Practices

### 1. Use Explicit Types
```csharp
// Good: Explicit type specification
.WithInputParameter("date", DateTime.Now, SqlDbType.DateTime2)

// Okay: Type inference (but less optimal)
.WithInputParameter("date", DateTime.Now)
```

### 2. Handle Output Parameters
```csharp
// Always check for output parameters
var result = await procedure.ExecuteAsync(connection);
if (result.IsSuccess)
{
    var outputValue = result.GetOutputParameter<int>("OutputParam");
    if (outputValue.HasValue)
    {
        // Use the output value
    }
}
```

### 3. Use Appropriate Execution Methods
```csharp
// For single values
int count = await builder.ExecuteScalarAsync<int>(connection) ?? 0;

// For updates/inserts/deletes
int rowsAffected = await builder.ExecuteNonQueryAsync(connection);

// For complex results
var result = await builder.ExecuteAsync(connection);
```

### 4. Configure Timeouts for Long Operations
```csharp
await SqlBuilder.StoredProcedure("LongRunningReport")
    .WithTimeout(TimeSpan.FromMinutes(15))
    .WithRetryPolicy(2, TimeSpan.FromSeconds(30))
    .ExecuteAsync(connection);
```

### 5. Use Extension Methods for Common Patterns
```csharp
// Leverage extension methods for common scenarios
await SqlBuilder.StoredProcedure("SearchUsers")
    .WithSearch(searchTerm, "name", "email")
    .WithPagination(page, pageSize)
    .WithDateRange(startDate, endDate, "Created")
    .WithStandardOptions()
    .ExecuteAsync(connection);
```

This comprehensive StoredProcedureBuilder provides a modern, robust solution for stored procedure execution in .NET 9 applications while maintaining backward compatibility and following established patterns within the Xpandables.Net framework.