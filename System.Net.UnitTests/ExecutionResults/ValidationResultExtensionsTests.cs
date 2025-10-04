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
using System.Net.Abstractions;
using System.Net.ExecutionResults;

using FluentAssertions;

namespace System.Net.UnitTests.ExecutionResults;

/// <summary>
/// Tests for ValidationResult extension methods.
/// </summary>
public class ValidationResultExtensionsTests
{
    [Fact]
    public void ToExecutionResult_WithValidationResults_ShouldCreateBadRequestResult()
    {
        // Arrange
        var validationResults = new[]
        {
            new ValidationResult("Field is required", ["Field1"]),
            new ValidationResult("Invalid format", ["Field2"]),
            new ValidationResult("Multiple errors", ["Field3", "Field4"])
        };

        // Act
        var result = validationResults.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Title.Should().Be("Bad Request");
        result.Detail.Should().Be("Please refer to the errors property for additional details");
        result.IsSuccess.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToExecutionResult_WithSingleValidationResult_ShouldCreateResult()
    {
        // Arrange
        var validationResults = new[]
        {
            new ValidationResult("Username is required", ["Username"])
        };

        // Act
        var result = validationResults.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.ContainsKey("Username").Should().BeTrue();
    }

    [Fact]
    public void ToExecutionResult_WithMultipleValidationResultsForSameField_ShouldGroupErrors()
    {
        // Arrange
        var validationResults = new[]
        {
            new ValidationResult("Field is required", ["Email"]),
            new ValidationResult("Invalid email format", ["Email"]),
            new ValidationResult("Email already exists", ["Email"])
        };

        // Act
        var result = validationResults.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.ContainsKey("Email").Should().BeTrue();

        result.Errors.TryGetValue("Email", out var emailEntry).Should().BeTrue();
        emailEntry.Values.Count.Should().Be(3);
    }

    [Fact]
    public void ToExecutionResult_WithValidationResultsHavingMultipleMemberNames_ShouldCreateMultipleEntries()
    {
        // Arrange
        var validationResults = new[]
        {
            new ValidationResult("Password confirmation must match", ["Password", "ConfirmPassword"])
        };

        // Act
        var result = validationResults.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.ContainsKey("Password").Should().BeTrue();
        result.Errors.ContainsKey("ConfirmPassword").Should().BeTrue();
    }

    [Fact]
    public void ToExecutionResult_WithEmptyValidationResults_ShouldThrow()
    {
        // Arrange
        var emptyValidationResults = Array.Empty<ValidationResult>();

        // Act & Assert
        Action action = () => emptyValidationResults.ToExecutionResult();
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToExecutionResult_WithNullValidationResults_ShouldThrow()
    {
        // Arrange
        IEnumerable<ValidationResult>? nullValidationResults = null;

        // Act & Assert
        Action action = () => nullValidationResults!.ToExecutionResult();
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToExecutionResult_WithComplexValidationScenario_ShouldHandleCorrectly()
    {
        // Arrange - Simulate a complex form validation
        var validationResults = new[]
        {
            new ValidationResult("First name is required", ["FirstName"]),
            new ValidationResult("Last name is required", ["LastName"]),
            new ValidationResult("Email is required", ["Email"]),
            new ValidationResult("Email format is invalid", ["Email"]),
            new ValidationResult("Password must be at least 8 characters", ["Password"]),
            new ValidationResult("Password must contain uppercase letter", ["Password"]),
            new ValidationResult("Password must contain special character", ["Password"]),
            new ValidationResult("Age must be between 18 and 120", ["Age"]),
            new ValidationResult("Terms of service must be accepted", ["AcceptTerms"])
        };

        // Act
        var result = validationResults.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Title.Should().Be("Bad Request");
        result.Detail.Should().Be("Please refer to the errors property for additional details");
        result.IsSuccess.Should().BeFalse();

        // Verify specific fields have errors
        result.Errors.ContainsKey("FirstName").Should().BeTrue();
        result.Errors.ContainsKey("LastName").Should().BeTrue();
        result.Errors.ContainsKey("Email").Should().BeTrue();
        result.Errors.ContainsKey("Password").Should().BeTrue();
        result.Errors.ContainsKey("Age").Should().BeTrue();
        result.Errors.ContainsKey("AcceptTerms").Should().BeTrue();

        // Verify Password field has multiple errors
        result.Errors.TryGetValue("Password", out var passwordEntry).Should().BeTrue();
        passwordEntry.Values.Count.Should().Be(3);

        // Verify Email field has multiple errors
        result.Errors.TryGetValue("Email", out var emailEntry).Should().BeTrue();
        emailEntry.Values.Count.Should().Be(2);
    }

    [Fact]
    public void ToExecutionResult_WithValidationResultsContainingNullMessages_ShouldHandleGracefully()
    {
        // Arrange
        var validationResults = new[]
        {
            new ValidationResult("Valid error message", ["Field1"]),
            new ValidationResult(null, ["Field2"]), // Null error message
            new ValidationResult("", ["Field3"]) // Empty error message
        };

        // Act
        var result = validationResults.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        // The exact behavior depends on ToElementCollection implementation
        // but the method should not throw
    }

    [Fact]
    public void ToExecutionResult_WithValidationResultsContainingEmptyMemberNames_ShouldHandleGracefully()
    {
        // Arrange
        var validationResults = new[]
        {
            new ValidationResult("Error with valid field", ["ValidField"]),
            new ValidationResult("Error with empty field name", [""]),
            new ValidationResult("Error with null field name", [null!])
        };

        // Act
        var result = validationResults.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Errors.ContainsKey("ValidField").Should().BeTrue();
        // The exact handling of empty/null member names depends on ToElementCollection implementation
    }

    [Fact]
    public void ToExecutionResult_ResultProperties_ShouldBeSetCorrectly()
    {
        // Arrange
        var validationResults = new[]
        {
            new ValidationResult("Test validation error", ["TestField"])
        };

        // Act
        var result = validationResults.ToExecutionResult();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Title.Should().Be(HttpStatusCode.BadRequest.Title);
        result.Detail.Should().Be(HttpStatusCode.BadRequest.Detail);
        result.IsSuccess.Should().BeFalse();
        result.Location.Should().BeNull();
        result.Value.Should().BeNull();
        result.Headers.Count.Should().Be(0);
        result.Extensions.Count.Should().Be(0);
        result.Errors.Count.Should().BeGreaterThan(0);
    }
}