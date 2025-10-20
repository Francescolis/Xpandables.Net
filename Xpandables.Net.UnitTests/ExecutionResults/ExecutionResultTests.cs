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
using System.Net;

using FluentAssertions;

using Xpandables.Net.Collections;
using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.UnitTests.ExecutionResults;

/// <summary>
/// Tests for ExecutionResult functionality.
/// </summary>
public class ExecutionResultTests
{
    [Fact]
    public void ExecutionResult_SuccessStatus_ShouldReturnTrue()
    {
        // Arrange
        var result = ExecutionResult.Success(HttpStatusCode.OK).Build();

        // Act & Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void ExecutionResult_FailureStatus_ShouldReturnFalse()
    {
        // Arrange
        var result = ExecutionResult.Failure(HttpStatusCode.BadRequest).Build();

        // Act & Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void ExecutionResult_WithErrorsAndExtensions_ShouldContainData()
    {
        // Arrange
        var errors = new ElementCollection
        {
            { "field1", "Error message 1" },
            { "field2", "Error message 2" }
        };

        var extensions = new ElementCollection
        {
            { "extension1", "Extension value 1" }
        };

        var headers = new ElementCollection
        {
            { "X-Custom-Header", "Custom value" }
        };

        var result = ExecutionResult
            .Failure(HttpStatusCode.BadRequest)
            .WithTitle("Validation Failed")
            .WithDetail("The request contains validation errors")
            .WithErrors(errors)
            .WithExtensions(extensions)
            .WithHeaders(headers)
            .WithLocation(new Uri("https://example.com/validation-errors"))
            .Build();

        // Act & Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Title.Should().Be("Validation Failed");
        result.Detail.Should().Be("The request contains validation errors");
        result.Errors.Count.Should().Be(2);
        result.Extensions.Count.Should().Be(1);
        result.Headers.Count.Should().Be(1);
        result.Location.Should().Be(new Uri("https://example.com/validation-errors"));
    }

    [Fact]
    public void ExecutionResult_EnsureSuccess_WhenSuccessful_ShouldNotThrow()
    {
        // Arrange
        var result = ExecutionResult.Success(HttpStatusCode.OK).Build();

        // Act & Assert
        result.Invoking(r => r.EnsureSuccess()).Should().NotThrow();
    }

    [Fact]
    public void ExecutionResult_EnsureSuccess_WhenFailed_ShouldThrow()
    {
        // Arrange
        var result = ExecutionResult.Failure(HttpStatusCode.BadRequest).Build();

        // Act & Assert
        result.Invoking(r => r.EnsureSuccess())
            .Should().Throw<ExecutionResultException>()
            .Which.ExecutionResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void ExecutionResult_ToHttpStatusCode_ShouldReturnStatusCode()
    {
        // Arrange
        var result = ExecutionResult.Success(HttpStatusCode.Created).Build();

        // Act
        var statusCode = result.ToHttpStatusCode();

        // Assert
        statusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public void ExecutionResult_ToExecutionResult_ShouldConvertToGeneric()
    {
        // Arrange
        var result = ExecutionResult
            .Success(HttpStatusCode.OK)
            .WithResult("test value")
            .Build();

        // Act
        var genericResult = result.ToExecutionResult<string>();

        // Assert
        genericResult.StatusCode.Should().Be(HttpStatusCode.OK);
        genericResult.Value.Should().Be("test value");
        genericResult.IsGeneric.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResult_IsGeneric_ShouldReturnFalse()
    {
        // Arrange
        var result = ExecutionResult.Success(HttpStatusCode.OK).Build();

        // Act & Assert
        result.IsGeneric.Should().BeFalse();
    }

    [Fact]
    public void ExecutionResult_WithValue_ShouldStoreValue()
    {
        // Arrange
        var testValue = new { Name = "Test", Id = 1 };
        var result = ExecutionResult
            .Success(HttpStatusCode.OK)
            .WithResult(testValue)
            .Build();

        // Act & Assert
        result.Value.Should().Be(testValue);
        result.IsSuccess.Should().BeTrue();
    }
}