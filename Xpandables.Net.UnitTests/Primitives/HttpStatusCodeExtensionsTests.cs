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
using System.Data;
using System.Net;
using System.Security;
using System.Security.Authentication;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class HttpStatusCodeExtensionsTests
{
    #region Success Status Tests

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    public void WhenStatusCodeIs2xxThenIsSuccessShouldBeTrue(HttpStatusCode statusCode)
    {
        statusCode.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.MovedPermanently)]
    public void WhenStatusCodeIsNot2xxThenIsSuccessShouldBeFalse(HttpStatusCode statusCode)
    {
        statusCode.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public void WhenStatusCodeIsFailureThenIsFailureShouldBeTrue(HttpStatusCode statusCode)
    {
        statusCode.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    public void WhenStatusCodeIsSuccessThenIsFailureShouldBeFalse(HttpStatusCode statusCode)
    {
        statusCode.IsFailure.Should().BeFalse();
    }

    #endregion

    #region Specific Status Code Tests

    [Fact]
    public void WhenStatusCodeIs200ThenIsOkShouldBeTrue()
    {
        HttpStatusCode.OK.IsOk.Should().BeTrue();
        HttpStatusCode.Created.IsOk.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs201ThenIsCreatedShouldBeTrue()
    {
        HttpStatusCode.Created.IsCreated.Should().BeTrue();
        HttpStatusCode.OK.IsCreated.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs202ThenIsAcceptedShouldBeTrue()
    {
        HttpStatusCode.Accepted.IsAccepted.Should().BeTrue();
        HttpStatusCode.OK.IsAccepted.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs204ThenIsNoContentShouldBeTrue()
    {
        HttpStatusCode.NoContent.IsNoContent.Should().BeTrue();
        HttpStatusCode.OK.IsNoContent.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs400ThenIsBadRequestShouldBeTrue()
    {
        HttpStatusCode.BadRequest.IsBadRequest.Should().BeTrue();
        HttpStatusCode.NotFound.IsBadRequest.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs401ThenIsUnauthorizedShouldBeTrue()
    {
        HttpStatusCode.Unauthorized.IsUnauthorized.Should().BeTrue();
        HttpStatusCode.Forbidden.IsUnauthorized.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs403ThenIsForbiddenShouldBeTrue()
    {
        HttpStatusCode.Forbidden.IsForbidden.Should().BeTrue();
        HttpStatusCode.Unauthorized.IsForbidden.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs404ThenIsNotFoundShouldBeTrue()
    {
        HttpStatusCode.NotFound.IsNotFound.Should().BeTrue();
        HttpStatusCode.BadRequest.IsNotFound.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs500ThenIsInternalServerErrorShouldBeTrue()
    {
        HttpStatusCode.InternalServerError.IsInternalServerError.Should().BeTrue();
        HttpStatusCode.BadGateway.IsInternalServerError.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs501ThenIsNotImplementedShouldBeTrue()
    {
        HttpStatusCode.NotImplemented.IsNotImplemented.Should().BeTrue();
        HttpStatusCode.InternalServerError.IsNotImplemented.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs502ThenIsBadGatewayShouldBeTrue()
    {
        HttpStatusCode.BadGateway.IsBadGateway.Should().BeTrue();
        HttpStatusCode.InternalServerError.IsBadGateway.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs503ThenIsServiceUnavailableShouldBeTrue()
    {
        HttpStatusCode.ServiceUnavailable.IsServiceUnavailable.Should().BeTrue();
        HttpStatusCode.BadGateway.IsServiceUnavailable.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs504ThenIsGatewayTimeoutShouldBeTrue()
    {
        HttpStatusCode.GatewayTimeout.IsGatewayTimeout.Should().BeTrue();
        HttpStatusCode.ServiceUnavailable.IsGatewayTimeout.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs409ThenIsConflictShouldBeTrue()
    {
        HttpStatusCode.Conflict.IsConflict.Should().BeTrue();
        HttpStatusCode.BadRequest.IsConflict.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs408ThenIsRequestTimeoutShouldBeTrue()
    {
        HttpStatusCode.RequestTimeout.IsRequestTimeout.Should().BeTrue();
        HttpStatusCode.GatewayTimeout.IsRequestTimeout.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs405ThenIsMethodNotAllowedShouldBeTrue()
    {
        HttpStatusCode.MethodNotAllowed.IsMethodNotAllowed.Should().BeTrue();
        HttpStatusCode.NotFound.IsMethodNotAllowed.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs429ThenIsTooManyRequestsShouldBeTrue()
    {
        ((HttpStatusCode)429).IsTooManyRequests.Should().BeTrue();
        HttpStatusCode.BadRequest.IsTooManyRequests.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs412ThenIsPreconditionFailedShouldBeTrue()
    {
        HttpStatusCode.PreconditionFailed.IsPreconditionFailed.Should().BeTrue();
        HttpStatusCode.BadRequest.IsPreconditionFailed.Should().BeFalse();
    }

    [Fact]
    public void WhenStatusCodeIs415ThenIsUnsupportedMediaTypeShouldBeTrue()
    {
        HttpStatusCode.UnsupportedMediaType.IsUnsupportedMediaType.Should().BeTrue();
        HttpStatusCode.BadRequest.IsUnsupportedMediaType.Should().BeFalse();
    }

    #endregion

    #region Category Tests

    [Theory]
    [InlineData(HttpStatusCode.Continue)]
    [InlineData(HttpStatusCode.SwitchingProtocols)]
    public void WhenStatusCodeIs1xxThenIsInformationalShouldBeTrue(HttpStatusCode statusCode)
    {
        statusCode.IsInformational.Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.PermanentRedirect)]
    public void WhenStatusCodeIs3xxThenIsRedirectShouldBeTrue(HttpStatusCode statusCode)
    {
        statusCode.IsRedirect.Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public void WhenStatusCodeIs4xxThenIsClientErrorShouldBeTrue(HttpStatusCode statusCode)
    {
        statusCode.IsClientError.Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.NotImplemented)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public void WhenStatusCodeIs5xxThenIsServerErrorShouldBeTrue(HttpStatusCode statusCode)
    {
        statusCode.IsServerError.Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    public void WhenStatusCodeIs4xxThenIsValidationProblemShouldBeTrue(HttpStatusCode statusCode)
    {
        statusCode.IsValidationProblem.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs5xxThenIsValidationProblemShouldBeFalse()
    {
        HttpStatusCode.InternalServerError.IsValidationProblem.Should().BeFalse();
    }

    #endregion

    #region EnsureSuccess/EnsureFailure Tests

    [Fact]
    public void WhenStatusCodeIsSuccessAndEnsureSuccessThenShouldNotThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;

        // Act
        var act = () => statusCode.EnsureSuccess();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WhenStatusCodeIsFailureAndEnsureSuccessThenShouldThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;

        // Act
        var act = () => statusCode.EnsureSuccess();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*BadRequest*");
    }

    [Fact]
    public void WhenStatusCodeIsFailureAndEnsureFailureThenShouldNotThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;

        // Act
        var act = () => statusCode.EnsureFailure();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WhenStatusCodeIsSuccessAndEnsureFailureThenShouldThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.OK;

        // Act
        var act = () => statusCode.EnsureFailure();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*OK*");
    }

    #endregion

    #region Title and Detail Tests

    [Theory]
    [InlineData(HttpStatusCode.OK, "Success")]
    [InlineData(HttpStatusCode.Created, "Created")]
    [InlineData(HttpStatusCode.BadRequest, "Bad Request")]
    [InlineData(HttpStatusCode.Unauthorized, "Unauthorized")]
    [InlineData(HttpStatusCode.NotFound, "Not Found")]
    [InlineData(HttpStatusCode.InternalServerError, "Internal Server Error")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Service Unavailable")]
    public void WhenGettingTitleThenShouldReturnCorrectPhrase(HttpStatusCode statusCode, string expected)
    {
        statusCode.Title.Should().Be(expected);
    }

    [Fact]
    public void WhenGettingDetailForServerErrorThenShouldSuggestContactAdmin()
    {
        HttpStatusCode.InternalServerError.Detail
            .Should().Contain("contact administrator");
    }

    [Fact]
    public void WhenGettingDetailForUnauthorizedThenShouldSuggestContactAdmin()
    {
        HttpStatusCode.Unauthorized.Detail
            .Should().Contain("contact administrator");
    }

    [Fact]
    public void WhenGettingDetailForClientErrorThenShouldReferToErrors()
    {
        HttpStatusCode.BadRequest.Detail
            .Should().Contain("errors property");
    }

    [Fact]
    public void WhenStatusCodeIsUnknownThenTitleShouldBeUnknown()
    {
        ((HttpStatusCode)999).Title.Should().Be("Unknown");
    }

    #endregion

    #region GetException Tests

    [Fact]
    public void WhenBadRequestThenShouldReturnArgumentException()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;

        // Act
        var exception = statusCode.GetException("Invalid input");

        // Assert
        exception.Should().BeOfType<ArgumentException>();
        exception.Message.Should().Be("Invalid input");
    }

    [Fact]
    public void WhenUnauthorizedThenShouldReturnUnauthorizedAccessException()
    {
        // Arrange
        var statusCode = HttpStatusCode.Unauthorized;

        // Act
        var exception = statusCode.GetException("Access denied");

        // Assert
        exception.Should().BeOfType<UnauthorizedAccessException>();
    }

    [Fact]
    public void WhenForbiddenThenShouldReturnSecurityException()
    {
        // Arrange
        var statusCode = HttpStatusCode.Forbidden;

        // Act
        var exception = statusCode.GetException("Forbidden");

        // Assert
        exception.Should().BeOfType<SecurityException>();
    }

    [Fact]
    public void WhenNotFoundThenShouldReturnKeyNotFoundException()
    {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;

        // Act
        var exception = statusCode.GetException("Resource not found");

        // Assert
        exception.Should().BeOfType<KeyNotFoundException>();
    }

    [Fact]
    public void WhenMethodNotAllowedThenShouldReturnNotSupportedException()
    {
        // Arrange
        var statusCode = HttpStatusCode.MethodNotAllowed;

        // Act
        var exception = statusCode.GetException("Method not allowed");

        // Assert
        exception.Should().BeOfType<NotSupportedException>();
    }

    [Fact]
    public void WhenRequestTimeoutThenShouldReturnTimeoutException()
    {
        // Arrange
        var statusCode = HttpStatusCode.RequestTimeout;

        // Act
        var exception = statusCode.GetException("Request timed out");

        // Assert
        exception.Should().BeOfType<TimeoutException>();
    }

    [Fact]
    public void WhenConflictThenShouldReturnIOException()
    {
        // Arrange
        var statusCode = HttpStatusCode.Conflict;

        // Act
        var exception = statusCode.GetException("Conflict");

        // Assert
        exception.Should().BeOfType<IOException>();
    }

    [Fact]
    public void WhenNotImplementedThenShouldReturnNotImplementedException()
    {
        // Arrange
        var statusCode = HttpStatusCode.NotImplemented;

        // Act
        var exception = statusCode.GetException("Not implemented");

        // Assert
        exception.Should().BeOfType<NotImplementedException>();
    }

    [Fact]
    public void WhenBadGatewayThenShouldReturnWebException()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadGateway;

        // Act
        var exception = statusCode.GetException("Bad gateway");

        // Assert
        exception.Should().BeOfType<WebException>();
    }

    [Fact]
    public void WhenGatewayTimeoutThenShouldReturnTimeoutException()
    {
        // Arrange
        var statusCode = HttpStatusCode.GatewayTimeout;

        // Act
        var exception = statusCode.GetException("Gateway timeout");

        // Assert
        exception.Should().BeOfType<TimeoutException>();
    }

    [Fact]
    public void WhenInternalServerErrorThenShouldReturnInvalidOperationException()
    {
        // Arrange
        var statusCode = HttpStatusCode.InternalServerError;

        // Act
        var exception = statusCode.GetException("Server error");

        // Assert
        exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void WhenGetExceptionWithNullMessageThenShouldThrow()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;

        // Act
        var act = () => statusCode.GetException(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenUnknownStatusCodeThenShouldReturnInvalidOperationException()
    {
        // Arrange
        var statusCode = (HttpStatusCode)999;

        // Act
        var exception = statusCode.GetException("Unknown error");

        // Assert
        exception.Should().BeOfType<InvalidOperationException>();
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void WhenBuildingApiResponseThenShouldHaveCorrectProperties()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;

        // Act - Building a standardized error response
        var response = new
        {
            Status = (int)statusCode,
            Title = statusCode.Title,
            Detail = statusCode.Detail,
            IsSuccess = statusCode.IsSuccess,
            IsClientError = statusCode.IsClientError
        };

        // Assert
        response.Status.Should().Be(400);
        response.Title.Should().Be("Bad Request");
        response.IsSuccess.Should().BeFalse();
        response.IsClientError.Should().BeTrue();
    }

    [Fact]
    public void WhenHandlingDifferentHttpResponsesThenShouldCategorizeCorrectly()
    {
        // Arrange
        var responses = new[]
        {
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError
        };

        // Act & Assert
        responses.Count(r => r.IsSuccess).Should().Be(2);
        responses.Count(r => r.IsClientError).Should().Be(3);
        responses.Count(r => r.IsServerError).Should().Be(1);
    }

    [Fact]
    public void WhenImplementingRetryLogicThenShouldIdentifyRetryableErrors()
    {
        // Arrange
        var retryableStatuses = new[]
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };

        // Act & Assert
        foreach (var status in retryableStatuses)
        {
            var isRetryable = status.IsRequestTimeout ||
                             status.IsTooManyRequests ||
                             status.IsServiceUnavailable ||
                             status.IsGatewayTimeout;

            isRetryable.Should().BeTrue($"{status} should be retryable");
        }

        HttpStatusCode.BadRequest.IsRequestTimeout.Should().BeFalse();
        HttpStatusCode.NotFound.IsServiceUnavailable.Should().BeFalse();
    }

    [Fact]
    public void WhenConvertingStatusToExceptionThenShouldPreserveMessage()
    {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;
        const string message = "Customer with ID 12345 was not found";

        // Act
        var exception = statusCode.GetException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Should().BeOfType<KeyNotFoundException>();
    }

    [Fact]
    public void WhenValidatingApiResponseThenShouldIdentifyProblems()
    {
        // Arrange
        var responses = new Dictionary<string, HttpStatusCode>
        {
            ["create_user"] = HttpStatusCode.Created,
            ["get_user"] = HttpStatusCode.OK,
            ["update_user"] = HttpStatusCode.UnprocessableEntity,
            ["delete_user"] = HttpStatusCode.NoContent
        };

        // Act
        var hasValidationProblems = responses.Any(r => r.Value.IsValidationProblem);
        var problemOperations = responses.Where(r => r.Value.IsValidationProblem).Select(r => r.Key);

        // Assert
        hasValidationProblems.Should().BeTrue();
        problemOperations.Should().Contain("update_user");
    }

    #endregion

    #region Additional Status Codes Tests

    [Fact]
    public void WhenStatusCodeIs411ThenIsLengthRequiredShouldBeTrue()
    {
        HttpStatusCode.LengthRequired.IsLengthRequired.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs413ThenIsRequestEntityTooLargeShouldBeTrue()
    {
        HttpStatusCode.RequestEntityTooLarge.IsRequestEntityTooLarge.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs414ThenIsRequestUriTooLongShouldBeTrue()
    {
        HttpStatusCode.RequestUriTooLong.IsRequestUriTooLong.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs416ThenIsRequestedRangeNotSatisfiableShouldBeTrue()
    {
        HttpStatusCode.RequestedRangeNotSatisfiable.IsRequestedRangeNotSatisfiable.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs417ThenIsExpectationFailedShouldBeTrue()
    {
        HttpStatusCode.ExpectationFailed.IsExpectationFailed.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs426ThenIsUpgradeRequiredShouldBeTrue()
    {
        HttpStatusCode.UpgradeRequired.IsUpgradeRequired.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs428ThenIsPreconditionRequiredShouldBeTrue()
    {
        ((HttpStatusCode)428).IsPreconditionRequired.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs431ThenIsRequestHeaderFieldsTooLargeShouldBeTrue()
    {
        ((HttpStatusCode)431).IsRequestHeaderFieldsTooLarge.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs451ThenIsUnavailableForLegalReasonsShouldBeTrue()
    {
        ((HttpStatusCode)451).IsUnavailableForLegalReasons.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs505ThenIsHttpVersionNotSupportedShouldBeTrue()
    {
        HttpStatusCode.HttpVersionNotSupported.IsHttpVersionNotSupported.Should().BeTrue();
    }

    [Fact]
    public void WhenStatusCodeIs511ThenIsNetworkAuthenticationRequiredShouldBeTrue()
    {
        ((HttpStatusCode)511).IsNetworkAuthenticationRequired.Should().BeTrue();
    }

    #endregion
}
