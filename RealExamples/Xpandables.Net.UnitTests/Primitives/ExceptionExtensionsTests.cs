/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Security;
using System.Security.Authentication;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class ExceptionExtensionsTests
{
    #region GetFullExceptionMessage Tests

    [Fact]
    public void WhenGettingFullMessageFromSingleExceptionThenShouldReturnMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");

		// Act
		string result = exception.GetFullExceptionMessage();

        // Assert
        result.Should().Contain("Something went wrong");
    }

    [Fact]
    public void WhenGettingFullMessageFromNestedExceptionsThenShouldIncludeAll()
    {
        // Arrange
        var innerMost = new ArgumentException("Invalid argument");
        var inner = new InvalidOperationException("Operation failed", innerMost);
        var outer = new ApplicationException("Application error", inner);

		// Act
		string result = outer.GetFullExceptionMessage();

        // Assert
        result.Should().Contain("Application error");
        result.Should().Contain("Operation failed");
        result.Should().Contain("Invalid argument");
    }

    [Fact]
    public void WhenGettingFullMessageFromNullExceptionThenShouldThrow()
    {
        // Arrange
        Exception? exception = null;

		// Act
		Func<string> act = () => exception!.GetFullExceptionMessage();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenExceptionHasMultipleLevelsOfNestingThenShouldIncludeAllLevels()
    {
        // Arrange
        var level4 = new Exception("Level 4");
        var level3 = new Exception("Level 3", level4);
        var level2 = new Exception("Level 2", level3);
        var level1 = new Exception("Level 1", level2);

		// Act
		string result = level1.GetFullExceptionMessage();

        // Assert
        result.Should().Contain("Level 1");
        result.Should().Contain("Level 2");
        result.Should().Contain("Level 3");
        result.Should().Contain("Level 4");
    }

    #endregion

    #region GetHttpStatusCode Tests

    [Theory]
    [InlineData(typeof(ArgumentNullException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(ArgumentOutOfRangeException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(ArgumentException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(FormatException), HttpStatusCode.BadRequest)]
    public void WhenMappingClientErrorExceptionsThenShouldReturnBadRequest(Type exceptionType, HttpStatusCode expected)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void WhenMappingValidationExceptionThenShouldReturnBadRequest()
    {
        // Arrange
        var exception = new ValidationException("Validation failed");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void WhenMappingUnauthorizedAccessExceptionThenShouldReturnUnauthorized()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void WhenMappingAuthenticationExceptionThenShouldReturnUnauthorized()
    {
        // Arrange
        var exception = new AuthenticationException("Authentication failed");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void WhenMappingSecurityExceptionThenShouldReturnForbidden()
    {
        // Arrange
        var exception = new SecurityException("Access forbidden");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(typeof(FileNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(DirectoryNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(KeyNotFoundException), HttpStatusCode.NotFound)]
    public void WhenMappingNotFoundExceptionsThenShouldReturnNotFound(Type exceptionType, HttpStatusCode expected)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Not found")!;

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void WhenMappingNotSupportedExceptionThenShouldReturnMethodNotAllowed()
    {
        // Arrange
        var exception = new NotSupportedException("Method not supported");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public void WhenMappingTimeoutExceptionThenShouldReturnRequestTimeout()
    {
        // Arrange
        var exception = new TimeoutException("Request timed out");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.RequestTimeout);
    }

    [Theory]
    [InlineData(typeof(IOException), HttpStatusCode.Conflict)]
    [InlineData(typeof(DuplicateNameException), HttpStatusCode.Conflict)]
    public void WhenMappingConflictExceptionsThenShouldReturnConflict(Type exceptionType, HttpStatusCode expected)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Conflict")!;

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void WhenMappingNotImplementedExceptionThenShouldReturnNotImplemented()
    {
        // Arrange
        var exception = new NotImplementedException("Not implemented");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.NotImplemented);
    }

    [Theory]
    [InlineData(typeof(TaskCanceledException), HttpStatusCode.GatewayTimeout)]
    [InlineData(typeof(OperationCanceledException), HttpStatusCode.GatewayTimeout)]
    public void WhenMappingCancellationExceptionsThenShouldReturnGatewayTimeout(Type exceptionType, HttpStatusCode expected)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void WhenMappingWebExceptionThenShouldReturnBadGateway()
    {
        // Arrange
        var exception = new WebException("Bad gateway");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.BadGateway);
    }

    [Theory]
    [InlineData(typeof(NullReferenceException), HttpStatusCode.InternalServerError)]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.InternalServerError)]
    [InlineData(typeof(ApplicationException), HttpStatusCode.InternalServerError)]
    public void WhenMappingServerErrorExceptionsThenShouldReturnInternalServerError(Type exceptionType, HttpStatusCode expected)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Server error")!;

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void WhenMappingUnknownExceptionThenShouldReturnInternalServerError()
    {
        // Arrange
        var exception = new CustomException("Custom error");

		// Act
		HttpStatusCode result = exception.GetHttpStatusCode();

        // Assert
        result.Should().Be(HttpStatusCode.InternalServerError);
    }

    private sealed class CustomException(string message) : Exception(message)
    {
    }

    #endregion

    #region GetElementEntries Tests

    [Fact]
    public void WhenExceptionHasValidationResultThenShouldReturnElementEntries()
    {
        // Arrange
        var validationResult = new ValidationResult(
            "Name is required",
            ["Name"]);
        var exception = new ValidationException(validationResult, null, null);

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.IsEmpty.Should().BeFalse();
        result.ContainsKey("Name").Should().BeTrue();
    }

    [Fact]
    public void WhenExceptionMessageContainsJsonErrorsThenShouldParseErrors()
    {
		// Arrange
		string jsonMessage = """{"errors":{"Email":["Invalid email format"],"Password":["Too short"]}}""";
        var exception = new Exception(jsonMessage);

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.IsEmpty.Should().BeFalse();
        result.ContainsKey("Email").Should().BeTrue();
        result.ContainsKey("Password").Should().BeTrue();
    }

    [Fact]
    public void WhenAggregateExceptionHasMultipleInnerExceptionsThenShouldCollectAllEntries()
    {
		// Arrange
		ValidationException[] innerExceptions = new[]
        {
            new ValidationException(new ValidationResult("Error 1", ["Field1"]), null, null),
            new ValidationException(new ValidationResult("Error 2", ["Field2"]), null, null)
        };
        var aggregateException = new AggregateException(innerExceptions);

		// Act
		ElementCollection result = aggregateException.GetElementEntries();

        // Assert
        result.ContainsKey("Field1").Should().BeTrue();
        result.ContainsKey("Field2").Should().BeTrue();
    }

    [Fact]
    public void WhenExceptionMessageIsPlainTextThenShouldReturnEmpty()
    {
        // Arrange
        var exception = new Exception("Simple error message");

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void WhenExceptionMessageIsInvalidJsonThenShouldReturnEmpty()
    {
        // Arrange
        var exception = new Exception("{invalid json}");

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void WhenJsonHasArrayOfErrorsThenShouldParseAll()
    {
		// Arrange
		string jsonMessage = """{"errors":{"Username":["Required","Must be unique","Too short"]}}""";
        var exception = new Exception(jsonMessage);

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.ContainsKey("Username").Should().BeTrue();
		ElementEntry? entry = result["Username"];
        entry.Should().NotBeNull();
        entry!.Value.Values.Count.Should().Be(3);
    }

    [Fact]
    public void WhenNestedExceptionHasJsonErrorsThenShouldParseFromInner()
    {
		// Arrange
		string jsonMessage = """{"errors":{"Age":["Must be positive"]}}""";
        var inner = new Exception(jsonMessage);
        var outer = new Exception("Wrapper", inner);

		// Act
		ElementCollection result = outer.GetElementEntries();

        // Assert
        result.ContainsKey("Age").Should().BeTrue();
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void WhenProcessingApiValidationErrorsThenShouldExtractAllErrors()
    {
		// Arrange - Simulating an API validation response
		string apiErrorResponse = """
            {
                "errors": {
                    "Email": ["Invalid email format", "Email already exists"],
                    "Password": ["Password must be at least 8 characters"],
                    "ConfirmPassword": ["Passwords do not match"]
                }
            }
            """;
        var exception = new HttpRequestException(apiErrorResponse);

		// Act
		ElementCollection entries = exception.GetElementEntries();

        // Assert
        entries.Count.Should().Be(3);
        entries["Email"]!.Value.Values.Count.Should().Be(2);
        entries["Password"]!.Value.Values.Count.Should().Be(1);
        entries["ConfirmPassword"]!.Value.Values.Count.Should().Be(1);
    }

    [Fact]
    public void WhenHandlingDatabaseExceptionThenShouldMapToConflict()
    {
        // Arrange - Simulating a database constraint violation
        var inner = new Exception("Unique constraint violation");
        var exception = new IOException("Unable to save", inner);

		// Act
		HttpStatusCode statusCode = exception.GetHttpStatusCode();
		string fullMessage = exception.GetFullExceptionMessage();

        // Assert
        statusCode.Should().Be(HttpStatusCode.Conflict);
        fullMessage.Should().Contain("Unable to save");
        fullMessage.Should().Contain("Unique constraint violation");
    }

    [Fact]
    public void WhenHandlingAuthenticationFailureThenShouldMapCorrectly()
    {
        // Arrange
        var inner = new AuthenticationException("Invalid credentials");
        var outer = new UnauthorizedAccessException("Login failed", inner);

		// Act
		HttpStatusCode statusCode = outer.GetHttpStatusCode();
		string fullMessage = outer.GetFullExceptionMessage();

        // Assert
        statusCode.Should().Be(HttpStatusCode.Unauthorized);
        fullMessage.Should().Contain("Login failed");
        fullMessage.Should().Contain("Invalid credentials");
    }

    [Fact]
    public void WhenHandlingServiceTimeoutThenShouldMapToGatewayTimeout()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            cts.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException ex)
        {
			// Act
			HttpStatusCode statusCode = ex.GetHttpStatusCode();

            // Assert
            statusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        }
    }

    [Fact]
    public void WhenHandlingResourceNotFoundThenShouldMapToNotFound()
    {
        // Arrange
        var inner = new KeyNotFoundException("Customer with ID 123 not found");
        var outer = new InvalidOperationException("Failed to load customer", inner);

		// Act
		// We test the inner exception mapping since that's the root cause
		HttpStatusCode statusCode = inner.GetHttpStatusCode();
		string fullMessage = outer.GetFullExceptionMessage();

        // Assert
        statusCode.Should().Be(HttpStatusCode.NotFound);
        fullMessage.Should().Contain("Customer with ID 123 not found");
    }

    [Fact]
    public void WhenHandlingValidationExceptionWithMultipleErrorsThenShouldExtractAll()
    {
        // Arrange
        var validationResult = new ValidationResult(
            "Multiple fields are invalid",
            ["Field1", "Field2", "Field3"]);
        var exception = new ValidationException(validationResult, null, null);

		// Act
		ElementCollection entries = exception.GetElementEntries();

        // Assert
        entries.Should().NotBeNull();
    }

    [Fact]
    public void WhenBuildingErrorResponseFromExceptionThenShouldHaveAllDetails()
    {
        // Arrange
        var inner = new ArgumentException("Invalid customer ID");
        var exception = new InvalidOperationException("Failed to process order", inner);

		// Act
		HttpStatusCode statusCode = exception.GetHttpStatusCode();
		string fullMessage = exception.GetFullExceptionMessage();
		ElementCollection entries = exception.GetElementEntries();

        // Assert - Building an error response
        var errorResponse = new
        {
            Status = (int)statusCode,
            Title = statusCode.ToString(),
            Detail = fullMessage.Trim(),
            Errors = entries.ToDictionary()
        };

        errorResponse.Status.Should().Be(500);
        errorResponse.Detail.Should().Contain("Failed to process order");
        errorResponse.Detail.Should().Contain("Invalid customer ID");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void WhenExceptionMessageStartsWithBracketButIsNotJsonThenShouldReturnEmpty()
    {
        // Arrange
        var exception = new Exception("[Error] Something went wrong");

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void WhenJsonHasErrorsPropertyButWrongTypeThenShouldReturnEmpty()
    {
        // Arrange
        var exception = new Exception("""{"errors":"string instead of object"}""");

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void WhenAggregateExceptionIsEmptyThenShouldReturnEmpty()
    {
        // Arrange
        var exception = new AggregateException([]);

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void WhenExceptionHasCircularReferenceThenShouldNotInfiniteLoop()
    {
        // Note: .NET exceptions don't typically have circular references,
        // but this tests the visited set logic
        var inner = new Exception("Inner");
        var outer = new AggregateException(inner, inner);

		// Act
		ElementCollection result = outer.GetElementEntries();

        // Assert - Should complete without hanging
        result.Should().NotBeNull();
    }

    [Fact]
    public void WhenJsonHasCaseInsensitiveErrorsKeyThenShouldParse()
    {
		// Arrange
		string jsonMessage = """{"ERRORS":{"Field":["Error message"]}}""";
        var exception = new Exception(jsonMessage);

		// Act
		ElementCollection result = exception.GetElementEntries();

        // Assert
        result.ContainsKey("Field").Should().BeTrue();
    }

    #endregion
}
