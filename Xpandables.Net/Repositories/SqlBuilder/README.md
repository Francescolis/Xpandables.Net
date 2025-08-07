# SqlBuilder - Enhanced SQL Query Builder for .NET 9

The SqlBuilder provides a fluent, type-safe way to build SQL queries with enhanced multi-source support, improved expression parsing, and better architecture.

## Features

### ✅ Factory Pattern
```csharp
var query = SqlBuilder.From<Customer>("c")
    .InnerJoin<Order>((c, o) => c.Id == o.CustomerId)
    .Where(c => c.IsActive)
    .Select((c, o) => new { c.Name, o.Total });
```

### ✅ Multi-Source Support
- **GroupBy**: Support grouping across joined tables
- **OrderBy**: Add `ThenBy` and `ThenByDescending` methods  
- **Having**: Support having clauses across multiple sources
- **Better alias management** for joined tables

### ✅ Enhanced Expression Parser
- Support for SQL operators (`IN`, `NOT IN`, `LIKE`, `IS NULL`, etc.)
- Better handling of method calls (`Contains`, `StartsWith`, `EndsWith`)
- Support for mathematical operations
- Improved parameter handling

### ✅ Refactored Build Method
- Broken down into smaller, focused methods:
  - `BuildSelectClause()`
  - `BuildFromClause()`
  - `BuildJoinClauses()`
  - `BuildWhereClause()`
  - `BuildGroupByClause()`
  - `BuildHavingClause()`
  - `BuildOrderByClause()`
  - `BuildPaginationClause()`

## API Examples

### Enhanced GroupBy
```csharp
// Multi-source GroupBy
var query = SqlBuilder.From<Customer>("c")
    .InnerJoin<Order>((c, o) => c.Id == o.CustomerId)
    .GroupBy<Order>((c, o) => new { c.Category, o.Status })
    .Having(c => c.Category != null);
```

### Enhanced OrderBy
```csharp
// Chained ordering
var query = SqlBuilder.From<Customer>("c")
    .InnerJoin<Order>((c, o) => c.Id == o.CustomerId)
    .OrderBy(c => c.Name)
    .ThenByDescending<Order>((c, o) => o.CreatedDate)
    .ThenBy(c => c.Id);
```

### Better Expression Support
```csharp
var statuses = new[] { "Active", "Premium" };

var query = SqlBuilder.From<Customer>("c")
    // Enhanced expressions
    .Where(c => c.Name.StartsWith("A"))
    .Where(c => c.Age.HasValue && c.Age > 18)
    .Where(c => statuses.Contains(c.Status));
```

### SQL Functions
```csharp
var query = SqlBuilder.From<Order>("o")
    .GroupBy(o => o.Status)
    .Select(o => new {
        Status = o.Status,
        TotalCount = SqlFunction.Count(),
        TotalAmount = SqlFunction.Sum("[o].[Total]"),
        AverageAmount = SqlFunction.Avg("[o].[Total]")
    });
```

## Supported Join Types
- `InnerJoin<T>()`
- `LeftJoin<T>()`
- `RightJoin<T>()`
- `FullJoin<T>()`

## Supported SQL Features
- ✅ SELECT with column selection
- ✅ FROM with automatic table aliases
- ✅ WHERE with complex expressions
- ✅ JOIN (all types) with multi-source support
- ✅ GROUP BY with multi-source support
- ✅ HAVING with multi-source support
- ✅ ORDER BY with ThenBy chaining
- ✅ DISTINCT
- ✅ Pagination (SKIP/TAKE using OFFSET/FETCH NEXT)
- ✅ Parameter management and SQL injection protection
- ✅ Automatic alias generation

## Architecture

### Core Classes
- **SqlBuilder<T>**: Main fluent builder class
- **SqlBuilder**: Static factory class
- **QueryModel**: Internal query state management
- **ExpressionParser**: LINQ to SQL expression conversion
- **TableModel**: Table and alias management
- **JoinModel**: Join definitions
- **OrderByModel**: Ordering logic
- **SqlFunction**: SQL function support

### Expression Support
- Comparison operators: `==`, `!=`, `<`, `>`, `<=`, `>=`
- Logical operators: `&&` (AND), `||` (OR), `!` (NOT)
- String methods: `Contains()`, `StartsWith()`, `EndsWith()`
- Null checks: `== null`, `!= null`, `HasValue`
- Collection operations: `Contains()` (IN operator)
- Mathematical operations: `+`, `-`, `*`, `/`, `%`
- String functions: `ToUpper()`, `ToLower()`, `Trim()`

## Usage

```csharp
// Build a complex query
var result = SqlBuilder.From<Customer>("c")
    .InnerJoin<Order>((c, o) => c.Id == o.CustomerId, "o")
    .Select((c, o) => new { c.Name, o.Total })
    .Where(c => c.IsActive)
    .Where<Order>((c, o) => o.CreatedDate >= DateTime.Today.AddDays(-30))
    .GroupBy<Order>((c, o) => new { c.Category, o.Status })
    .Having(c => c.Category != null)
    .OrderBy(c => c.Name)
    .ThenByDescending<Order>((c, o) => o.CreatedDate)
    .Skip(20)
    .Take(10)
    .Build();

// result.Sql contains the generated SQL
// result.Parameters contains the parameterized values
```

## .NET 9 Features Used
- Nullable reference type annotations throughout
- Latest C# features and pattern matching
- Modern expression tree handling
- Proper parameter and SQL injection protection

## Backward Compatibility
The SqlBuilder is designed as a new implementation that provides the enhanced API specified in the requirements. It maintains consistent patterns with existing Xpandables.Net library conventions.