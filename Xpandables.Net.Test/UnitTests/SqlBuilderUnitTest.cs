/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/

using System.ComponentModel.DataAnnotations.Schema;

using FluentAssertions;

using Xpandables.Net.Sql;

namespace Xpandables.Net.Test.UnitTests;

public sealed class SqlBuilderUnitTest
{
    #region Test Entities

    [Table("Users")]
    public sealed class User
    {
        public int Id { get; set; }
        [Column("FirstName")]
        public string FirstName { get; set; } = string.Empty;
        [Column("LastName")]
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
        public int DepartmentId { get; set; }
    }

    [Table("Orders")]
    public sealed class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    [Table("Departments")]
    public sealed class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region SELECT Tests

    [Fact]
    public void BasicSelect_ShouldGenerateCorrectSqlAndParameters()
    {
        // Act
        var query = SqlBuilder.From<User>()
            .Select(u => new { u.Id, u.LastName, u.FirstName, u.BirthDate })
            .Where(u => u.IsActive)
            .OrderBy(u => u.Id)
            .Skip(10)
            .Take(5);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("SELECT [u].[Id] AS [Id], [u].[LastName] AS [LastName], [u].[FirstName] AS [FirstName], [u].[BirthDate] AS [BirthDate]");
        result.Sql.Should().Contain("FROM [Users] AS [u]");
        result.Sql.Should().Contain("WHERE ([u].[IsActive] = @p0)");
        result.Sql.Should().Contain("ORDER BY [u].[Id] ASC");
        result.Sql.Should().Contain("OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY");

        result.Parameters.Should().HaveCount(1);
        result.Parameters.First().ParameterName.Should().Be("@p0");
        result.Parameters.First().Value.Should().Be(true);
    }

    [Fact]
    public void SelectWithoutColumns_ShouldGenerateSelectAll()
    {
        // Act
        var query = SqlBuilder.From<User>();
        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("SELECT [u].*");
        result.Sql.Should().Contain("FROM [Users] AS [u]");
    }

    [Fact]
    public void SelectWithJoin_ShouldGenerateCorrectSqlWithAliases()
    {
        // Act
        var query = SqlBuilder.From<User>()
            .Select<Order>((u, o) => new { u.Id, u.LastName, u.FirstName, o.Date, o.Amount })
            .InnerJoin<Order>((u, o) => u.Id == o.UserId)
            .Where<Order>((u, o) => o.Amount > 100)
            .OrderByDescending(u => u.Id)
            .Skip(10)
            .Take(5);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("SELECT [u].[Id] AS [Id], [u].[LastName] AS [LastName], [u].[FirstName] AS [FirstName], [o].[Date] AS [Date], [o].[Amount] AS [Amount]");
        result.Sql.Should().Contain("FROM [Users] AS [u]");
        result.Sql.Should().Contain("INNER JOIN [Orders] AS [o] ON ([u].[Id] = [o].[UserId])");
        result.Sql.Should().Contain("WHERE ([o].[Amount] > @p0)");
        result.Sql.Should().Contain("ORDER BY [u].[Id] DESC");
        result.Sql.Should().Contain("OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY");

        result.Parameters.Should().HaveCount(1);
        result.Parameters.First().Value.Should().Be(100);
    }

