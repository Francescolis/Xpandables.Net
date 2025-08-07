//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

using FluentAssertions;
using Xpandables.Net.Repositories.SqlBuilder;

namespace Xpandables.Net.Test.UnitTests.SqlBuilder;

/// <summary>
/// Basic unit tests for SqlBuilder functionality.
/// </summary>
public sealed class SqlBuilderBasicTests
{
    [Fact]
    public void From_ShouldCreateBuilder()
    {
        // Act
        var builder = SqlBuilder.From<Customer>();

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void BasicQuery_ShouldGenerateValidSql()
    {
        // Act
        var query = SqlBuilder.From<Customer>("c")
            .Where(c => c.Id == 1)
            .Build();

        // Assert  
        query.Should().NotBeNull();
        query.Sql.Should().NotBeEmpty();
        query.Sql.Should().Contain("SELECT");
        query.Sql.Should().Contain("FROM");
        query.Sql.Should().Contain("WHERE");
        query.Parameters.Should().NotBeEmpty();
    }

    [Fact]
    public void TableModel_ShouldCreateCorrectReference()
    {
        // Act
        var table = TableModel.Create<Customer>("c");

        // Assert
        table.Should().NotBeNull();
        table.Name.Should().Be("Customer");
        table.Alias.Should().Be("c");
        table.TableReference.Should().Be("[Customer] AS [c]");
    }

    [Fact] 
    public void OrderByModel_ShouldGenerateCorrectSql()
    {
        // Act
        var orderBy = OrderByModel.Create("[c].[Name]", OrderDirection.Ascending, 0);

        // Assert
        orderBy.ToSql().Should().Be("[c].[Name] ASC");
    }

    [Fact]
    public void JoinModel_ShouldGenerateCorrectSql()
    {
        // Arrange
        var table = TableModel.Create<Order>("o");
        
        // Act
        var join = JoinModel.Create(JoinType.Inner, table, "[c].[Id] = [o].[CustomerId]");

        // Assert
        join.ToSql().Should().Be("INNER JOIN [Order] AS [o] ON [c].[Id] = [o].[CustomerId]");
    }

    [Fact]
    public void SqlFunction_Count_ShouldGenerateCorrectExpression()
    {
        // Act
        var result = SqlFunction.Count();

        // Assert
        result.Should().Be("COUNT(*)");
    }

    [Fact]
    public void SqlFunction_Sum_ShouldGenerateCorrectExpression()
    {
        // Act
        var result = SqlFunction.Sum("[o].[Total]");

        // Assert
        result.Should().Be("SUM([o].[Total])");
    }
}