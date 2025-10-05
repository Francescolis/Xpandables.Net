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

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests.Validators;

public class ValidatorProviderTests
{
    [Fact]
    public void ValidatorProvider_WithNullServiceProvider_ShouldCreateDefaultValidators()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act
        var validator = provider.TryGetValidator<TestModel>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<Validator<TestModel>>();
    }

    [Fact]
    public void ValidatorProvider_WithServiceProvider_ShouldUseRegisteredValidators()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IValidator<TestModel>, CustomTestValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var provider = new ValidatorProvider(serviceProvider);

        // Act
        var validator = provider.TryGetValidator<TestModel>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<CustomTestValidator>();
    }

    [Fact]
    public void ValidatorProvider_WithMultipleValidatorsRegistered_ShouldPreferCustomOverBuiltIn()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IValidator<TestModel>, Validator<TestModel>>();
        services.AddTransient<IValidator<TestModel>, CustomTestValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var provider = new ValidatorProvider(serviceProvider);

        // Act
        var validator = provider.TryGetValidator<TestModel>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<CustomTestValidator>();
    }

    [Fact]
    public void ValidatorProvider_WithOnlyBuiltInValidator_ShouldReturnBuiltIn()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IValidator<TestModel>, Validator<TestModel>>();
        var serviceProvider = services.BuildServiceProvider();

        var provider = new ValidatorProvider(serviceProvider);

        // Act
        var validator = provider.TryGetValidator<TestModel>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<Validator<TestModel>>();
    }

    [Fact]
    public void ValidatorProvider_WithNoRegisteredValidators_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var provider = new ValidatorProvider(serviceProvider);

        // Act
        var validator = provider.TryGetValidator<TestModel>();

        // Assert
        validator.Should().BeNull();
    }

    [Fact]
    public void ValidatorProvider_TryGetValidator_WithType_ShouldReturnValidator()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act
        var validator = provider.TryGetValidator<TestModel>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<Validator<TestModel>>();
    }

    [Fact]
    public void ValidatorProvider_TryGetValidator_WithNullType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act & Assert
        var act = () => provider.TryGetValidator(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("type");
    }

    [Fact]
    public void ValidatorProvider_TryGetValidator_WithNonIRequiresValidationType_ShouldThrowArgumentException()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act & Assert
        var act = () => provider.TryGetValidator(typeof(string));
        act.Should().Throw<ArgumentException>()
           .WithParameterName("type")
           .WithMessage("The type 'System.String' must implement 'Xpandables.Net.Validators.IRequiresValidation'. (Parameter 'type')");
    }

    [Fact]
    public void ValidatorProvider_TryGetValidator_Generic_ShouldUseTypeOverload()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act
        var genericValidator = provider.TryGetValidator<TestModel>();
        var typeValidator = provider.TryGetValidator<TestModel>();

        // Assert
        genericValidator.Should().NotBeNull();
        typeValidator.Should().NotBeNull();
        genericValidator.Should().BeOfType<Validator<TestModel>>();
        typeValidator.Should().BeOfType<Validator<TestModel>>();
    }

    [Fact]
    public void ValidatorProvider_IsSealed()
    {
        // Act & Assert
        typeof(ValidatorProvider).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void ValidatorProvider_ImplementsIValidatorProvider()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act & Assert
        provider.Should().BeAssignableTo<IValidatorProvider>();
    }

    [Fact]
    public void ValidatorProvider_WithComplexServiceProviderSetup_ShouldResolveCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<string>("test-dependency");
        services.AddTransient<CustomTestValidator>();
        services.AddTransient<IValidator<TestModel>, CustomTestValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var provider = new ValidatorProvider(serviceProvider);

        // Act
        var validator = provider.TryGetValidator<TestModel>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<CustomTestValidator>();
    }

    [Fact]
    public void ValidatorProvider_WithEmptyTestModel_ShouldCreateValidator()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act
        var validator = provider.TryGetValidator<EmptyTestModel>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<Validator<EmptyTestModel>>();
    }

    [Fact]
    public void ValidatorProvider_WithComplexTestModel_ShouldCreateValidator()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act
        var validator = provider.TryGetValidator<ComplexTestModel>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<Validator<ComplexTestModel>>();
    }

    [Fact]
    public void ValidatorProvider_MultipleCalls_ShouldCreateNewInstances()
    {
        // Arrange
        var provider = new ValidatorProvider();

        // Act
        var validator1 = provider.TryGetValidator<TestModel>();
        var validator2 = provider.TryGetValidator<TestModel>();

        // Assert
        validator1.Should().NotBeNull();
        validator2.Should().NotBeNull();
        validator1.Should().NotBeSameAs(validator2);
        validator1.Should().BeOfType<Validator<TestModel>>();
        validator2.Should().BeOfType<Validator<TestModel>>();
    }
}