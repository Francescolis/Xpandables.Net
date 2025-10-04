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
using System.Net.Validators;

using FluentAssertions;

namespace System.Net.UnitTests.Validators;

public class CompositeValidatorTests
{
    [Fact]
    public void CompositeValidator_WithNullValidators_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new CompositeValidator<TestModel>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("validators");
    }

    [Fact]
    public void CompositeValidator_WithEmptyValidators_ShouldReturnEmptyResults()
    {
        // Arrange
        var validators = Array.Empty<IValidator<TestModel>>();
        var compositeValidator = new CompositeValidator<TestModel>(validators);
        var testModel = new TestModel { Name = "John Doe", Age = 25 };

        // Act
        var results = compositeValidator.Validate(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CompositeValidator_WithSingleValidator_ShouldReturnSingleValidatorResults()
    {
        // Arrange
        var validator = new CustomTestValidator();
        var validators = new List<IValidator<TestModel>> { validator };
        var compositeValidator = new CompositeValidator<TestModel>(validators);

        var testModel = new TestModel
        {
            Name = "", // This will trigger validation errors
            Age = -5,  // This will trigger custom validation
            Email = "john@example.com"
        };

        // Act
        var results = compositeValidator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.ErrorMessage == "Age cannot be negative");
    }

    [Fact]
    public void CompositeValidator_WithMultipleValidators_ShouldCombineAllResults()
    {
        // Arrange
        var validator1 = new AlwaysFailValidator();
        var validator2 = new SecondValidator();
        var validators = new List<IValidator<TestModel>> { validator1, validator2 };
        var compositeValidator = new CompositeValidator<TestModel>(validators);

        var testModel = new TestModel
        {
            Name = "Jo", // Short name will trigger SecondValidator
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = compositeValidator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.ErrorMessage == "Always fails");
        results.Should().Contain(r => r.ErrorMessage == "Name must be at least 5 characters");
    }

    [Fact]
    public void CompositeValidator_WithMixOfValidatorsPassingAndFailing_ShouldOnlyReturnFailures()
    {
        // Arrange
        var validValidator = new NullValidator<TestModel>(); // Never fails
        var failingValidator = new AlwaysFailValidator(); // Always fails
        var validators = new List<IValidator<TestModel>> { validValidator, failingValidator };
        var compositeValidator = new CompositeValidator<TestModel>(validators);

        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = compositeValidator.Validate(testModel);

        // Assert
        results.Should().HaveCount(1);
        results.Should().Contain(r => r.ErrorMessage == "Always fails");
    }

    [Fact]
    public async Task CompositeValidator_ValidateAsync_ShouldCombineAllAsyncResults()
    {
        // Arrange
        var validator1 = new AsyncTestValidator();
        var validator2 = new AlwaysFailValidator();
        var validators = new List<IValidator<TestModel>> { validator1, validator2 };
        var compositeValidator = new CompositeValidator<TestModel>(validators);

        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 110, // This will trigger async validation
            Email = "john@example.com"
        };

        // Act
        var results = await compositeValidator.ValidateAsync(testModel);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.ErrorMessage == "Age is too high for async validation");
        results.Should().Contain(r => r.ErrorMessage == "Always fails");
    }

    [Fact]
    public async Task CompositeValidator_ValidateAsync_WithAllValidValidators_ShouldReturnEmpty()
    {
        // Arrange
        var validator1 = new NullValidator<TestModel>();
        var validator2 = new NullValidator<TestModel>();
        var validators = new List<IValidator<TestModel>> { validator1, validator2 };
        var compositeValidator = new CompositeValidator<TestModel>(validators);

        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = await compositeValidator.ValidateAsync(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CompositeValidator_InheritsFromCorrectTypes()
    {
        // Arrange
        var validators = new List<IValidator<TestModel>>();
        var compositeValidator = new CompositeValidator<TestModel>(validators);

        // Act & Assert
        compositeValidator.Should().BeAssignableTo<Validator<TestModel>>();
        compositeValidator.Should().BeAssignableTo<IValidator<TestModel>>();
        compositeValidator.Should().BeAssignableTo<ICompositeValidator<TestModel>>();
        compositeValidator.Should().BeAssignableTo<IValidator>();
    }

    [Fact]
    public void CompositeValidator_IsSealed()
    {
        // Act & Assert
        typeof(CompositeValidator<TestModel>).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void CompositeValidator_WithValidatorsReturningEmptyResults_ShouldReturnEmpty()
    {
        // Arrange
        var validator1 = new Validator<TestModel>();
        var validator2 = new NullValidator<TestModel>();
        var validators = new List<IValidator<TestModel>> { validator1, validator2 };
        var compositeValidator = new CompositeValidator<TestModel>(validators);

        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var results = compositeValidator.Validate(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CompositeValidator_WithComplexModel_ShouldValidateUsingAllValidators()
    {
        // Arrange
        var validator1 = new Validator<ComplexTestModel>();
        var validator2 = new NullValidator<ComplexTestModel>();
        var validators = new List<IValidator<ComplexTestModel>> { validator1, validator2 };
        var compositeValidator = new CompositeValidator<ComplexTestModel>(validators);

        var testModel = new ComplexTestModel
        {
            Title = "", // Required but empty
            Value = 0,  // Must be greater than 1
            Items = []  // Must have at least 1 item
        };

        // Act
        var results = compositeValidator.Validate(testModel);

        // Assert
        // Only the first validator should produce errors (NullValidator always returns empty)
        results.Should().NotBeEmpty();
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ComplexTestModel.Title)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ComplexTestModel.Value)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ComplexTestModel.Items)));
    }

    [Fact]
    public void CompositeValidator_ValidateExecutesInOrder()
    {
        // Arrange
        var executionOrder = new List<string>();

        var validator1 = new TrackingValidator("First", executionOrder);
        var validator2 = new TrackingValidator("Second", executionOrder);
        var validator3 = new TrackingValidator("Third", executionOrder);

        var validators = new List<IValidator<TestModel>> { validator1, validator2, validator3 };
        var compositeValidator = new CompositeValidator<TestModel>(validators);

        var testModel = new TestModel { Name = "John Doe", Age = 25, Email = "john@example.com" };

        // Act
        compositeValidator.Validate(testModel);

        // Assert
        executionOrder.Should().Equal("First", "Second", "Third");
    }

    private class TrackingValidator(string name, List<string> executionOrder) : Validator<TestModel>
    {
        private readonly string _name = name;
        private readonly List<string> _executionOrder = executionOrder;

        public override IReadOnlyCollection<ValidationResult> Validate(TestModel instance)
        {
            _executionOrder.Add(_name);
            return [];
        }
    }
}