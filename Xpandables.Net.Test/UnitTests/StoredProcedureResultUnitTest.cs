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
using FluentAssertions;
using Xpandables.Net.Repositories.Sql;

namespace Xpandables.Net.Test.UnitTests;

public sealed class StoredProcedureResultUnitTest
{
    [Fact]
    public void Success_WithValidParameters_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var outputParams = new Dictionary<string, object?> { ["output1"] = 123 };
        object returnValue = 456;
        DataSet dataSet = CreateTestDataSet();
        int rowsAffected = 10;
        TimeSpan executionTime = TimeSpan.FromMilliseconds(500);

        // Act
        StoredProcedureResult result = StoredProcedureResult.Success(outputParams, returnValue, dataSet, rowsAffected, executionTime);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.HasErrors.Should().BeFalse();
        result.OutputParameters.Should().ContainKey("output1").WhoseValue.Should().Be(123);
        result.ReturnValue.Should().Be(456);
        result.DataSet.Should().BeSameAs(dataSet);
        result.RowsAffected.Should().Be(10);
        result.ExecutionTime.Should().Be(TimeSpan.FromMilliseconds(500));
        result.ErrorMessages.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithNullParameters_ShouldCreateValidResult()
    {
        // Arrange
        IDictionary<string, object?> outputParams = null!;
        object? returnValue = null;
        DataSet? dataSet = null;
        int rowsAffected = 0;
        TimeSpan executionTime = TimeSpan.Zero;

        // Act
        StoredProcedureResult result = StoredProcedureResult.Success(outputParams, returnValue, dataSet, rowsAffected, executionTime);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.HasErrors.Should().BeFalse();
        result.OutputParameters.Should().BeEmpty();
        result.ReturnValue.Should().BeNull();
        result.DataSet.Should().BeNull();
        result.RowsAffected.Should().Be(0);
        result.ExecutionTime.Should().Be(TimeSpan.Zero);
        result.ErrorMessages.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithErrorMessages_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessages = new List<string> { "Error 1", "Error 2" };
        TimeSpan executionTime = TimeSpan.FromMilliseconds(100);

        // Act
        StoredProcedureResult result = StoredProcedureResult.Failure(errorMessages, executionTime);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
        result.OutputParameters.Should().BeEmpty();
        result.ReturnValue.Should().BeNull();
        result.DataSet.Should().BeNull();
        result.RowsAffected.Should().Be(0);
        result.ExecutionTime.Should().Be(TimeSpan.FromMilliseconds(100));
        result.ErrorMessages.Should().HaveCount(2)
            .And.Contain("Error 1")
            .And.Contain("Error 2");
    }

    [Fact]
    public void Failure_WithNullErrorMessages_ShouldCreateValidResult()
    {
        // Arrange
        IList<string> errorMessages = null!;
        TimeSpan executionTime = TimeSpan.FromMilliseconds(100);

        // Act
        StoredProcedureResult result = StoredProcedureResult.Failure(errorMessages, executionTime);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // No errors means success
        result.HasErrors.Should().BeFalse();
        result.ErrorMessages.Should().BeEmpty();
    }

    [Fact]
    public void FirstTable_WithDataSet_ShouldReturnFirstTable()
    {
        // Arrange
        DataSet dataSet = CreateTestDataSet();
        StoredProcedureResult result = StoredProcedureResult.Success(
            new Dictionary<string, object?>(),
            null,
            dataSet,
            0,
            TimeSpan.Zero);

        // Act
        DataTable? firstTable = result.FirstTable;

        // Assert
        firstTable.Should().NotBeNull();
        firstTable!.TableName.Should().Be("Table0");
        firstTable.Rows.Should().HaveCount(2);
    }

    [Fact]
    public void FirstTable_WithEmptyDataSet_ShouldReturnNull()
    {
        // Arrange
        DataSet dataSet = new();
        StoredProcedureResult result = StoredProcedureResult.Success(
            new Dictionary<string, object?>(),
            null,
            dataSet,
            0,
            TimeSpan.Zero);

        // Act
        DataTable? firstTable = result.FirstTable;

        // Assert
        firstTable.Should().BeNull();
    }

    [Fact]
    public void FirstTable_WithNullDataSet_ShouldReturnNull()
    {
        // Arrange
        StoredProcedureResult result = StoredProcedureResult.Success(
            new Dictionary<string, object?>(),
            null,
            null,
            0,
            TimeSpan.Zero);

        // Act
        DataTable? firstTable = result.FirstTable;

        // Assert
        firstTable.Should().BeNull();
    }

    [Fact]
    public void FirstRow_WithDataSet_ShouldReturnFirstRow()
    {
        // Arrange
        DataSet dataSet = CreateTestDataSet();
        StoredProcedureResult result = StoredProcedureResult.Success(
            new Dictionary<string, object?>(),
            null,
            dataSet,
            0,
            TimeSpan.Zero);

        // Act
        DataRow? firstRow = result.FirstRow;

        // Assert
        firstRow.Should().NotBeNull();
        firstRow!["Id"].Should().Be(1);
        firstRow["Name"].Should().Be("Test1");
    }

