//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

using Xpandables.Net.Repositories.SqlBuilder;

namespace Xpandables.Net.Repositories.SqlBuilder;

/// <summary>
/// Example usage of the SqlBuilder functionality demonstrating the enhanced API.
/// This class serves as documentation for the expected API usage patterns.
/// </summary>
public static class SqlBuilderExamples
{
    /// <summary>
    /// Example: Factory Pattern
    /// </summary>
    public static void FactoryPatternExample()
    {
        // New factory approach
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId)
            .Where(c => c.IsActive)
            .Select((c, o) => new { c.Name, o.Total });
    }

    /// <summary>
    /// Example: Enhanced GroupBy across multiple sources
    /// </summary>
    public static void EnhancedGroupByExample()
    {
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId)
            // Multi-source GroupBy
            .GroupBy<Order>((c, o) => new { c.Category, o.Status })
            .Having(c => c.Category != null);
    }

    /// <summary>
    /// Example: Enhanced OrderBy with chained operations
    /// </summary>
    public static void EnhancedOrderByExample()
    {
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId)
            // Chained ordering
            .OrderBy(c => c.Name)
            .ThenByDescending<Order>((c, o) => o.CreatedDate)
            .ThenBy(c => c.Id);
    }

    /// <summary>
    /// Example: Better Expression Support
    /// </summary>
    public static void BetterExpressionSupportExample()
    {
        var statuses = new[] { "Active", "Premium" };
        
        var query = SqlBuilder.From<Customer>("c")
            // Enhanced expressions
            .Where(c => c.Name.StartsWith("A"))
            .Where(c => c.Age.HasValue && c.Age > 18)
            .Where(c => statuses.Contains(c.Status));
    }

    /// <summary>
    /// Example: Complex multi-source query with all features
    /// </summary>
    public static SqlQuery ComplexQueryExample()
    {
        return SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId, "o")
            .LeftJoin<OrderItem>((c, oi) => c.Id == oi.CustomerId, "oi")
            .Select((c, o) => new { 
                CustomerName = c.Name, 
                OrderTotal = o.Total,
                OrderCount = SqlFunction.Count()
            })
            .Where(c => c.IsActive)
            .Where<Order>((c, o) => o.CreatedDate >= DateTime.Today.AddDays(-30))
            .Where(c => c.Name.Contains("Corp"))
            .GroupBy<Order>((c, o) => new { c.Category, o.Status })
            .Having(c => c.Category != null)
            .OrderBy(c => c.Category)
            .ThenByDescending<Order>((c, o) => o.CreatedDate)
            .ThenBy(c => c.Name)
            .Skip(20)
            .Take(10)
            .Distinct()
            .Build();
    }

    /// <summary>
    /// Example: SQL Functions usage
    /// </summary>
    public static void SqlFunctionsExample()
    {
        var query = SqlBuilder.From<Order>("o")
            .GroupBy(o => o.Status)
            .Select(o => new {
                Status = o.Status,
                TotalCount = SqlFunction.Count(),
                TotalAmount = SqlFunction.Sum("[o].[Total]"),
                AverageAmount = SqlFunction.Avg("[o].[Total]"),
                MinAmount = SqlFunction.Min("[o].[Total]"),
                MaxAmount = SqlFunction.Max("[o].[Total]")
            })
            .Build();
    }
}

// Example entity classes for the documentation
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }
    public int? Age { get; set; }
    public string Status { get; set; } = default!;
    public string? Category { get; set; }
}

public class Order  
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; } = default!;
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}