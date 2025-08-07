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

namespace Xpandables.Net.Test.IntegrationTests;

public sealed class StoredProcedureBuilderIntegrationTest
{
    [Fact]
    public async Task CompleteWorkflow_WithMockedDatabase_ShouldExecuteSuccessfully()
    {
        // Arrange
        Mock<DbConnection> connectionMock = SetupMockConnection();
        Mock<DbCommand> commandMock = SetupMockCommand();
        Mock<DbDataReader> readerMock = SetupMockDataReader();

        connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);
        commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);

        // Create the builder with comprehensive parameter setup
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("GetCustomerOrders")
            .WithInputParameter("customerId", 123)
            .WithInputParameter("startDate", DateTime.Now.AddDays(-30), SqlDbType.DateTime2)
            .WithOutputParameter("totalAmount", SqlDbType.Decimal, precision: 18, scale: 2)
            .WithInputOutputParameter("recordCount", 0, SqlDbType.Int)
            .WithReturnValue()
            .WithTimeout(TimeSpan.FromMinutes(5))
            .WithRetryPolicy(maxRetries: 3, delay: TimeSpan.FromSeconds(2))
            .WithConnection(connectionMock.Object);

        // Act
        StoredProcedureResult result = await builder.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.HasErrors.Should().BeFalse();
        result.ExecutionTime.Should().BePositive();
        
        // Verify connection was opened and command was configured
        connectionMock.Verify(c => c.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
        commandMock.Verify(c => c.CreateParameter(), Times.AtLeast(4));
        commandMock.VerifySet(c => c.CommandText = "GetCustomerOrders", Times.Once);
        commandMock.VerifySet(c => c.CommandType = CommandType.StoredProcedure, Times.Once);
        commandMock.VerifySet(c => c.CommandTimeout = 300, Times.Once); // 5 minutes = 300 seconds
    }

    [Fact]
    public async Task ObjectParameterBinding_ShouldMapAllProperties()
    {
        // Arrange
        Mock<DbConnection> connectionMock = SetupMockConnection();
        Mock<DbCommand> commandMock = SetupMockCommand();
        Mock<DbDataReader> readerMock = SetupMockDataReader();

        connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);
        commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);

        var searchCriteria = new 
        { 
            CustomerId = 123, 
            Status = "Active", 
            MaxResults = 50,
            IncludeDetails = true 
        };

        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("SearchCustomers")
            .WithParameters(searchCriteria)
            .WithConnection(connectionMock.Object);

        // Act
        StoredProcedureResult result = await builder.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify parameters were added for each property
        commandMock.Verify(c => c.CreateParameter(), Times.AtLeast(4));
    }

    [Fact]
    public async Task ExpressionParameterBinding_ShouldExtractValues()
    {
        // Arrange
        Mock<DbConnection> connectionMock = SetupMockConnection();
        Mock<DbCommand> commandMock = SetupMockCommand();
        Mock<DbDataReader> readerMock = SetupMockDataReader();

        connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);
        commandMock.Setup(c => c.ExecuteReader()).Returns(readerMock.Object);

        int customerId = 456;
        string customerName = "Test Customer";
        bool isActive = true;

        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("UpdateCustomer")
            .WithParametersFromExpression(() => new { customerId, customerName, isActive })
            .WithConnection(connectionMock.Object);

        // Act
        StoredProcedureResult result = await builder.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify parameters were extracted from expression
        commandMock.Verify(c => c.CreateParameter(), Times.AtLeast(3));
    }

    [Fact]
    public async Task ScalarExecution_ShouldReturnTypedValue()
    {
        // Arrange
        Mock<DbConnection> connectionMock = SetupMockConnection();
        Mock<DbCommand> commandMock = SetupMockCommand();

        connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);
        commandMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(42);

        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("GetCustomerCount")
            .WithInputParameter("status", "Active")
            .WithConnection(connectionMock.Object);

        // Act
        int? result = await builder.ExecuteScalarAsync<int>();

        // Assert
        result.Should().Be(42);
        
        commandMock.Verify(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ExtensionMethods_ShouldProvideConvenientAPIs()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("TestProcedure");
        var parameters = new Dictionary<string, object?> { ["id"] = 123 };

        // Act
        StoredProcedureBuilder result = builder
            .WithParameter("legacyParam", "value") // Backward compatibility
            .WithParameters(parameters)
            .WithCommonOutputs(includeRecordCount: true, includeErrorMessage: true)
            .WithPagination(pageIndex: 0, pageSize: 25)
            .WithSearch("test search", "name", "description")
            .WithDateRange(DateTime.Today.AddDays(-7), DateTime.Today, "Created")
            .WithStandardOptions(timeoutMinutes: 10, maxRetries: 5, retryDelaySeconds: 3);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void TableValuedParameters_ShouldCreateDataTable()
    {
        // Arrange
        StoredProcedureBuilder builder = SqlBuilder.StoredProcedure("ProcessOrders");
        var orders = new[]
        {
            new { OrderId = 1, CustomerId = 100, Amount = 150.50m },
            new { OrderId = 2, CustomerId = 101, Amount = 275.25m },
            new { OrderId = 3, CustomerId = 102, Amount = 99.99m }
        };

        // Act
        StoredProcedureBuilder result = builder
            .WithTableParameter("orders", orders, "dbo.OrderTableType")
            .WithInputParameter("processDate", DateTime.Today);

        // Assert
        result.Should().BeSameAs(builder);
    }

    private static Mock<DbConnection> SetupMockConnection()
    {
        Mock<DbConnection> connectionMock = new();
        connectionMock.SetupGet(c => c.State).Returns(ConnectionState.Closed);
        connectionMock.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        connectionMock.Setup(c => c.CloseAsync()).Returns(ValueTask.CompletedTask);
        return connectionMock;
    }

    private static Mock<DbCommand> SetupMockCommand()
    {
        Mock<DbCommand> commandMock = new();
        Mock<DbParameterCollection> parametersCollection = new();
        Mock<DbParameter> parameter = new();
        
        commandMock.SetupGet(c => c.Parameters).Returns(parametersCollection.Object);
        commandMock.Setup(c => c.CreateParameter()).Returns(parameter.Object);
        
        parametersCollection.Setup(p => p.Add(It.IsAny<object>())).Returns(0);
        parametersCollection.Setup(p => p.Cast<DbParameter>()).Returns([parameter.Object]);
        
        parameter.SetupAllProperties();
        
        return commandMock;
    }

    private static Mock<DbDataReader> SetupMockDataReader()
    {
        Mock<DbDataReader> readerMock = new();
        readerMock.SetupGet(r => r.FieldCount).Returns(2);
        readerMock.Setup(r => r.GetName(0)).Returns("Id");
        readerMock.Setup(r => r.GetName(1)).Returns("Name");
        readerMock.Setup(r => r.GetFieldType(0)).Returns(typeof(int));
        readerMock.Setup(r => r.GetFieldType(1)).Returns(typeof(string));
        readerMock.Setup(r => r.Read()).Returns(false); // No data for simplicity
        readerMock.Setup(r => r.NextResult()).Returns(false);
        return readerMock;
    }
}