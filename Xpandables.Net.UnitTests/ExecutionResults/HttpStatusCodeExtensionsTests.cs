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
using System.Net;
using System.Net.Abstractions;
using System.Net.ExecutionResults;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.ExecutionResults;

/// <summary>
/// Tests for HttpStatusCode extension methods.
/// </summary>
public class HttpStatusCodeExtensionsTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.Accepted, true)]
    [InlineData(HttpStatusCode.NoContent, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsSuccess_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsSuccess)
    {
        // Act
        var isSuccess = statusCode.IsSuccess;

        // Assert
        isSuccess.Should().Be(expectedIsSuccess);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.Created, false)]
    [InlineData(HttpStatusCode.BadRequest, true)]
    [InlineData(HttpStatusCode.NotFound, true)]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    public void IsFailure_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsFailure)
    {
        // Act
        var isFailure = statusCode.IsFailure;

        // Assert
        isFailure.Should().Be(expectedIsFailure);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsOk_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsOk)
    {
        // Act
        var isOk = statusCode.IsOk;

        // Assert
        isOk.Should().Be(expectedIsOk);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsCreated_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsCreated)
    {
        // Act
        var isCreated = statusCode.IsCreated;

        // Assert
        isCreated.Should().Be(expectedIsCreated);
    }

    [Theory]
    [InlineData(HttpStatusCode.Accepted, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsAccepted_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsAccepted)
    {
        // Act
        var isAccepted = statusCode.IsAccepted;

        // Assert
        isAccepted.Should().Be(expectedIsAccepted);
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsNoContent_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsNoContent)
    {
        // Act
        var isNoContent = statusCode.IsNoContent;

        // Assert
        isNoContent.Should().Be(expectedIsNoContent);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    public void IsBadRequest_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsBadRequest)
    {
        // Act
        var isBadRequest = statusCode.IsBadRequest;

        // Assert
        isBadRequest.Should().Be(expectedIsBadRequest);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.Forbidden, false)]
    public void IsUnauthorized_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsUnauthorized)
    {
        // Act
        var isUnauthorized = statusCode.IsUnauthorized;

        // Assert
        isUnauthorized.Should().Be(expectedIsUnauthorized);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, true)]
    [InlineData(HttpStatusCode.Unauthorized, false)]
    [InlineData(HttpStatusCode.OK, false)]
    public void IsForbidden_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsForbidden)
    {
        // Act
        var isForbidden = statusCode.IsForbidden;

        // Assert
        isForbidden.Should().Be(expectedIsForbidden);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsNotFound_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsNotFound)
    {
        // Act
        var isNotFound = statusCode.IsNotFound;

        // Assert
        isNotFound.Should().Be(expectedIsNotFound);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.OK, false)]
    public void IsInternalServerError_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsInternalServerError)
    {
        // Act
        var isInternalServerError = statusCode.IsInternalServerError;

        // Assert
        isInternalServerError.Should().Be(expectedIsInternalServerError);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.OK, false)]
    public void IsConflict_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsConflict)
    {
        // Act
        var isConflict = statusCode.IsConflict;

        // Assert
        isConflict.Should().Be(expectedIsConflict);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    [InlineData(HttpStatusCode.OK, false)]
    public void IsServiceUnavailable_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsServiceUnavailable)
    {
        // Act
        var isServiceUnavailable = statusCode.IsServiceUnavailable;

        // Assert
        isServiceUnavailable.Should().Be(expectedIsServiceUnavailable);
    }

    [Theory]
    [InlineData((HttpStatusCode)429, true)] // Too Many Requests
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.OK, false)]
    public void IsTooManyRequests_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsTooManyRequests)
    {
        // Act
        var isTooManyRequests = statusCode.IsTooManyRequests;

        // Assert
        isTooManyRequests.Should().Be(expectedIsTooManyRequests);
    }

    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently, true)]
    [InlineData(HttpStatusCode.Found, true)]
    [InlineData(HttpStatusCode.SeeOther, true)]
    [InlineData(HttpStatusCode.NotModified, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsRedirect_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsRedirect)
    {
        // Act
        var isRedirect = statusCode.IsRedirect;

        // Assert
        isRedirect.Should().Be(expectedIsRedirect);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, true)]
    [InlineData(HttpStatusCode.NotFound, true)]
    [InlineData(HttpStatusCode.Conflict, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsClientError_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsClientError)
    {
        // Act
        var isClientError = statusCode.IsClientError;

        // Assert
        isClientError.Should().Be(expectedIsClientError);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.BadGateway, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.OK, false)]
    public void IsServerError_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsServerError)
    {
        // Act
        var isServerError = statusCode.IsServerError;

        // Assert
        isServerError.Should().Be(expectedIsServerError);
    }

    [Theory]
    [InlineData(HttpStatusCode.Continue, true)]
    [InlineData(HttpStatusCode.SwitchingProtocols, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsInformational_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsInformational)
    {
        // Act
        var isInformational = statusCode.IsInformational;

        // Assert
        isInformational.Should().Be(expectedIsInformational);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, true)]
    [InlineData(HttpStatusCode.NotFound, true)]
    [InlineData(HttpStatusCode.Conflict, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void IsValidationProblem_ShouldReturnCorrectValue(HttpStatusCode statusCode, bool expectedIsValidationProblem)
    {
        // Act
        var isValidationProblem = statusCode.IsValidationProblem;

        // Assert
        isValidationProblem.Should().Be(expectedIsValidationProblem);
    }

    [Fact]
    public void EnsureSuccess_WithSuccessStatusCode_ShouldNotThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;

        // Act & Assert
        statusCode.Invoking(s => s.EnsureSuccess()).Should().NotThrow();
    }

    [Fact]
    public void EnsureSuccess_WithFailureStatusCode_ShouldThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;

        // Act & Assert
        statusCode.Invoking(s => s.EnsureSuccess())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("The HTTP status code 'BadRequest' indicates a failure.");
    }

    [Fact]
    public void EnsureFailure_WithFailureStatusCode_ShouldNotThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;

        // Act & Assert
        statusCode.Invoking(s => s.EnsureFailure()).Should().NotThrow();
    }

    [Fact]
    public void EnsureFailure_WithSuccessStatusCode_ShouldThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;

        // Act & Assert
        statusCode.Invoking(s => s.EnsureFailure())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("The HTTP status code 'OK' indicates a success.");
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "Please refer to the errors/or contact administrator for additional details")]
    [InlineData(HttpStatusCode.Unauthorized, "Please refer to the errors/or contact administrator for additional details")]
    [InlineData(HttpStatusCode.BadRequest, "Please refer to the errors property for additional details")]
    [InlineData(HttpStatusCode.NotFound, "Please refer to the errors property for additional details")]
    public void Detail_ShouldReturnCorrectMessage(HttpStatusCode statusCode, string expectedDetail)
    {
        // Act
        var detail = statusCode.Detail;

        // Assert
        detail.Should().Be(expectedDetail);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "Success")]
    [InlineData(HttpStatusCode.Created, "Created")]
    [InlineData(HttpStatusCode.BadRequest, "Bad Request")]
    [InlineData(HttpStatusCode.NotFound, "Not Found")]
    [InlineData(HttpStatusCode.InternalServerError, "Internal Server Error")]
    [InlineData((HttpStatusCode)999, "Unknown")]
    public void Title_ShouldReturnCorrectTitle(HttpStatusCode statusCode, string expectedTitle)
    {
        // Act
        var title = statusCode.Title;

        // Assert
        title.Should().Be(expectedTitle);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, typeof(ArgumentException))]
    [InlineData(HttpStatusCode.Unauthorized, typeof(UnauthorizedAccessException))]
    [InlineData(HttpStatusCode.NotFound, typeof(KeyNotFoundException))]
    [InlineData(HttpStatusCode.Conflict, typeof(IOException))]
    [InlineData(HttpStatusCode.InternalServerError, typeof(InvalidOperationException))]
    [InlineData(HttpStatusCode.NotImplemented, typeof(NotImplementedException))]
    [InlineData(HttpStatusCode.RequestTimeout, typeof(TimeoutException))]
    public void GetException_ShouldReturnCorrectExceptionType(HttpStatusCode statusCode, Type expectedExceptionType)
    {
        // Arrange
        const string message = "Test error message";

        // Act
        var exception = statusCode.GetException(message);

        // Assert
        exception.Should().BeOfType(expectedExceptionType);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void GetException_WithNullMessage_ShouldThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;

        // Act & Assert
        statusCode.Invoking(s => s.GetException(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(HttpStatusCode.MethodNotAllowed, typeof(NotSupportedException))]
    [InlineData(HttpStatusCode.UnsupportedMediaType, typeof(InvalidOperationException))]
    [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable, typeof(ArgumentOutOfRangeException))]
    [InlineData(HttpStatusCode.BadGateway, typeof(WebException))]
    [InlineData(HttpStatusCode.GatewayTimeout, typeof(TimeoutException))]
    public void GetException_WithSpecificStatusCodes_ShouldReturnAppropriateExceptions(HttpStatusCode statusCode, Type expectedExceptionType)
    {
        // Arrange
        const string message = "Specific error message";

        // Act
        var exception = statusCode.GetException(message);

        // Assert
        exception.Should().BeOfType(expectedExceptionType);
        exception.Message.Should().Be(message);
    }

    [Theory]
    [InlineData((HttpStatusCode)422, typeof(DataException))] // Unprocessable Entity
    [InlineData((HttpStatusCode)429, typeof(InvalidOperationException))] // Too Many Requests  
    [InlineData((HttpStatusCode)511, typeof(System.Security.Authentication.AuthenticationException))] // Network Authentication Required
    public void GetException_WithExtendedStatusCodes_ShouldReturnCorrectExceptionType(HttpStatusCode statusCode, Type expectedExceptionType)
    {
        // Arrange
        const string message = "Extended status code error";

        // Act
        var exception = statusCode.GetException(message);

        // Assert
        exception.Should().BeOfType(expectedExceptionType);
        exception.Message.Should().Be(message);
    }
}