    [Fact]
    public void FirstRow_WithEmptyTable_ShouldReturnNull()
    {
        // Arrange
        DataSet dataSet = new();
        DataTable table = dataSet.Tables.Add("TestTable");
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));

        StoredProcedureResult result = StoredProcedureResult.Success(
            new Dictionary<string, object?>(),
            null,
            dataSet,
            0,
            TimeSpan.Zero);

        // Act
        DataRow? firstRow = result.FirstRow;

        // Assert
        firstRow.Should().BeNull();
    }

    [Fact]
    public void GetOutputParameter_WithExistingParameter_ShouldReturnTypedValue()
    {
        // Arrange
        var outputParams = new Dictionary<string, object?> 
        { 
            ["intParam"] = 123,
            ["stringParam"] = "test",
            ["nullParam"] = null
        };

        StoredProcedureResult result = StoredProcedureResult.Success(outputParams, null, null, 0, TimeSpan.Zero);

        // Act & Assert
        result.GetOutputParameter<int>("intParam").Should().Be(123);
        result.GetOutputParameter<string>("stringParam").Should().Be("test");
        result.GetOutputParameter<string>("nullParam").Should().BeNull();
    }

    [Fact]
    public void GetOutputParameter_WithNonExistentParameter_ShouldReturnDefault()
    {
        // Arrange
        var outputParams = new Dictionary<string, object?>();
        StoredProcedureResult result = StoredProcedureResult.Success(outputParams, null, null, 0, TimeSpan.Zero);

        // Act & Assert
        result.GetOutputParameter<int>("nonExistent").Should().Be(0);
        result.GetOutputParameter<string>("nonExistent").Should().BeNull();
    }

    [Fact]
    public void GetOutputParameter_WithTypeConversion_ShouldConvertValue()
    {
        // Arrange
        var outputParams = new Dictionary<string, object?> { ["param"] = "123" };
        StoredProcedureResult result = StoredProcedureResult.Success(outputParams, null, null, 0, TimeSpan.Zero);

        // Act & Assert
        result.GetOutputParameter<int>("param").Should().Be(123);
    }

    [Fact]
    public void GetOutputParameter_WithInvalidConversion_ShouldReturnDefault()
    {
        // Arrange
        var outputParams = new Dictionary<string, object?> { ["param"] = "not a number" };
        StoredProcedureResult result = StoredProcedureResult.Success(outputParams, null, null, 0, TimeSpan.Zero);

        // Act & Assert
        result.GetOutputParameter<int>("param").Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GetOutputParameter_WithInvalidParameterName_ShouldThrowArgumentException(string? parameterName)
    {
        // Arrange
        StoredProcedureResult result = StoredProcedureResult.Success(new Dictionary<string, object?>(), null, null, 0, TimeSpan.Zero);

        // Act & Assert
        Action act = () => result.GetOutputParameter<int>(parameterName!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetReturnValue_WithTypedReturnValue_ShouldReturnValue()
    {
        // Arrange
        StoredProcedureResult result = StoredProcedureResult.Success(new Dictionary<string, object?>(), 456, null, 0, TimeSpan.Zero);

        // Act & Assert
        result.GetReturnValue<int>().Should().Be(456);
    }

    [Fact]
    public void GetReturnValue_WithNullReturnValue_ShouldReturnDefault()
    {
        // Arrange
        StoredProcedureResult result = StoredProcedureResult.Success(new Dictionary<string, object?>(), null, null, 0, TimeSpan.Zero);

        // Act & Assert
        result.GetReturnValue<int>().Should().Be(0);
        result.GetReturnValue<string>().Should().BeNull();
    }

    [Fact]
    public void GetReturnValue_WithTypeConversion_ShouldConvertValue()
    {
        // Arrange
        StoredProcedureResult result = StoredProcedureResult.Success(new Dictionary<string, object?>(), "789", null, 0, TimeSpan.Zero);

        // Act & Assert
        result.GetReturnValue<int>().Should().Be(789);
    }

    [Fact]
    public void GetReturnValue_WithInvalidConversion_ShouldReturnDefault()
    {
        // Arrange
        StoredProcedureResult result = StoredProcedureResult.Success(new Dictionary<string, object?>(), "not a number", null, 0, TimeSpan.Zero);

        // Act & Assert
        result.GetReturnValue<int>().Should().Be(0);
    }

    private static DataSet CreateTestDataSet()
    {
        DataSet dataSet = new();
        
        DataTable table1 = dataSet.Tables.Add("Table0");
        table1.Columns.Add("Id", typeof(int));
        table1.Columns.Add("Name", typeof(string));
        table1.Rows.Add(1, "Test1");
        table1.Rows.Add(2, "Test2");

        DataTable table2 = dataSet.Tables.Add("Table1");
        table2.Columns.Add("Code", typeof(string));
        table2.Columns.Add("Value", typeof(decimal));
        table2.Rows.Add("A", 10.5m);
        table2.Rows.Add("B", 20.7m);

        return dataSet;
    }
}