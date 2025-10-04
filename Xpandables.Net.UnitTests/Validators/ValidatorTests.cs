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
using System.Net.Validators;

using FluentAssertions;

using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests.Validators;

public class ValidatorTests
{
    [Fact]
    public void Validator_WithValidObject_ShouldReturnEmptyResults()
    {
        // Arrange
        var validator = new Validator<TestModel>();
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validator_WithInvalidObject_ShouldReturnValidationErrors()
    {
        // Arrange
        var validator = new Validator<TestModel>();
        var testModel = new TestModel
        {
            Name = "", // Required field empty
            Age = 150, // Out of range
            Email = "invalid-email" // Invalid email format
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.MemberNames.Contains(nameof(TestModel.Name)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(TestModel.Age)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(TestModel.Email)));
    }

    [Fact]
    public async Task ValidateAsync_WithValidObject_ShouldReturnEmptyResults()
    {
        // Arrange
        var validator = new Validator<TestModel>();
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = await validator.ValidateAsync(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidObject_ShouldReturnValidationErrors()
    {
        // Arrange
        var validator = new Validator<TestModel>();
        var testModel = new TestModel
        {
            Name = "", // Required field empty
            Age = 150, // Out of range
            Email = "invalid-email" // Invalid email format
        };

        // Act
        var results = await validator.ValidateAsync(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(3);
    }

    [Fact]
    public void Validator_WithEmptyModel_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var validator = new Validator<EmptyTestModel>();
        var testModel = new EmptyTestModel();

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CustomValidator_ShouldExecuteCustomLogic()
    {
        // Arrange
        var validator = new CustomTestValidator();
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = -5, // This should trigger custom validation
            Email = "john@example.com"
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.ErrorMessage == "Age cannot be negative");
    }

    [Fact]
    public void CustomValidator_WithLongEmail_ShouldTriggerCustomValidation()
    {
        // Arrange
        var validator = new CustomTestValidator();
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = new string('a', 101) // Very long email
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().Contain(r => r.ErrorMessage == "Email is too long");
    }

    [Fact]
    public async Task AsyncValidator_ShouldExecuteAsyncLogic()
    {
        // Arrange
        var validator = new AsyncTestValidator();
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 110, // This should trigger async validation
            Email = "john@example.com"
        };

        // Act
        var results = await validator.ValidateAsync(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.ErrorMessage == "Age is too high for async validation");
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData(null)]
    public void Validator_WithInvalidName_ShouldReturnNameValidationError(string? invalidName)
    {
        // Arrange
        var validator = new Validator<TestModel>();
        var testModel = new TestModel
        {
            Name = invalidName!,
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(TestModel.Name)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    [InlineData(int.MaxValue)]
    public void Validator_WithInvalidAge_ShouldReturnAgeValidationError(int invalidAge)
    {
        // Arrange
        var validator = new Validator<TestModel>();
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = invalidAge,
            Email = "john@example.com"
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(TestModel.Age)));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@invalid.com")]
    [InlineData("invalid@")]
    [InlineData("invalid")]
    public void Validator_WithInvalidEmail_ShouldReturnEmailValidationError(string invalidEmail)
    {
        // Arrange
        var validator = new Validator<TestModel>();
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = invalidEmail
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(TestModel.Email)));
    }

    [Fact]
    public void IValidator_NonGeneric_Validate_ShouldWorkWithObjectParameter()
    {
        // Arrange
        IValidator validator = new Validator<TestModel>();
        object testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task IValidator_NonGeneric_ValidateAsync_ShouldWorkWithObjectParameter()
    {
        // Arrange
        IValidator validator = new Validator<TestModel>();
        object testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = await validator.ValidateAsync(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validator_WithComplexModel_ShouldValidateAllProperties()
    {
        // Arrange
        var validator = new Validator<ComplexTestModel>();
        var testModel = new ComplexTestModel
        {
            Title = "", // Required but empty
            Value = 0,  // Must be greater than 1
            Items = []  // Must have at least 1 item
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ComplexTestModel.Title)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ComplexTestModel.Value)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ComplexTestModel.Items)));
    }
}