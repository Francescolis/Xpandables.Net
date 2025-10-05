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

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests.Validators;

[Collection("sqlite-shared-db")]
public class ValidatorsIntegrationTests
{
    [Fact]
    public async Task AsyncValidationWorkflow_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXValidator()
               .AddXValidatorProvider();

        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IValidatorProvider>();

        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 25,
            Email = "john@example.com"
        };

        // Act
        var validator = provider.TryGetValidator<TestModel>();
        var results = await validator!.ValidateAsync(testModel);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ValidatorWithSpecification_ShouldCombineValidation()
    {
        // Arrange
        var validator = new Validator<TestModel>();
        var ageSpecification = new Specification<TestModel>(x => x.Age >= 21); // Drinking age

        var validModel = new TestModel { Name = "John Doe", Age = 25, Email = "john@example.com" };
        var invalidDataAnnotationModel = new TestModel { Name = "", Age = 25, Email = "john@example.com" };
        var invalidSpecificationModel = new TestModel { Name = "Jane Doe", Age = 20, Email = "jane@example.com" };

        // Act
        var dataAnnotationResults = validator.Validate(invalidDataAnnotationModel);
        var specificationResult = ageSpecification.IsSatisfiedBy(invalidSpecificationModel);
        var bothValidResult = validator.Validate(validModel);
        var specificationValidResult = ageSpecification.IsSatisfiedBy(validModel);

        // Assert
        dataAnnotationResults.Should().NotBeEmpty(); // Data annotation validation fails
        specificationResult.Should().BeFalse(); // Specification validation fails
        bothValidResult.Should().BeEmpty(); // Data annotation validation passes
        specificationValidResult.Should().BeTrue(); // Specification validation passes
    }

    [Fact]
    public void CompositeValidatorWithMultipleValidators_ShouldAggregateResults()
    {
        // Arrange
        var validators = new List<IValidator<TestModel>>
        {
            new CustomTestValidator(),
            new SecondValidator(),
            new AlwaysFailValidator()
        };

        var compositeValidator = new CompositeValidator<TestModel>(validators);

        var testModel = new TestModel
        {
            Name = "Jo", // Too short for SecondValidator
            Age = -5,   // Invalid for CustomTestValidator
            Email = "john@example.com"
        };

        // Act
        var results = compositeValidator.Validate(testModel);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCountGreaterThan(2); // Multiple validators should contribute errors
    }

    [Fact]
    public void SpecificationWithQueryable_ShouldFilter()
    {
        // Arrange
        var models = new List<TestModel>
        {
            new() { Name = "Alice", Age = 25, Email = "alice@example.com" },
            new() { Name = "Bob", Age = 17, Email = "bob@example.com" },
            new() { Name = "Charlie", Age = 30, Email = "charlie@example.com" },
            new() { Name = "Diana", Age = 16, Email = "diana@example.com" }
        };

        var adultSpecification = new Specification<TestModel>(x => x.Age >= 18);

        // Act
        var adults = models.AsQueryable().Where(adultSpecification).ToList();

        // Assert
        adults.Should().HaveCount(2);
        adults.Should().Contain(m => m.Name == "Alice");
        adults.Should().Contain(m => m.Name == "Charlie");
    }

    [Fact]
    public void ValidatorProvider_WithCustomValidator_ShouldPreferCustomOverDefault()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXValidator();
        services.AddTransient<IValidator<TestModel>, CustomTestValidator>(); // Custom validator
        services.AddXValidatorProvider();

        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IValidatorProvider>();

        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = -5, // This should trigger custom validation logic
            Email = "john@example.com"
        };

        // Act
        var validator = provider.TryGetValidator<TestModel>();
        var results = validator!.Validate(testModel);

        // Assert
        validator.Should().BeOfType<CustomTestValidator>();
        results.Should().Contain(r => r.ErrorMessage == "Age cannot be negative");
    }

    [Fact]
    public void NullValidator_ShouldBypassAllValidation()
    {
        // Arrange
        var nullValidator = new NullValidator<TestModel>();
        var compositeValidator = new CompositeValidator<TestModel>([nullValidator]);

        var completelyInvalidModel = new TestModel
        {
            Name = "", // Required field empty
            Age = -100, // Negative age
            Email = "totally-invalid-email" // Invalid email
        };

        // Act
        var nullValidatorResults = nullValidator.Validate(completelyInvalidModel);
        var compositeResults = compositeValidator.Validate(completelyInvalidModel);

        // Assert
        nullValidatorResults.Should().BeEmpty(); // Null validator ignores everything
        compositeResults.Should().BeEmpty(); // Composite with only null validator also ignores
    }

    [Fact]
    public void RealWorldScenario_UserRegistrationValidation()
    {
        // Arrange - Simulate a user registration system
        var services = new ServiceCollection();
        services.AddTransient<IValidator<TestModel>, UserRegistrationValidator>();
        services.AddXValidatorProvider();

        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IValidatorProvider>();

        var validUser = new TestModel
        {
            Name = "John Doe Smith",
            Age = 25,
            Email = "john.doe@company.com"
        };

        var invalidUsers = new[]
        {
            new TestModel { Name = "Jo", Age = 25, Email = "john@company.com" }, // Name too short
            new TestModel { Name = "John Doe", Age = 17, Email = "john@company.com" }, // Under 18
            new TestModel { Name = "John Doe", Age = 25, Email = "johncompany.com" } // Invalid email
        };

        // Act
        var validator = provider.TryGetValidator<TestModel>();
        var validUserResults = validator!.Validate(validUser);

        var invalidUserResults = invalidUsers.Select(user => new
        {
            User = user,
            Results = validator.Validate(user)
        }).ToList();

        // Assert
        validUserResults.Should().BeEmpty();

        foreach (var result in invalidUserResults)
        {
            result.Results.Should().NotBeEmpty($"User {result.User.Name} should have validation errors");
        }
    }

    private class UserRegistrationValidator : Validator<TestModel>
    {
        public override IReadOnlyCollection<ValidationResult> Validate(TestModel instance)
        {
            var results = base.Validate(instance).ToList();

            // Additional business rules for user registration
            if (instance.Age < 18)
            {
                results.Add(new ValidationResult("Must be at least 18 years old to register", [nameof(TestModel.Age)]));
            }

            if (!string.IsNullOrEmpty(instance.Name) && instance.Name.Length < 3)
            {
                results.Add(new ValidationResult("Full name must be at least 3 characters", [nameof(TestModel.Name)]));
            }

            if (!string.IsNullOrEmpty(instance.Email) && !instance.Email.Contains('@'))
            {
                results.Add(new ValidationResult("Email must contain @ symbol", [nameof(TestModel.Email)]));
            }

            return results;
        }
    }
}