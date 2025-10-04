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
using System.Net.Abstractions.Collections;
using System.Net.ExecutionResults;

using FluentAssertions;

using Microsoft.Extensions.Primitives;

namespace System.Net.UnitTests.ExecutionResults;

/// <summary>
/// Tests for ExecutionResult builders functionality.
/// </summary>
public class ExecutionResultBuildersTests
{
    [Fact]
    public void ExecutionResultSuccessBuilder_WithValidStatusCode_ShouldBuild()
    {
        // Arrange & Act
        var builder = new ExecutionResultSuccessBuilder(HttpStatusCode.OK);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultSuccessBuilder_WithInvalidStatusCode_ShouldThrow()
    {
        // Act & Assert
        Action action = () => new ExecutionResultSuccessBuilder(HttpStatusCode.BadRequest);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ExecutionResultSuccessBuilder_Generic_WithValidStatusCode_ShouldBuild()
    {
        // Arrange & Act
        var builder = new ExecutionResultSuccessBuilder<string>(HttpStatusCode.Created);
        var result = builder.WithResult("test result").Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Value.Should().Be("test result");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultFailureBuilder_WithValidStatusCode_ShouldBuild()
    {
        // Arrange & Act
        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.NotFound);
        var result = builder
            .WithTitle("Resource Not Found")
            .WithDetail("The requested resource could not be found")
            .Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Title.Should().Be("Resource Not Found");
        result.Detail.Should().Be("The requested resource could not be found");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ExecutionResultFailureBuilder_WithSuccessStatusCode_ShouldThrow()
    {
        // Act & Assert
        Action action = () => new ExecutionResultFailureBuilder(HttpStatusCode.OK);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ExecutionResultFailureBuilder_Generic_WithErrors_ShouldBuild()
    {
        // Arrange & Act
        var builder = new ExecutionResultFailureBuilder<TestModel>(HttpStatusCode.BadRequest);
        var result = builder
            .WithError("field1", "Field is required")
            .WithError("field2", "Invalid format", "Must be numeric")
            .Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Count.Should().Be(2);
        result.Errors.ContainsKey("field1").Should().BeTrue();
        result.Errors.ContainsKey("field2").Should().BeTrue();

        result.Errors.TryGetValue("field2", out var field2Entry).Should().BeTrue();
        field2Entry.Values.Count.Should().Be(2);
    }

    [Fact]
    public void ExecutionResultBuilder_WithHeaders_ShouldStoreHeaders()
    {
        // Arrange
        var builder = new ExecutionResultSuccessBuilder(HttpStatusCode.OK);

        // Act
        var result = builder
            .WithHeader("X-Custom-Header", "custom-value")
            .WithHeader("X-Multi-Header", "value1", "value2")
            .Build();

        // Assert
        result.Headers.Count.Should().Be(2);
        result.Headers.ContainsKey("X-Custom-Header").Should().BeTrue();
        result.Headers.ContainsKey("X-Multi-Header").Should().BeTrue();

        result.Headers.TryGetValue("X-Multi-Header", out var multiEntry).Should().BeTrue();
        multiEntry.Values.Count.Should().Be(2);
    }

    [Fact]
    public void ExecutionResultBuilder_WithExtensions_ShouldStoreExtensions()
    {
        // Arrange
        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.BadRequest);

        // Act
        var result = builder
            .WithExtension("traceId", "12345")
            .WithExtension("requestId", "abc-def-ghi")
            .Build();

        // Assert
        result.Extensions.Count.Should().Be(2);
        result.Extensions.ContainsKey("traceId").Should().BeTrue();
        result.Extensions.ContainsKey("requestId").Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultBuilder_WithLocation_UsingUri_ShouldStoreLocation()
    {
        // Arrange
        var uri = new Uri("https://api.example.com/resources/123");
        var builder = new ExecutionResultSuccessBuilder(HttpStatusCode.Created);

        // Act
        var result = builder.WithLocation(uri).Build();

        // Assert
        result.Location.Should().Be(uri);
    }

    [Fact]
    public void ExecutionResultBuilder_WithLocation_UsingString_ShouldStoreLocation()
    {
        // Arrange
        var locationString = "https://api.example.com/resources/456";
        var builder = new ExecutionResultSuccessBuilder(HttpStatusCode.Created);

        // Act
        var result = builder.WithLocation(locationString).Build();

        // Assert
        result.Location.Should().Be(new Uri(locationString));
    }

    [Fact]
    public void ExecutionResultBuilder_WithLocation_InvalidUri_ShouldThrow()
    {
        // Arrange
        var builder = new ExecutionResultSuccessBuilder(HttpStatusCode.Created);

        // Act & Assert
        builder.Invoking(b => b.WithLocation("invalid-uri"))
            .Should().Throw<ArgumentException>()
            .WithMessage("Invalid URI format: invalid-uri*");
    }

    [Fact]
    public void ExecutionResultBuilder_WithException_ShouldStoreException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.InternalServerError);

        // Act
        var result = builder.WithException(exception).Build();

        // Assert
        result.Errors.ContainsKey("Exception").Should().BeTrue();
        result.Errors.TryGetValue("Exception", out var exceptionEntry).Should().BeTrue();
        exceptionEntry.Values[0].Should().Contain("Test exception");
    }

    [Fact]
    public void ExecutionResultBuilder_WithAggregateException_ShouldFlattenExceptions()
    {
        // Arrange
        var innerExceptions = new Exception[]
        {
            new InvalidOperationException("First error"),
            new ArgumentException("Second error")
        };
        var aggregateException = new AggregateException(innerExceptions);
        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.InternalServerError);

        // Act
        var result = builder.WithException(aggregateException).Build();

        // Assert
        result.Errors.ContainsKey("Exception").Should().BeTrue();
        result.Errors.TryGetValue("Exception", out var exceptionEntry).Should().BeTrue();
        exceptionEntry.Values[0].Should().Contain("First error");
        exceptionEntry.Values[0].Should().Contain("Second error");
    }

    [Fact]
    public void ExecutionResultBuilder_Merge_ShouldCombineFailureResults()
    {
        // Arrange
        var existingResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.BadRequest)
            .WithTitle("Original Title")
            .WithError("field1", "Original error")
            .Build();

        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.UnprocessableEntity)
            .WithError("field2", "New error");

