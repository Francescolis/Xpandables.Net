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

using System.Data;
using System.Data.Common;
using FluentAssertions;
using Moq;
using Xpandables.Net.Repositories.Sql;

namespace Xpandables.Net.Test.UnitTests;

public sealed class StoredProcedureBuilderUnitTest
{
    [Fact]
    public void Constructor_WithValidProcedureName_ShouldCreateBuilder()
    {
        // Arrange
        const string procedureName = "TestProcedure";

        // Act
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure(procedureName);

        // Assert
        builder.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidProcedureName_ShouldThrowArgumentException(string? procedureName)
    {
        // Act & Assert
        Action act = () => SqlBuilder.StoredProcedure(procedureName!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithInputParameter_WithValidParameters_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithInputParameter("testParam", 123);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void WithInputParameter_WithInvalidParameterName_ShouldThrowArgumentException(string? parameterName)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithInputParameter(parameterName!, 123);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithOutputParameter_WithValidParameters_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithOutputParameter("outputParam", SqlDbType.Int);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void WithOutputParameter_WithInvalidParameterName_ShouldThrowArgumentException(string? parameterName)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithOutputParameter(parameterName!, SqlDbType.Int);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithInputOutputParameter_WithValidParameters_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithInputOutputParameter("ioParam", 123, SqlDbType.Int);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void WithInputOutputParameter_WithInvalidParameterName_ShouldThrowArgumentException(string? parameterName)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithInputOutputParameter(parameterName!, 123, SqlDbType.Int);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithReturnValue_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithReturnValue();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithReturnValue_CalledMultipleTimes_ShouldReplaceExistingReturnValue()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure")
            .WithReturnValue();

        // Act
        StoredProcedureBuilder result = builder.WithReturnValue();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithTableParameter_WithValidParameters_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        List<TestTableType> tableData = [new TestTableType { Id = 1, Name = "Test" }];

        // Act
        StoredProcedureBuilder result = builder.WithTableParameter("tableParam", tableData, "dbo.TestTableType");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Theory]
    [InlineData("", "dbo.TestTableType")]
    [InlineData(" ", "dbo.TestTableType")]
    [InlineData(null, "dbo.TestTableType")]
    [InlineData("tableParam", "")]
    [InlineData("tableParam", " ")]
    [InlineData("tableParam", null)]
    public void WithTableParameter_WithInvalidParameters_ShouldThrowArgumentException(string? parameterName, string? typeName)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        List<TestTableType> tableData = [new TestTableType { Id = 1, Name = "Test" }];

        // Act & Assert
        Action act = () => builder.WithTableParameter(parameterName!, tableData, typeName!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithTableParameter_WithNullValues_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithTableParameter("tableParam", (IEnumerable<TestTableType>)null!, "dbo.TestTableType");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithStructuredParameter_WithValidParameters_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        DataTable table = new();
        table.Columns.Add("Id", typeof(int));
        table.Rows.Add(1);

        // Act
        StoredProcedureBuilder result = builder.WithStructuredParameter("structParam", table, "dbo.TestTableType");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithParameters_WithValidObject_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        var parameters = new { Id = 123, Name = "Test" };

        // Act
        StoredProcedureBuilder result = builder.WithParameters(parameters);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithParameters_WithNullObject_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithParameters<object>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithParametersFromExpression_WithValidExpression_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        int id = 123;
        string name = "Test";

        // Act
        StoredProcedureBuilder result = builder.WithParametersFromExpression(() => new { id, name });

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithParametersFromExpression_WithNullExpression_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithParametersFromExpression<object>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithParametersFromExpression_WithInvalidExpression_ShouldThrowArgumentException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithParametersFromExpression(() => "not a constructor");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithTimeout_WithValidTimeout_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        TimeSpan timeout = TimeSpan.FromMinutes(5);

        // Act
        StoredProcedureBuilder result = builder.WithTimeout(timeout);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithTimeout_WithNegativeTimeout_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        TimeSpan negativeTimeout = TimeSpan.FromMinutes(-1);

        // Act & Assert
        Action act = () => builder.WithTimeout(negativeTimeout);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WithRetryPolicy_WithValidParameters_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithRetryPolicy(3, TimeSpan.FromSeconds(2));

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(3, -1)]
    public void WithRetryPolicy_WithInvalidParameters_ShouldThrowArgumentOutOfRangeException(int maxRetries, int delaySeconds)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithRetryPolicy(maxRetries, TimeSpan.FromSeconds(delaySeconds));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WithConnection_WithValidConnection_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        Mock<DbConnection> connectionMock = new();

        // Act
        StoredProcedureBuilder result = builder.WithConnection(connectionMock.Object);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithConnection_WithNullConnection_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithConnection(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithTransaction_WithValidTransaction_ShouldReturnSelf()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        Mock<DbTransaction> transactionMock = new();

        // Act
        StoredProcedureBuilder result = builder.WithTransaction(transactionMock.Object);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithTransaction_WithNullTransaction_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithTransaction(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithoutConnection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Func<Task> act = async () => await builder.ExecuteAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No connection specified for stored procedure execution.");
    }

    [Fact]
    public async Task ExecuteScalarAsync_WithoutConnection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Func<Task> act = async () => await builder.ExecuteScalarAsync<int>();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No connection specified for stored procedure execution.");
    }

    [Fact]
    public async Task ExecuteDataSetAsync_WithoutConnection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Func<Task> act = async () => await builder.ExecuteDataSetAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No connection specified for stored procedure execution.");
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_WithoutConnection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Func<Task> act = async () => await builder.ExecuteNonQueryAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No connection specified for stored procedure execution.");
    }

    [Fact]
    public void FluentInterface_ShouldAllowMethodChaining()
    {
        // Arrange & Act
        StoredProcedureBuilder result = SqlBuilder.StoredProcedure("TestProcedure")
            .WithInputParameter("param1", 123)
            .WithOutputParameter("output1", SqlDbType.Int)
            .WithInputOutputParameter("ioParam", 456, SqlDbType.Int)
            .WithReturnValue()
            .WithTimeout(TimeSpan.FromMinutes(5))
            .WithRetryPolicy(3, TimeSpan.FromSeconds(2));

        // Assert
        result.Should().NotBeNull();
    }

    private sealed class TestTableType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}