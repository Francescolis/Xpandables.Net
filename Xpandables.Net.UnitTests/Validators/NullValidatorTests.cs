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
using FluentAssertions;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.UnitTests.Validators;

public class NullValidatorTests
{
    [Fact]
    public void NullValidator_WithAnyObject_ShouldAlwaysReturnEmptyResults()
    {
        // Arrange
        var validator = new NullValidator<TestModel>();
        var testModel = new TestModel
        {
            Name = "", // This would normally fail validation
            Age = 150, // This would normally fail validation
            Email = "invalid-email" // This would normally fail validation
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task NullValidator_ValidateAsync_ShouldAlwaysReturnEmptyResults()
    {
        // Arrange
        var validator = new NullValidator<TestModel>();
        var testModel = new TestModel
        {
            Name = "", // This would normally fail validation
            Age = 150, // This would normally fail validation
            Email = "invalid-email" // This would normally fail validation
        };

        // Act
        var results = await validator.ValidateAsync(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void NullValidator_WithValidObject_ShouldReturnEmptyResults()
    {
        // Arrange
        var validator = new NullValidator<TestModel>();
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
    public void NullValidator_WithEmptyModel_ShouldReturnEmptyResults()
    {
        // Arrange
        var validator = new NullValidator<EmptyTestModel>();
        var testModel = new EmptyTestModel();

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void NullValidator_WithComplexInvalidModel_ShouldIgnoreAllValidationAndReturnEmpty()
    {
        // Arrange
        var validator = new NullValidator<ComplexTestModel>();
        var testModel = new ComplexTestModel
        {
            Title = "", // Required but empty
            Value = 0,  // Must be greater than 1
            Items = []  // Must have at least 1 item
        };

        // Act
        var results = validator.Validate(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void NullValidator_InheritsFromValidator()
    {
        // Arrange
        var validator = new NullValidator<TestModel>();

        // Act & Assert
        validator.Should().BeAssignableTo<Validator<TestModel>>();
        validator.Should().BeAssignableTo<IValidator<TestModel>>();
        validator.Should().BeAssignableTo<IValidator>();
    }

    [Fact]
    public void NullValidator_IsSealed()
    {
        // Act & Assert
        typeof(NullValidator<TestModel>).IsSealed.Should().BeTrue();
    }
}