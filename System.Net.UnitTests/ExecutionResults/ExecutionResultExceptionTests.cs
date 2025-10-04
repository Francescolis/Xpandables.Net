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
/// Tests for ExecutionResultException functionality.
/// </summary>
public class ExecutionResultExceptionTests
{
    [Fact]
    public void ExecutionResultException_DefaultConstructor_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Action action = () => new ExecutionResultException();
        action.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ExecutionResultException_WithMessageOnly_ShouldThrowNotSupportedException()
    {
        // Arrange
        const string message = "Custom error message";

        // Act & Assert
        Action action = () => new ExecutionResultException(message);
        action.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ExecutionResultException_WithMessageAndInnerException_ShouldThrowNotSupportedException()
    {
        // Arrange
        const string message = "Custom error message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act & Assert
        Action action = () => new ExecutionResultException(message, innerException);
        action.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ExecutionResultException_WithExecutionResult_ShouldStoreResult()
    {
        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.BadRequest)
            .WithTitle("Validation Error")
            .WithDetail("The request contains validation errors")
            .Build();

        // Act
        var exception = new ExecutionResultException(executionResult);

        // Assert
        exception.ExecutionResult.Should().Be(executionResult);
        exception.Message.Should().Contain("BadRequest");
    }

    [Fact]
    public void ExecutionResultException_WithExecutionResultAndMessage_ShouldStoreResultAndMessage()
    {
        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.NotFound)
            .WithTitle("Not Found")
            .WithDetail("Resource not found")
            .Build();
        const string customMessage = "Custom exception message";

        // Act
        var exception = new ExecutionResultException(customMessage, executionResult);

        // Assert
        exception.ExecutionResult.Should().Be(executionResult);
        exception.Message.Should().Be(customMessage);
    }

    [Fact]
    public void ExecutionResultException_WithExecutionResultMessageAndInnerException_ShouldStoreAll()
    {
        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.InternalServerError)
            .WithTitle("Server Error")
            .Build();
        const string customMessage = "Custom exception message";
        var innerException = new ArgumentException("Inner exception message");

        // Act
        var exception = new ExecutionResultException(customMessage, executionResult, innerException);

        // Assert
        exception.ExecutionResult.Should().Be(executionResult);
        exception.Message.Should().Be(customMessage);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ExecutionResultException_WithNullExecutionResult_ShouldThrow()
    {
        // Act & Assert
        Action action = () => new ExecutionResultException((ExecutionResult)null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("*executionResult*");
    }

    [Fact]
    public void ExecutionResultException_WithNullExecutionResultAndMessage_ShouldThrow()
    {
        // Act & Assert
        Action action = () => new ExecutionResultException("message", (ExecutionResult)null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResultException_WithNullExecutionResultMessageAndInnerException_ShouldThrow()
    {
        // Arrange
        var innerException = new Exception("Inner");

        // Act & Assert
        Action action = () => new ExecutionResultException("message", (ExecutionResult)null!, innerException);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResultException_WithSuccessExecutionResult_ShouldThrow()
    {
        // Arrange
        var successResult = ExecutionResultExtensions
            .Success(HttpStatusCode.OK)
            .Build();

        // Act & Assert
        Action action = () => new ExecutionResultException(successResult);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("The HTTP status code 'OK' indicates a success.");
    }

    [Fact]
    public void ExecutionResultException_MessageProperty_WithExecutionResultTitle_ShouldIncludeStatusCode()
    {
        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.BadRequest)
            .WithTitle("Bad Request Title")
            .WithDetail("Detailed error description")
            .Build();

        // Act
        var exception = new ExecutionResultException(executionResult);

        // Assert
        exception.Message.Should().Contain("BadRequest");
    }

    [Fact]
    public void ExecutionResultException_MessageProperty_WithoutExecutionResultTitle_ShouldUseStatusCode()
    {
        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.BadRequest)
            .Build();

        // Act
        var exception = new ExecutionResultException(executionResult);

        // Assert
        exception.Message.Should().Contain("BadRequest");
    }

    [Fact]
    public void ExecutionResultException_Serialization_ShouldPreserveExecutionResult()
    {
        // This test ensures that if serialization is implemented, ExecutionResult is preserved
        // For now, we'll just verify the property is maintained

        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.Conflict)
            .WithTitle("Conflict")
            .WithDetail("Resource conflict detected")
            .Build();

        // Act
        var exception = new ExecutionResultException(executionResult);

        // Assert
        exception.ExecutionResult.Should().Be(executionResult);
        exception.ExecutionResult.StatusCode.Should().Be(HttpStatusCode.Conflict);
        exception.ExecutionResult.Title.Should().Be("Conflict");
        exception.ExecutionResult.Detail.Should().Be("Resource conflict detected");
    }

    [Fact]
    public void ExecutionResultException_ToString_ShouldIncludeExecutionResultInfo()
    {
        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.UnprocessableEntity)
            .WithTitle("Validation Failed")
            .WithDetail("Multiple validation errors occurred")
            .Build();

        var exception = new ExecutionResultException(executionResult);

        // Act
        var toStringResult = exception.ToString();

        // Assert
        toStringResult.Should().Contain(nameof(ExecutionResultException));
        toStringResult.Should().Contain("UnprocessableEntity");
    }

    [Fact]
    public void ExecutionResultException_WithGenericExecutionResult_ShouldWork()
    {
        // Arrange
        var genericResult = ExecutionResultExtensions
            .Failure<string>(HttpStatusCode.BadRequest)
            .WithTitle("Generic Error")
            .Build();

        // Act
        var exception = new ExecutionResultException(genericResult);

        // Assert
        exception.ExecutionResult.Should().BeOfType<ExecutionResult>();
        exception.ExecutionResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.ExecutionResult.Title.Should().Be("Generic Error");
    }

    [Fact]
    public void ExecutionResultException_ConstructorValidation_ShouldEnsureFailureStatus()
    {
        // Arrange
        var failureResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.NotFound)
            .WithTitle("Resource Not Found")
            .Build();

        // Act
        var exception = new ExecutionResultException(failureResult);

        // Assert
        exception.ExecutionResult.Should().Be(failureResult);
        exception.ExecutionResult.IsSuccess.Should().BeFalse();
        exception.Message.Should().Contain("NotFound");
    }

    [Fact]
    public void ExecutionResultException_WithCustomMessage_ShouldOverrideDefaultMessage()
    {
        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.InternalServerError)
            .WithTitle("Server Error")
            .Build();
        const string customMessage = "A custom error occurred during processing";

        // Act
        var exception = new ExecutionResultException(customMessage, executionResult);

        // Assert
        exception.Message.Should().Be(customMessage);
        exception.ExecutionResult.Should().Be(executionResult);
    }

    [Fact]
    public void ExecutionResultException_WithInnerException_ShouldPreserveInnerException()
    {
        // Arrange
        var executionResult = ExecutionResultExtensions
            .Failure(HttpStatusCode.BadGateway)
            .WithTitle("Gateway Error")
            .Build();
        const string customMessage = "Gateway operation failed";
        var innerException = new HttpRequestException("Network error");

        // Act
        var exception = new ExecutionResultException(customMessage, executionResult, innerException);

        // Assert
        exception.Message.Should().Be(customMessage);
        exception.ExecutionResult.Should().Be(executionResult);
        exception.InnerException.Should().Be(innerException);
        exception.InnerException.Message.Should().Be("Network error");
    }
}