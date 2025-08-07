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

using FluentAssertions;
using Xpandables.Net.Repositories.Sql;

namespace Xpandables.Net.Test.UnitTests;

public sealed class StoredProcedureExtensionsUnitTest
{
    [Fact]
    public void WithParameter_WithValidParameters_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithParameter("testParam", 123);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithParameter_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = null!;

        // Act & Assert
        Action act = () => builder.WithParameter("testParam", 123);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithParameters_WithValidDictionary_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        var parameters = new Dictionary<string, object?>
        {
            ["param1"] = 123,
            ["param2"] = "test",
            ["param3"] = null
        };

        // Act
        StoredProcedureBuilder result = builder.WithParameters(parameters);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithParameters_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = null!;
        var parameters = new Dictionary<string, object?> { ["param1"] = 123 };

        // Act & Assert
        Action act = () => builder.WithParameters(parameters);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithParameters_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithParameters((IDictionary<string, object?>)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithCommonOutputs_WithDefaults_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithCommonOutputs();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void WithCommonOutputs_WithSpecifiedParameters_ShouldReturnBuilder(bool includeRecordCount, bool includeErrorMessage)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithCommonOutputs(includeRecordCount, includeErrorMessage);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithCommonOutputs_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = null!;

        // Act & Assert
        Action act = () => builder.WithCommonOutputs();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithPagination_WithValidParameters_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithPagination(0, 20);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithPagination_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = null!;

        // Act & Assert
        Action act = () => builder.WithPagination(0, 20);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(-1, 20)]
    [InlineData(0, 0)]
    [InlineData(0, -1)]
    public void WithPagination_WithInvalidParameters_ShouldThrowArgumentOutOfRangeException(int pageIndex, int pageSize)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithPagination(pageIndex, pageSize);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WithSearch_WithValidParameters_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithSearch("test", "field1", "field2");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithSearch_WithNullSearchTerm_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithSearch(null);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithSearch_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = null!;

        // Act & Assert
        Action act = () => builder.WithSearch("test");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithDateRange_WithValidDates_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        DateTime startDate = DateTime.Today.AddDays(-30);
        DateTime endDate = DateTime.Today;

        // Act
        StoredProcedureBuilder result = builder.WithDateRange(startDate, endDate);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithDateRange_WithNullDates_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithDateRange();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithDateRange_WithCustomPrefix_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        DateTime startDate = DateTime.Today.AddDays(-30);

        // Act
        StoredProcedureBuilder result = builder.WithDateRange(startDate, null, "Created");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithDateRange_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = null!;

        // Act & Assert
        Action act = () => builder.WithDateRange();
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void WithDateRange_WithInvalidPrefix_ShouldThrowArgumentException(string? prefix)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithDateRange(DateTime.Today, null, prefix!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithStandardOptions_WithDefaults_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithStandardOptions();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithStandardOptions_WithCustomValues_ShouldReturnBuilder()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act
        StoredProcedureBuilder result = builder.WithStandardOptions(10, 5, 3);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithStandardOptions_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        StoredProcedureBuilder builder = null!;

        // Act & Assert
        Action act = () => builder.WithStandardOptions();
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0, 3, 2)]
    [InlineData(-1, 3, 2)]
    [InlineData(5, -1, 2)]
    [InlineData(5, 3, -1)]
    public void WithStandardOptions_WithInvalidParameters_ShouldThrowArgumentOutOfRangeException(int timeoutMinutes, int maxRetries, int retryDelaySeconds)
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");

        // Act & Assert
        Action act = () => builder.WithStandardOptions(timeoutMinutes, maxRetries, retryDelaySeconds);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ExtensionMethods_ShouldAllowChaining()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        var parameters = new Dictionary<string, object?> { ["id"] = 123 };

        // Act
        StoredProcedureBuilder result = builder
            .WithParameter("singleParam", "value")
            .WithParameters(parameters)
            .WithCommonOutputs()
            .WithPagination(0, 25)
            .WithSearch("test search", "name", "description")
            .WithDateRange(DateTime.Today.AddDays(-7), DateTime.Today)
            .WithStandardOptions();

        // Assert
        result.Should().BeSameAs(builder);
    }
}