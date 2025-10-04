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
using System.Net.DependencyInjection;
using System.Net.Validators;
using System.Reflection;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests.Validators;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddXValidator_ShouldRegisterGenericValidatorTypes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXValidator();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var validatorService = serviceProvider.GetService<IValidator<TestModel>>();
        var compositeValidatorService = serviceProvider.GetService<ICompositeValidator<TestModel>>();

        validatorService.Should().NotBeNull();
        validatorService.Should().BeOfType<Validator<TestModel>>();

        compositeValidatorService.Should().NotBeNull();
        compositeValidatorService.Should().BeOfType<CompositeValidator<TestModel>>();
    }

    [Fact]
    public void AddXValidator_ShouldRegisterWithTransientLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXValidator();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var validator1 = serviceProvider.GetService<IValidator<TestModel>>();
        var validator2 = serviceProvider.GetService<IValidator<TestModel>>();

        validator1.Should().NotBeNull();
        validator2.Should().NotBeNull();
        validator1.Should().NotBeSameAs(validator2);
    }

    [Fact]
    public void AddXValidator_ShouldNotOverrideExistingRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IValidator<TestModel>, CustomTestValidator>();

        // Act
        services.AddXValidator();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var validators = serviceProvider.GetServices<IValidator<TestModel>>().ToList();
        validators.Should().HaveCount(2);
        validators.Should().Contain(v => v.GetType() == typeof(CustomTestValidator));
        validators.Should().Contain(v => v.GetType() == typeof(Validator<TestModel>));
    }

    [Fact]
    public void AddXValidatorProvider_Generic_ShouldRegisterCustomProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXValidatorProvider<CustomValidatorProvider>();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IValidatorProvider>();
        provider.Should().NotBeNull();
        provider.Should().BeOfType<CustomValidatorProvider>();
    }

    [Fact]
    public void AddXValidatorProvider_Instance_ShouldRegisterProvidedInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var customProvider = new CustomValidatorProvider();

        // Act
        services.AddXValidatorProvider(customProvider);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IValidatorProvider>();
        provider.Should().NotBeNull();
        provider.Should().BeSameAs(customProvider);
    }

    [Fact]
    public void AddXValidatorProvider_WithNullProvider_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var act = () => services.AddXValidatorProvider(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("provider");
    }

    [Fact]
    public void AddXValidatorProvider_Default_ShouldRegisterValidatorProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXValidatorProvider();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IValidatorProvider>();
        provider.Should().NotBeNull();
        provider.Should().BeOfType<ValidatorProvider>();
    }

    [Fact]
    public void AddXValidatorProvider_ShouldReplaceExistingRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidatorProvider, ValidatorProvider>();

        // Act
        services.AddXValidatorProvider<CustomValidatorProvider>();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var providers = serviceProvider.GetServices<IValidatorProvider>().ToList();
        providers.Should().HaveCount(1);
        providers[0].Should().BeOfType<CustomValidatorProvider>();
    }

    [Fact]
    public void AddXValidators_WithNoAssemblies_ShouldScanCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXValidators();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Should register validators from the current test assembly
        var validators = serviceProvider.GetServices<IValidator<TestModel>>().ToList();
        validators.Should().NotBeEmpty();
    }

    [Fact]
    public void AddXValidators_WithSpecificAssembly_ShouldScanProvidedAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetAssembly(typeof(TestModel))!;

        // Act
        services.AddXValidators(assembly!);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var validators = serviceProvider.GetServices<IValidator<TestModel>>().ToList();
        validators.Should().NotBeEmpty();
    }

    [Fact]
    public void AddXValidators_ShouldRegisterSealedValidatorsOnly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXValidators();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Should find sealed validators like CustomTestValidator, AlwaysFailValidator, etc.
        var validators = serviceProvider.GetServices<IValidator<TestModel>>().ToList();
        validators.Should().Contain(v => v.GetType() == typeof(AlwaysFailValidator));
        validators.Should().Contain(v => v.GetType() == typeof(SecondValidator));
        validators.Should().Contain(v => v.GetType() == typeof(AsyncTestValidator));
    }

    [Fact]
    public void AddXValidators_ShouldRegisterCompositeValidatorsForDiscoveredTypes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXValidators();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var compositeValidators = serviceProvider.GetServices<ICompositeValidator<TestModel>>().ToList();
        compositeValidators.Should().NotBeEmpty();
    }

    [Fact]
    public void CompleteWorkflow_AddingAllServices_ShouldWorkTogether()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXValidator()
               .AddXValidatorProvider()
               .AddXValidators();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetRequiredService<IValidatorProvider>();
        var validator = provider.TryGetValidator<TestModel>();
        var compositeValidator = serviceProvider.GetService<ICompositeValidator<TestModel>>();

        provider.Should().NotBeNull();
        validator.Should().NotBeNull();
        compositeValidator.Should().NotBeNull();
    }

    [Fact]
    public void Extensions_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        services.AddXValidator()
               .Should().BeSameAs(services);

        services.AddXValidatorProvider()
               .Should().BeSameAs(services);

        services.AddXValidatorProvider<CustomValidatorProvider>()
               .Should().BeSameAs(services);

        services.AddXValidators()
               .Should().BeSameAs(services);
    }

    private class CustomValidatorProvider : IValidatorProvider
    {
        public IValidator? TryGetValidator(Type type) => null;
        public IValidator? TryGetValidator<TArgument>() where TArgument : class, IRequiresValidation => null;
    }
}