    [Fact]
    public void SelectWithMultipleJoins_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.From<User>()
            .Select<Order, Department>((u, o, d) => new { u.FirstName, u.LastName, o.Amount, d.Name })
            .InnerJoin<Order>((u, o) => u.Id == o.UserId)
            .LeftJoin<Department>((u, d) => u.DepartmentId == d.Id)
            .Where(u => u.IsActive);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("INNER JOIN [Orders] AS [o] ON ([u].[Id] = [o].[UserId])");
        result.Sql.Should().Contain("LEFT JOIN [Departments] AS [d] ON ([u].[DepartmentId] = [d].[Id])");
    }

    [Fact]
    public void SelectWithRightJoin_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.From<User>()
            .RightJoin<Order>((u, o) => u.Id == o.UserId);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("RIGHT JOIN [Orders] AS [o] ON ([u].[Id] = [o].[UserId])");
    }

    [Fact]
    public void SelectWithGroupByAndHaving_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.From<User>()
            .Select<Order>((u, o) => new { u.DepartmentId, TotalAmount = o.Amount })
            .InnerJoin<Order>((u, o) => u.Id == o.UserId)
            .GroupBy<Order>((u, o) => u.DepartmentId)
            .Having<Order>((u, o) => o.Amount > 1000);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("GROUP BY [u].[DepartmentId]");
        result.Sql.Should().Contain("HAVING ([o].[Amount] > @p0)");

        result.Parameters.Should().HaveCount(1);
        result.Parameters.First().Value.Should().Be(1000);
    }

    [Fact]
    public void SelectWithCte_ShouldGenerateCorrectSql()
    {
        // Arrange
        var cteQuery = SqlBuilder.From<Order>()
            .Select(o => new { o.UserId, o.Amount })
            .GroupBy(o => o.UserId)
            .Having(o => o.Amount > 1000);

        // Act
        var mainQuery = SqlBuilder.From<User>()
            .WithCte("UserTotals", cteQuery)
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName);

        var result = mainQuery.Build();

        // Assert
        result.Sql.Should().StartWith("WITH [UserTotals] AS (");
        result.Sql.Should().Contain("SELECT [u].[Id] AS [Id], [u].[FirstName] AS [FirstName], [u].[LastName] AS [LastName]");
        result.Sql.Should().Contain("FROM [Users] AS [u]");
        result.Sql.Should().Contain("WHERE ([u].[IsActive] = @p");
        result.Sql.Should().Contain("ORDER BY [u].[LastName] ASC");
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(5, 15)]
    [InlineData(100, 25)]
    public void SelectWithPagination_ShouldGenerateCorrectOffsetFetch(int skip, int take)
    {
        // Act
        var query = SqlBuilder.From<User>()
            .OrderBy(u => u.Id)
            .Skip(skip)
            .Take(take);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain($"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY");
    }

    [Fact]
    public void SelectWithOnlyTake_ShouldGenerateOffsetZero()
    {
        // Act
        var query = SqlBuilder.From<User>()
            .OrderBy(u => u.Id)
            .Take(5);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY");
    }

    #endregion

    #region INSERT Tests

    [Fact]
    public void InsertWithValues_ShouldGenerateCorrectSql()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var birthDate = new DateTime(1990, 1, 1);
        var isActive = true;

        // Act
        var query = SqlBuilder.Insert<User>()
            .Values(u => new { FirstName = firstName, LastName = lastName, BirthDate = birthDate, IsActive = isActive });

        var result = query.Build();

        // Assert
        result.Sql.Should().Be("INSERT INTO [Users] ([FirstName], [LastName], [BirthDate], [IsActive])\r\nVALUES (@p0, @p1, @p2, @p3)");

        result.Parameters.Should().HaveCount(4);
        result.Parameters.ElementAt(0).Value.Should().Be(firstName);
        result.Parameters.ElementAt(1).Value.Should().Be(lastName);
        result.Parameters.ElementAt(2).Value.Should().Be(birthDate);
        result.Parameters.ElementAt(3).Value.Should().Be(isActive);
    }

    [Fact]
    public void InsertMultipleEntities_ShouldGenerateCorrectSql()
    {
        // Arrange
        var users = new List<User>
        {
            new() { FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990, 1, 1), IsActive = true },
            new() { FirstName = "Jane", LastName = "Smith", BirthDate = new DateTime(1985, 5, 15), IsActive = true }
        };

        // Act
        var query = SqlBuilder.Insert<User>()
            .Values(users);

        var result = query.Build();

        // Assert
        result.Sql.Should().StartWith("INSERT INTO [Users]");
        result.Sql.Should().Contain("VALUES (@p0, @p1, @p2, @p3), (@p4, @p5, @p6, @p7)");
        result.Parameters.Should().HaveCount(8);
    }

    [Fact]
    public void InsertWithoutValues_ShouldThrowInvalidOperationException()
    {
        // Act
        var query = SqlBuilder.Insert<User>();

        // Assert
        var act = () => query.Build();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No columns specified for INSERT operation.");
    }

    #endregion

    #region UPDATE Tests

    [Fact]
    public void UpdateWithSet_ShouldGenerateCorrectSql()
    {
        // Arrange
        var firstName = "UpdatedJohn";
        var lastName = "UpdatedDoe";

        // Act
        var query = SqlBuilder.Update<User>()
            .Set(u => new { FirstName = firstName, LastName = lastName })
            .Where(u => u.Id == 1);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("UPDATE [u] SET [FirstName] = @p0, [LastName] = @p1");
        result.Sql.Should().Contain("FROM [Users] AS [u]");
        result.Sql.Should().Contain("WHERE ([u].[Id] = @p2)");

        result.Parameters.Should().HaveCount(3);
        result.Parameters.ElementAt(0).Value.Should().Be(firstName);
        result.Parameters.ElementAt(1).Value.Should().Be(lastName);
        result.Parameters.ElementAt(2).Value.Should().Be(1);
    }

    [Fact]
    public void UpdateWithoutSet_ShouldThrowInvalidOperationException()
    {
        // Act
        var query = SqlBuilder.Update<User>();

        // Assert
        var act = () => query.Build();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No SET clauses specified for UPDATE operation.");
    }

    [Fact]
    public void UpdateWithoutWhere_ShouldGenerateUpdateWithoutWhereClause()
    {
        // Act
        var query = SqlBuilder.Update<User>()
            .Set(u => new { IsActive = false });

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("UPDATE [u] SET [IsActive] = @p0");
        result.Sql.Should().Contain("FROM [Users] AS [u]");
        result.Sql.Should().NotContain("WHERE");
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public void DeleteWithWhere_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.Delete<User>()
            .Where(u => !u.IsActive);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("DELETE FROM [Users]");
        result.Sql.Should().Contain("WHERE (NOT [u].[IsActive])");
    }

    [Fact]
    public void DeleteWithMultipleConditions_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.Delete<User>()
            .Where(u => !u.IsActive)
            .Where(u => u.BirthDate < DateTime.Now.AddYears(-65));

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("DELETE FROM [Users]");
        result.Sql.Should().Contain("WHERE (NOT [u].[IsActive]) AND ([u].[BirthDate] < @p0)");
        result.Parameters.Should().HaveCount(1);
    }

    [Fact]
    public void DeleteWithoutWhere_ShouldGenerateDeleteAll()
    {
        // Act
        var query = SqlBuilder.Delete<User>();
        var result = query.Build();

        // Assert
        result.Sql.Should().Be("DELETE FROM [Users]");
        result.Sql.Should().NotContain("WHERE");
    }

    #endregion

    #region Stored Procedure Tests

    [Fact]
    public void StoredProcedureWithParameters_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.StoredProcedure("GetUsersByAge")
            .AddParameter("MinAge", 18)
            .AddParameter("MaxAge", 65)
            .AddParameter("IsActive", true);

        var result = query.Build();

        // Assert
        result.Sql.Should().Be("EXEC [GetUsersByAge] @MinAge, @MaxAge, @IsActive");

        result.Parameters.Should().HaveCount(3);
        result.Parameters.Should().Contain(p => p.ParameterName == "@MinAge" && p.Value.Equals(18));
        result.Parameters.Should().Contain(p => p.ParameterName == "@MaxAge" && p.Value.Equals(65));
        result.Parameters.Should().Contain(p => p.ParameterName == "@IsActive" && p.Value.Equals(true));
    }

    [Fact]
    public void StoredProcedureWithoutParameters_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.StoredProcedure("GetAllUsers");
        var result = query.Build();

        // Assert
        result.Sql.Should().Be("EXEC [GetAllUsers]");
        result.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void StoredProcedureWithParameterPrefix_ShouldHandleCorrectly()
    {
        // Act
        var query = SqlBuilder.StoredProcedure("TestProcedure")
            .AddParameter("@ParamWithPrefix", "value")
            .AddParameter("ParamWithoutPrefix", "value2");

        var result = query.Build();

        // Assert
        result.Sql.Should().Be("EXEC [TestProcedure] @ParamWithPrefix, @ParamWithoutPrefix");
        result.Parameters.Should().HaveCount(2);
        result.Parameters.Should().Contain(p => p.ParameterName == "@ParamWithPrefix");
        result.Parameters.Should().Contain(p => p.ParameterName == "@ParamWithoutPrefix");
    }

    #endregion

    #region String Method Tests

    [Fact]
    public void WhereWithStringContains_ShouldGenerateLikeWithPercents()
    {
        // Arrange
        var searchTerm = "John";

        // Act
        var query = SqlBuilder.From<User>()
            .Where(u => u.FirstName.Contains(searchTerm));

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("WHERE ([u].[FirstName] LIKE @p0)");
        result.Parameters.Should().HaveCount(1);
        result.Parameters.First().Value.Should().Be($"%{searchTerm}%");
    }

    [Fact]
    public void WhereWithStringStartsWith_ShouldGenerateLikeWithTrailingPercent()
    {
        // Arrange
        var prefix = "Jo";

        // Act
        var query = SqlBuilder.From<User>()
            .Where(u => u.FirstName.StartsWith(prefix));

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("WHERE ([u].[FirstName] LIKE @p0)");
        result.Parameters.Should().HaveCount(1);
        result.Parameters.First().Value.Should().Be($"{prefix}%");
    }

    [Fact]
    public void WhereWithStringEndsWith_ShouldGenerateLikeWithLeadingPercent()
    {
        // Arrange
        var suffix = "hn";

        // Act
        var query = SqlBuilder.From<User>()
            .Where(u => u.FirstName.EndsWith(suffix));

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("WHERE ([u].[FirstName] LIKE @p0)");
        result.Parameters.Should().HaveCount(1);
        result.Parameters.First().Value.Should().Be($"%{suffix}");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void SkipWithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => SqlBuilder.From<User>().Skip(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TakeWithZeroValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => SqlBuilder.From<User>().Take(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TakeWithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => SqlBuilder.From<User>().Take(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void StoredProcedureWithNullOrEmptyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act1 = () => SqlBuilder.StoredProcedure(string.Empty);
        act1.Should().Throw<ArgumentException>();

        var act2 = () => SqlBuilder.StoredProcedure("   ");
        act2.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void SelectWithComplexExpression_ShouldGenerateCorrectSql()
    {
        // Act
        var query = SqlBuilder.From<User>()
            .Where(u => u.IsActive && u.BirthDate > DateTime.Now.AddYears(-30) && u.FirstName.Length > 2);

        var result = query.Build();

        // Assert
        result.Sql.Should().Contain("WHERE");
        result.Sql.Should().Contain("AND");
        result.Parameters.Should().HaveCount(3); // IsActive, BirthDate comparison, Length comparison
    }

    [Fact]
    public void SelectWithLocalVariableInWhere_ShouldParameterizeCorrectly()
    {
        // Arrange
        var minAge = 18;
        var department = "IT";

        // Act
        var query = SqlBuilder.From<User>()
            .Where(u => u.Id > minAge && u.FirstName == department);

        var result = query.Build();

        // Assert
        result.Parameters.Should().HaveCount(2);
        result.Parameters.Should().Contain(p => p.Value.Equals(minAge));
        result.Parameters.Should().Contain(p => p.Value.Equals(department));
    }

    #endregion
}