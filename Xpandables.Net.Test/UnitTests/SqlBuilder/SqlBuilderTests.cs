//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

using FluentAssertions;
using Xpandables.Net.Repositories.SqlBuilder;

namespace Xpandables.Net.Test.UnitTests.SqlBuilder;

/// <summary>
/// Test entities for SqlBuilder tests.
/// </summary>
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

/// <summary>
/// Unit tests for SqlBuilder functionality.
/// </summary>
public sealed class SqlBuilderTests
{
    [Fact]
    public void From_ShouldCreateBuilderWithCorrectFromClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>()
            .Build();

        // Assert
        query.Sql.Should().Contain("FROM [Customer] AS");
        query.Sql.Should().Contain("c1"); // Auto-generated alias
    }

    [Fact]
    public void Select_ShouldGenerateCorrectSelectClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Select(c => new { c.Id, c.Name })
            .Build();

        // Assert
        query.Sql.Should().StartWith("SELECT [c].[Id], [c].[Name]");
    }

    [Fact]
    public void Where_ShouldGenerateCorrectWhereClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => c.IsActive)
            .Build();

        // Assert
        query.Sql.Should().Contain("WHERE [c].[IsActive] = @p0");
        query.Parameters.Should().ContainKey("@p0");
        query.Parameters["@p0"].Should().Be(true);
    }

    [Fact]
    public void InnerJoin_ShouldGenerateCorrectJoinClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId, "o")
            .Build();

        // Assert
        query.Sql.Should().Contain("INNER JOIN [Order] AS [o] ON [c].[Id] = [o].[CustomerId]");
    }

    [Fact]
    public void MultiSourceWhere_ShouldGenerateCorrectCondition()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId, "o")
            .Where<Order>((c, o) => c.IsActive && o.Total > 100)
            .Build();

        // Assert
        query.Sql.Should().Contain("WHERE ([c].[IsActive] = @p0) AND ([o].[Total] > @p1)");
        query.Parameters.Should().ContainKey("@p0");
        query.Parameters.Should().ContainKey("@p1");
    }

    [Fact]
    public void GroupBy_ShouldGenerateCorrectGroupByClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .GroupBy(c => c.Category)
            .Build();

        // Assert
        query.Sql.Should().Contain("GROUP BY [c].[Category]");
    }

    [Fact]
    public void MultiSourceGroupBy_ShouldGenerateCorrectGroupByClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId, "o")
            .GroupBy<Order>((c, o) => new { c.Category, o.Status })
            .Build();

        // Assert
        query.Sql.Should().Contain("GROUP BY [c].[Category], [o].[Status]");
    }

    [Fact]
    public void Having_ShouldGenerateCorrectHavingClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId, "o")
            .GroupBy(c => c.Category)
            .Having(c => c.Category != null)  // Simplified having clause for testing
            .Build();

        // Assert
        query.Sql.Should().Contain("GROUP BY [c].[Category]");
        query.Sql.Should().Contain("HAVING [c].[Category] IS NOT NULL");
    }

    [Fact]
    public void OrderBy_ShouldGenerateCorrectOrderByClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .OrderBy(c => c.Name)
            .Build();

        // Assert
        query.Sql.Should().Contain("ORDER BY [c].[Name] ASC");
    }

    [Fact]
    public void OrderByDescending_ShouldGenerateCorrectOrderByClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .OrderByDescending(c => c.Name)
            .Build();

        // Assert
        query.Sql.Should().Contain("ORDER BY [c].[Name] DESC");
    }

    [Fact]
    public void ThenBy_ShouldGenerateMultipleOrderByColumns()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId, "o")
            .OrderBy(c => c.Name)
            .ThenByDescending<Order>((c, o) => o.CreatedDate)
            .ThenBy(c => c.Id)
            .Build();

        // Assert
        query.Sql.Should().Contain("ORDER BY [c].[Name] ASC, [o].[CreatedDate] DESC, [c].[Id] ASC");
    }

    [Fact]
    public void Distinct_ShouldGenerateDistinctClause()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Select(c => c.Category)
            .Distinct()
            .Build();

        // Assert
        query.Sql.Should().StartWith("SELECT DISTINCT [c].[Category]");
    }

    [Fact]
    public void SkipTake_ShouldGeneratePaginationClauses()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .OrderBy(c => c.Id)
            .Skip(10)
            .Take(20)
            .Build();

        // Assert
        query.Sql.Should().Contain("OFFSET 10 ROWS");
        query.Sql.Should().Contain("FETCH NEXT 20 ROWS ONLY");
    }

    [Fact]
    public void ContainsMethod_ShouldGenerateLikeExpression()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => c.Name.Contains("John"))
            .Build();

        // Assert
        query.Sql.Should().Contain("WHERE [c].[Name] LIKE '%' + @p0 + '%'");
        query.Parameters["@p0"].Should().Be("John");
    }

    [Fact]
    public void StartsWithMethod_ShouldGenerateLikeExpression()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => c.Name.StartsWith("A"))
            .Build();

        // Assert
        query.Sql.Should().Contain("WHERE [c].[Name] LIKE @p0 + '%'");
        query.Parameters["@p0"].Should().Be("A");
    }

    [Fact]
    public void EndsWithMethod_ShouldGenerateLikeExpression()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => c.Name.EndsWith("son"))
            .Build();

        // Assert
        query.Sql.Should().Contain("WHERE [c].[Name] LIKE '%' + @p0");
        query.Parameters["@p0"].Should().Be("son");
    }

    [Fact]
    public void InOperator_ShouldGenerateInExpression()
    {
        // Arrange
        var statuses = new[] { "Active", "Premium" };

        // Act  
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => statuses.Contains(c.Status))
            .Build();

        // Assert
        query.Sql.Should().Contain("IN");
        query.Parameters.Should().ContainKey("@p0");
        query.Parameters.Should().ContainKey("@p1");
    }

    [Fact]
    public void NullComparison_ShouldGenerateIsNullExpression()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => c.Category == null)
            .Build();

        // Assert
        query.Sql.Should().Contain("WHERE [c].[Category] IS NULL");
    }

    [Fact]
    public void NotNullComparison_ShouldGenerateIsNotNullExpression()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => c.Category != null)
            .Build();

        // Assert
        query.Sql.Should().Contain("WHERE [c].[Category] IS NOT NULL");
    }

    [Fact]
    public void HasValueMethod_ShouldGenerateIsNotNullExpression()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => c.Age.HasValue && c.Age > 18)
            .Build();

        // Assert
        query.Sql.Should().Contain("WHERE ([c].[Age] IS NOT NULL) AND ([c].[Age] > @p0)");
        query.Parameters["@p0"].Should().Be(18);
    }

    [Fact]
    public void ComplexQuery_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .InnerJoin<Order>((c, o) => c.Id == o.CustomerId, "o")
            .Select((c, o) => new { c.Name, o.Total })
            .Where(c => c.IsActive)
            .Where<Order>((c, o) => o.CreatedDate >= DateTime.Today)
            .GroupBy<Order>((c, o) => new { c.Category, o.Status })
            .Having(c => c.Category != null)  // Simplified having clause
            .OrderBy(c => c.Category)
            .ThenByDescending<Order>((c, o) => o.CreatedDate)
            .Skip(20)
            .Take(10)
            .Build();

        // Assert
        query.Sql.Should().NotBeEmpty();
        query.Sql.Should().Contain("SELECT [c].[Name], [o].[Total]");
        query.Sql.Should().Contain("FROM [Customer] AS [c]");
        query.Sql.Should().Contain("INNER JOIN [Order] AS [o] ON [c].[Id] = [o].[CustomerId]");
        query.Sql.Should().Contain("WHERE ([c].[IsActive] = @p0) AND ([o].[CreatedDate] >= @p1)");
        query.Sql.Should().Contain("GROUP BY [c].[Category], [o].[Status]");
        query.Sql.Should().Contain("HAVING [c].[Category] IS NOT NULL");
        query.Sql.Should().Contain("ORDER BY [c].[Category] ASC, [o].[CreatedDate] DESC");
        query.Sql.Should().Contain("OFFSET 20 ROWS");
        query.Sql.Should().Contain("FETCH NEXT 10 ROWS ONLY");
        
        query.Parameters.Should().HaveCount(2);  // Updated count
    }
}