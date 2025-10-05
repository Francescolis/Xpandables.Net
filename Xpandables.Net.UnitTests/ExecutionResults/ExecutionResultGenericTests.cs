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
/// Tests for generic ExecutionResult functionality.
/// </summary>
public class ExecutionResultGenericTests
{
    [Fact]
    public void ExecutionResult_Generic_SuccessStatus_ShouldReturnTrue()
    {
        // Arrange
        var result = ExecutionResultExtensions
            .Success(HttpStatusCode.OK, "test value")
            .Build();

        // Act & Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
        result.IsGeneric.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResult_Generic_FailureStatus_ShouldReturnFalse()
    {
        // Arrange
        var result = ExecutionResultExtensions
            .Failure<string>(HttpStatusCode.BadRequest)
            .Build();

        // Act & Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void ExecutionResult_Generic_EnsureSuccess_WhenSuccessful_ShouldNotThrow()
    {
        // Arrange
        var result = ExecutionResultExtensions
            .Success(HttpStatusCode.OK, 42)
            .Build();

        // Act & Assert
        result.Invoking(r => r.EnsureSuccess()).Should().NotThrow();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ExecutionResult_Generic_EnsureSuccess_WhenFailed_ShouldThrow()
    {
        // Arrange
        var result = ExecutionResultExtensions
            .Failure<string>(HttpStatusCode.NotFound)
            .Build();

        // Act & Assert
        result.Invoking(r => r.EnsureSuccess())
            .Should().Throw<ExecutionResultException>()
            .Which.ExecutionResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void ExecutionResult_Generic_ToExecutionResult_ShouldConvertToNonGeneric()
    {
        // Arrange
        var result = ExecutionResultExtensions
            .Success(HttpStatusCode.Created, "created resource")
            .Build();

        // Act
        var nonGenericResult = result.ToExecutionResult();

        // Assert
        nonGenericResult.StatusCode.Should().Be(HttpStatusCode.Created);
        nonGenericResult.Value.Should().Be("created resource");
        nonGenericResult.IsGeneric.Should().BeFalse();
    }

    [Fact]
    public void ExecutionResult_Generic_WithComplexType_ShouldStoreValue()
    {
        // Arrange
        var complexValue = new TestModel { Id = 1, Name = "Test Model", Active = true };
        var result = ExecutionResultExtensions
            .Success(HttpStatusCode.OK, complexValue)
            .Build();

        // Act & Assert
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test Model");
        result.Value.Active.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResult_Generic_WithNullValue_ShouldAllowNull()
    {
        // Arrange
        var result = ExecutionResultExtensions
            .Success(HttpStatusCode.NoContent, (string?)null)
            .Build();

        // Act & Assert
        result.Value.Should().BeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResult_Generic_WithErrors_ShouldContainErrorDetails()
    {
        // Arrange
        var errors = new ElementCollection
        {
            { "validation", "Field is required" },
            { "business", "Business rule violation" }
        };

        var result = ExecutionResultExtensions
            .Failure<TestModel>(HttpStatusCode.UnprocessableEntity)
            .WithTitle("Validation Error")
            .WithDetail("The submitted items contains errors")
            .WithErrors(errors)
            .Build();

        // Act & Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Count.Should().Be(2);
        result.Errors.ContainsKey("validation").Should().BeTrue();
        result.Errors.ContainsKey("business").Should().BeTrue();
    }

    [Fact]
    public void ExecutionResult_Generic_WithLocation_ShouldStoreUri()
    {
        // Arrange
        var location = new Uri("https://api.example.com/resources/123");
        var result = ExecutionResultExtensions
            .Success(HttpStatusCode.Created, new TestModel { Id = 123, Name = "Created Resource" })
            .WithLocation(location)
            .Build();

        // Act & Assert
        result.Location.Should().Be(location);
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(123);
    }

    private record TestModel
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool Active { get; init; }
    }
}