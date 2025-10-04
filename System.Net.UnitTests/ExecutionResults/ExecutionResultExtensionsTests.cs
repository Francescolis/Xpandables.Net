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
using System.Net.ExecutionResults;

using FluentAssertions;

namespace System.Net.UnitTests.ExecutionResults;

/// <summary>
/// Tests for ExecutionResult extension methods.
/// </summary>
public class ExecutionResultExtensionsTests
{
    [Fact]
    public void Success_WithNoParameters_ShouldReturnOkResult()
    {
        // Act
        var result = ExecutionResultExtensions.Success();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Success_WithValue_ShouldReturnOkResultWithValue()
    {
        // Arrange
        const string testValue = "test result";

        // Act
        var result = ExecutionResultExtensions.Success(testValue);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Value.Should().Be(testValue);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Success_WithStatusCode_ShouldReturnBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Success(HttpStatusCode.Created);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Success_Generic_WithStatusCode_ShouldReturnGenericBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Success<string>(HttpStatusCode.Created);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Success_WithStatusCodeAndValue_ShouldReturnBuilderWithValue()
    {
        // Arrange
        const int testValue = 42;

        // Act
        var builder = ExecutionResultExtensions.Success(HttpStatusCode.Created, testValue);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Value.Should().Be(testValue);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ok_ShouldReturnOkBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Ok();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ok_Generic_ShouldReturnGenericOkBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Ok<string>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ok_WithValue_ShouldReturnOkResultWithValue()
    {
        // Arrange
        var testModel = new TestModel { Id = 1, Name = "Test" };

        // Act
        var builder = ExecutionResultExtensions.Ok(testModel);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Value.Should().Be(testModel);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Created_ShouldReturnCreatedBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Created();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Created_Generic_ShouldReturnGenericCreatedBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Created<TestModel>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Created_WithValue_ShouldReturnCreatedResultWithValue()
    {
        // Arrange
        var testModel = new TestModel { Id = 2, Name = "Created Model" };

        // Act
        var builder = ExecutionResultExtensions.Created(testModel);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Value.Should().Be(testModel);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void NoContent_ShouldReturnNoContentBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.NoContent();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void NoContent_Generic_ShouldReturnGenericNoContentBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.NoContent<TestModel>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Failure_WithKeyAndMessage_ShouldReturnBadRequestResult()
    {
        // Act
        var result = ExecutionResultExtensions.Failure("validation", "Field is required");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.ContainsKey("validation").Should().BeTrue();
    }

    [Fact]
    public void Failure_WithException_ShouldReturnInternalErrorResult()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var result = ExecutionResultExtensions.Failure(exception);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
        result.Errors.ContainsKey(ExecutionResult.ExceptionKey).Should().BeTrue();
    }

    [Fact]
    public void Failure_Generic_WithException_ShouldReturnBadRequestResult()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        var result = ExecutionResultExtensions.Failure<TestModel>(exception);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.ContainsKey("Exception").Should().BeTrue();
    }

    [Fact]
    public void Failure_WithStatusCode_ShouldReturnFailureBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Failure(HttpStatusCode.InternalServerError);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Failure_Generic_WithStatusCode_ShouldReturnGenericFailureBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Failure<TestModel>(HttpStatusCode.InternalServerError);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void NotFound_ShouldReturnNotFoundBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.NotFound();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void NotFound_Generic_ShouldReturnGenericNotFoundBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.NotFound<TestModel>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void BadRequest_ShouldReturnBadRequestBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.BadRequest();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void BadRequest_Generic_ShouldReturnGenericBadRequestBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.BadRequest<TestModel>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Conflict_ShouldReturnConflictBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Conflict();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Conflict_Generic_ShouldReturnGenericConflictBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Conflict<TestModel>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Unauthorized_ShouldReturnUnauthorizedBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Unauthorized();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Unauthorized_Generic_ShouldReturnGenericUnauthorizedBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.Unauthorized<TestModel>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void InternalServerError_ShouldReturnInternalServerErrorBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.InternalServerError();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void InternalServerError_Generic_ShouldReturnGenericInternalServerErrorBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.InternalServerError<TestModel>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ServiceUnavailable_ShouldReturnServiceUnavailableBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.ServiceUnavailable();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ServiceUnavailable_Generic_ShouldReturnGenericServiceUnavailableBuilder()
    {
        // Act
        var builder = ExecutionResultExtensions.ServiceUnavailable<TestModel>();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ToExecutionResultException_ShouldCreateException()
    {
        // Arrange
        var result = ExecutionResult.BadRequest()
            .WithTitle("Bad Request")
            .WithDetail("The request is invalid")
            .Build();

        // Act
        var exception = result.ToExecutionResultException();

        // Assert
        exception.Should().NotBeNull();
        exception.ExecutionResult.Should().Be(result);
        exception.Message.Should().Contain($"Execution failed with status code: {result.StatusCode}");
    }

    [Fact]
    public void ToExecutionResultException_WithNullResult_ShouldThrow()
    {
        // Arrange
        ExecutionResult? result = null;

        // Act & Assert
        Action action = () => result!.ToExecutionResultException();
        action.Should().Throw<ArgumentNullException>();
    }

    private record TestModel
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}