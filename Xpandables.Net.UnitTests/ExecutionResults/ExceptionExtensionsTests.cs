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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Security;
using System.Security.Authentication;

using FluentAssertions;

using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.UnitTests.ExecutionResults;

/// <summary>
/// Tests for Exception extension methods.
/// </summary>
public class ExceptionExtensionsTests
{
    [Theory]
    [InlineData(typeof(ArgumentNullException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(ArgumentOutOfRangeException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(ArgumentException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(ValidationException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(FormatException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized)]
    [InlineData(typeof(AuthenticationException), HttpStatusCode.Unauthorized)]
    [InlineData(typeof(SecurityException), HttpStatusCode.Forbidden)]
    [InlineData(typeof(FileNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(DirectoryNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(KeyNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(NotSupportedException), HttpStatusCode.MethodNotAllowed)]
    [InlineData(typeof(TimeoutException), HttpStatusCode.RequestTimeout)]
    [InlineData(typeof(IOException), HttpStatusCode.Conflict)]
    [InlineData(typeof(DuplicateNameException), HttpStatusCode.Conflict)]
    [InlineData(typeof(InvalidDataException), (HttpStatusCode)422)] // Unprocessable Entity
    [InlineData(typeof(SynchronizationLockException), (HttpStatusCode)423)] // Locked
    [InlineData(typeof(WebException), HttpStatusCode.BadGateway)]
    [InlineData(typeof(NotImplementedException), HttpStatusCode.NotImplemented)]
    [InlineData(typeof(InvalidProgramException), HttpStatusCode.ServiceUnavailable)]
    [InlineData(typeof(TaskCanceledException), HttpStatusCode.GatewayTimeout)]
    [InlineData(typeof(OperationCanceledException), HttpStatusCode.GatewayTimeout)]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.InternalServerError)]
    [InlineData(typeof(NullReferenceException), HttpStatusCode.InternalServerError)]
    public void GetHttpStatusCode_ShouldReturnCorrectStatusCode(Type exceptionType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;

        // Act
        var statusCode = exception.GetHttpStatusCode();

        // Assert
        statusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void GetHttpStatusCode_WithNullException_ShouldThrow()
    {
        // Arrange
        Exception? exception = null;

        // Act & Assert
        Action action = () => exception!.GetHttpStatusCode();
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToExecutionResult_WithBasicException_ShouldCreateFailureResult()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error message");

        // Act
        var result = exception.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.IsSuccess.Should().BeFalse();
        result.Errors.ContainsKey("Exception").Should().BeTrue();
    }

    [Fact]
    public void ToExecutionResult_WithCustomStatusCode_ShouldUseProvidedStatusCode()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        var result = exception.ToExecutionResult(HttpStatusCode.UnprocessableEntity);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ToExecutionResult_WithCustomReason_ShouldUseProvidedReason()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        const string customReason = "Custom error reason";

        // Act
        var result = exception.ToExecutionResult(null, customReason);

        // Assert
        result.Title.Should().Be(customReason);
    }

    [Fact]
    public void ToExecutionResult_WithExecutionResultException_ShouldExtractOriginalResult()
    {
        // Arrange
        var originalResult = ExecutionResult.BadRequest()
            .WithTitle("Original Title")
            .WithDetail("Original Detail")
            .Build();
        var executionResultException = new ExecutionResultException(originalResult);

        // Act
        var result = executionResultException.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Title.Should().Be("Bad Request"); // Uses status code title
        result.Detail.Should().Be("Please refer to the errors property for additional details"); // Uses status code detail
    }

    [Fact]
    public void ToExecutionResult_WithNullException_ShouldThrow()
    {
        // Arrange
        Exception? exception = null;

        // Act & Assert
        Action action = () => exception!.ToExecutionResult();
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetElementEntries_WithBasicException_ShouldReturnEmptyCollection()
    {
        // Arrange
        var exception = new InvalidOperationException("Simple error");

        // Act
        var entries = exception.GetElementEntries();

        // Assert
        entries.Should().BeEmpty();
    }

    [Fact]
    public void GetElementEntries_WithValidationException_ShouldExtractValidationErrors()
    {
        // Arrange
        var validationResult = new ValidationResult("Validation error", ["field1"]);
        var validationException = new ValidationException(validationResult, null, null);

        // Act
        var entries = validationException.GetElementEntries();

        // Assert
        entries.Count.Should().BeGreaterThan(0);
        entries.ContainsKey("field1").Should().BeTrue();
    }

    [Fact]
    public void GetElementEntries_WithAggregateException_ShouldFlattenAndExtractEntries()
    {
        // Arrange
        var validationResult1 = new ValidationResult("Error 1", ["field1"]);
        var validationResult2 = new ValidationResult("Error 2", ["field2"]);
        var validation1 = new ValidationException(validationResult1, null, null);
        var validation2 = new ValidationException(validationResult2, null, null);
        var aggregateException = new AggregateException(validation1, validation2);

        // Act
        var entries = aggregateException.GetElementEntries();

        // Assert
        entries.Count.Should().BeGreaterThan(0);
        entries.ContainsKey("field1").Should().BeTrue();
        entries.ContainsKey("field2").Should().BeTrue();
    }

    [Fact]
    public void GetElementEntries_WithJsonFormattedMessage_ShouldParseErrors()
    {
        // Arrange
        var jsonMessage = """
            {
                "errors": {
                    "field1": "Field 1 error",
                    "field2": ["Error 1", "Error 2"]
                }
            }
            """;
        var exception = new InvalidOperationException(jsonMessage);

        // Act
        var entries = exception.GetElementEntries();

        // Assert
        entries.Count.Should().Be(2);
        entries.ContainsKey("field1").Should().BeTrue();
        entries.ContainsKey("field2").Should().BeTrue();

        entries.TryGetValue("field2", out var field2Entry).Should().BeTrue();
        field2Entry.Values.Count.Should().Be(2);
    }

    [Fact]
    public void GetElementEntries_WithInvalidJsonMessage_ShouldIgnoreParsingErrors()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var exception = new InvalidOperationException(invalidJson);

        // Act
        var entries = exception.GetElementEntries();

        // Assert
        entries.Should().BeEmpty();
    }

    [Fact]
    public void GetElementEntries_WithNonJsonMessage_ShouldReturnEmpty()
    {
        // Arrange
        var plainMessage = "This is a plain text error message";
        var exception = new InvalidOperationException(plainMessage);

        // Act
        var entries = exception.GetElementEntries();

        // Assert
        entries.Should().BeEmpty();
    }

    [Fact]
    public void GetElementEntries_WithNestedExceptions_ShouldTraverseHierarchy()
    {
        // Arrange
        var validationResult = new ValidationResult("Inner validation error", ["innerField"]);
        var innerException = new ValidationException(validationResult, null, null);
        var outerException = new InvalidOperationException("Outer error", innerException);

        // Act
        var entries = outerException.GetElementEntries();

        // Assert
        entries.Count.Should().BeGreaterThan(0);
        entries.ContainsKey("innerField").Should().BeTrue();
    }

    [Fact]
    public void GetElementEntries_WithNullException_ShouldThrow()
    {
        // Arrange
        Exception? exception = null;

        // Act & Assert
        Action action = () => exception!.GetElementEntries();
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetElementEntries_WithCircularReferences_ShouldNotInfiniteLoop()
    {
        // This is a bit artificial since .NET exceptions don't naturally create cycles,
        // but we'll test the visited set mechanism with AggregateException

        // Arrange
        var baseException = new InvalidOperationException("Base error");
        var aggregateException = new AggregateException(baseException, baseException); // Same exception referenced twice

        // Act
        var entries = aggregateException.GetElementEntries();

        // Assert - Should complete without infinite loop
        entries.Should().BeEmpty(); // No validation errors in this case
    }

    [Fact]
    public void GetElementEntries_WithEmptyJsonErrors_ShouldHandleGracefully()
    {
        // Arrange
        var jsonMessage = """
            {
                "errors": {}
            }
            """;
        var exception = new InvalidOperationException(jsonMessage);

        // Act
        var entries = exception.GetElementEntries();

        // Assert
        entries.Should().BeEmpty();
    }

    [Fact]
    public void GetElementEntries_WithMixedJsonErrorTypes_ShouldHandleAllTypes()
    {
        // Arrange
        var jsonMessage = """
            {
                "errors": {
                    "stringField": "String error",
                    "arrayField": ["Array error 1", "Array error 2"],
                    "nullField": null,
                    "numberField": 123,
                    "emptyStringField": "",
                    "emptyArrayField": []
                }
            }
            """;
        var exception = new InvalidOperationException(jsonMessage);

        // Act
        var entries = exception.GetElementEntries();

        // Assert
        entries.Count.Should().Be(2); // Only stringField and arrayField should be added
        entries.ContainsKey("stringField").Should().BeTrue();
        entries.ContainsKey("arrayField").Should().BeTrue();
        entries.ContainsKey("nullField").Should().BeFalse();
        entries.ContainsKey("numberField").Should().BeFalse();
        entries.ContainsKey("emptyStringField").Should().BeFalse();
        entries.ContainsKey("emptyArrayField").Should().BeFalse();
    }

    [Theory]
    [InlineData("Development", true)]
    [InlineData("Production", false)]
    [InlineData("Staging", false)]
    [InlineData("", false)]
    [InlineData(null, true)] // Default is Development
    public void ToExecutionResult_EnvironmentAwareness_ShouldShowDifferentDetailsBasedOnEnvironment(string? environment, bool shouldShowDetails)
    {
        // Arrange
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var exception = new InvalidOperationException("Sensitive error details");

        try
        {
            if (environment is not null)
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
            }

            // Act
            var result = exception.ToExecutionResult();

            // Assert
            if (shouldShowDetails)
            {
                result.Title.Should().Contain("Sensitive error details");
                result.Detail.Should().Contain("InvalidOperationException");
            }
            else
            {
                result.Title.Should().Be("Internal Server Error");
                result.Detail.Should().Be("Please refer to the errors/or contact administrator for additional details");
            }
        }
        finally
        {
            // Restore original environment
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
        }
    }
}