        // Act
        var result = builder.Merge(existingResult).Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Takes from merged result
        result.Title.Should().Be("Original Title");
        result.Errors.Count.Should().Be(2);
        result.Errors.ContainsKey("field1").Should().BeTrue();
        result.Errors.ContainsKey("field2").Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultBuilder_Merge_WithSuccessResult_ShouldThrow()
    {
        // Arrange
        var successResult = ExecutionResultExtensions
            .Success(HttpStatusCode.OK)
            .Build();

        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.BadRequest);

        // Act & Assert
        builder.Invoking(b => b.Merge(successResult))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Both execution results must be failure to merge them.");
    }

    [Fact]
    public void ExecutionResultBuilder_ClearMethods_ShouldClearCollections()
    {
        // Arrange
        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.BadRequest)
            .WithError("field1", "error")
            .WithExtension("ext1", "value")
            .WithHeader("header1", "value");

        // Act
        builder.ClearErrors();
        builder.ClearExtensions();
        builder.ClearHeaders();
        var result = builder.Build();

        // Assert
        result.Errors.Count.Should().Be(0);
        result.Extensions.Count.Should().Be(0);
        result.Headers.Count.Should().Be(0);
    }

    [Fact]
    public void ExecutionResultBuilder_ClearAll_ShouldResetBuilder()
    {
        // Arrange
        var builder = ExecutionResult.Failure(HttpStatusCode.BadRequest)
            .WithTitle("Title")
            .WithDetail("Detail")
            .WithError("field1", "error")
            .WithExtension("ext1", "value")
            .WithHeader("header1", "value")
            .WithLocation("https://example.com");

        // Act
        builder.ClearAll();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Continue); // Default value
        result.Title.Should().BeNull();
        result.Detail.Should().BeNull();
        result.Location.Should().BeNull();
        result.Value.Should().BeNull();
        result.Errors.Count.Should().Be(0);
        result.Extensions.Count.Should().Be(0);
        result.Headers.Count.Should().Be(0);
    }

    [Fact]
    public void ExecutionResultBuilder_WithStatusCode_ShouldUpdateStatusCode()
    {
        // Arrange
        var builder = new ExecutionResultSuccessBuilder(HttpStatusCode.OK);

        // Act
        var result = builder.WithStatusCode(HttpStatusCode.Created).Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public void ExecutionResultBuilder_WithErrorCollection_ShouldAddAllErrors()
    {
        // Arrange
        var errorCollection = new ElementCollection
        {
            { "field1", "Error 1" },
            { "field2", "Error 2", "Error 3" }
        };

        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.BadRequest);

        // Act
        var result = builder.WithErrors(errorCollection).Build();

        // Assert
        result.Errors.Count.Should().Be(2);
        result.Errors.ContainsKey("field1").Should().BeTrue();
        result.Errors.ContainsKey("field2").Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultBuilder_WithErrorDictionary_ShouldAddAllErrors()
    {
        // Arrange
        var errorDict = new Dictionary<string, string>
        {
            ["field1"] = "Error 1",
            ["field2"] = "Error 2"
        };

        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.BadRequest);

        // Act
        var result = builder.WithErrors(errorDict).Build();

        // Assert
        result.Errors.Count.Should().Be(2);
        result.Errors.ContainsKey("field1").Should().BeTrue();
        result.Errors.ContainsKey("field2").Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultBuilder_WithErrorSpan_ShouldAddAllErrors()
    {
        // Arrange
        var errors = new[]
        {
            new ElementEntry("field1", "Error 1"),
            new ElementEntry("field2", "Error 2")
        };

        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.BadRequest);

        // Act
        var result = builder.WithErrors(errors.AsSpan()).Build();

        // Assert
        result.Errors.Count.Should().Be(2);
        result.Errors.ContainsKey("field1").Should().BeTrue();
        result.Errors.ContainsKey("field2").Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultBuilder_WithErrorStringValues_ShouldStoreMultipleValues()
    {
        // Arrange
        var stringValues = new StringValues(["Error 1", "Error 2", "Error 3"]);
        var builder = new ExecutionResultFailureBuilder(HttpStatusCode.BadRequest);

        // Act
        var result = builder.WithError("field1", in stringValues).Build();

        // Assert
        result.Errors.ContainsKey("field1").Should().BeTrue();
        result.Errors.TryGetValue("field1", out var entry).Should().BeTrue();
        entry.Values.Count.Should().Be(3);
    }

    private record TestModel
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}