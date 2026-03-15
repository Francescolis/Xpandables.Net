/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring and registering services with an IServiceCollection.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of IServiceCollection,
/// enabling additional service registration patterns and configuration options commonly used in dependency injection
/// scenarios.</remarks>
public static class IValiadatorExtensions
{
	/// <summary>
	/// Adds the validation services to the specified IServiceCollection.
	/// </summary>
	/// <param name="services">The IServiceCollection to which the validation services will be added.</param>
	extension(IServiceCollection services)
	{
		/// <summary>
		/// Adds CompositeValidator services to the current <see cref="IServiceCollection"/> for dependency injection.
		/// </summary>
		/// <remarks>This method registers the generic <see cref="ICompositeValidator{TArgument}"/> services with a transient lifetime. Call this method during application
		/// startup to enable XValidator-based validation throughout the application.</remarks>
		/// <returns>The <see cref="IServiceCollection"/> instance with Validator services registered. This enables chaining of
		/// further service configuration calls.</returns>
		public IServiceCollection AddXCompositeValidator()
		{
			services.TryAdd(new ServiceDescriptor(typeof(ICompositeValidator<>), typeof(CompositeValidator<>), ServiceLifetime.Transient));
			return services;
		}

		/// <summary>
		/// Registers the specified validator factory type as a scoped service in the dependency injection container.
		/// </summary>
		/// <remarks>If a service of type IValidatorFactor has already been registered, this method does
		/// not overwrite the existing registration.</remarks>
		/// <typeparam name="TValidatorFactory">The type of the validator factory to register. Must implement the IValidatorFactory interface and have a
		/// public constructor.</typeparam>
		/// <returns>The IServiceCollection instance for chaining additional service registrations.</returns>
		public IServiceCollection AddXValidatorFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TValidatorFactory>()
			where TValidatorFactory : class, IValidatorFactory
		{
			services.Replace(new ServiceDescriptor(typeof(IValidatorFactory), typeof(TValidatorFactory), ServiceLifetime.Singleton));
			return services;
		}

		/// <summary>
		/// Replaces the current <see cref="IValidatorFactory"/> registration in the service collection with the
		/// specified factory.
		/// </summary>
		/// <remarks>This method removes any existing <see cref="IValidatorFactory"/> registration and
		/// adds the specified factory as a singleton. Use this method to customize validation behavior by supplying a
		/// custom factory.</remarks>
		/// <param name="factory">The <see cref="IValidatorFactory"/> instance to register. Cannot be null.</param>
		/// <returns>The <see cref="IServiceCollection"/> instance, to allow for method chaining.</returns>
		public IServiceCollection AddXValidatorFactory(IValidatorFactory factory)
		{
			ArgumentNullException.ThrowIfNull(factory);
			services.Replace(new ServiceDescriptor(typeof(IValidatorFactory), factory));
			return services;
		}

		/// <summary>
		/// Adds the default Validator factory to the service collection.
		/// </summary>
		/// <remarks>Call this method during application startup to configure Validator support. This
		/// method registers the default implementation of the validator factory; to use a custom factory, use the
		/// generic overload.</remarks>
		/// <returns>The <see cref="IServiceCollection"/> instance with the XValidator factory registered. This enables
		/// Validator-based validation in the application's dependency injection container.</returns>
		public IServiceCollection AddXValidatorFactory() =>
			services.AddXValidatorFactory<ValidatorFactory>();

		/// <summary>
		/// Registers the specified validator provider type as a scoped service in the dependency injection container.
		/// </summary>
		/// <remarks>If a service of type IValidatorProvider has already been registered, this method does
		/// not overwrite the existing registration.</remarks>
		/// <typeparam name="TValidatorProvider">The type of the validator provider to register. Must implement the IValidatorProvider interface and have a
		/// public constructor.</typeparam>
		/// <returns>The IServiceCollection instance for chaining additional service registrations.</returns>
		public IServiceCollection AddXValidatorProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TValidatorProvider>()
			where TValidatorProvider : class, IValidatorProvider
		{
			services.Replace(new ServiceDescriptor(typeof(IValidatorProvider), typeof(TValidatorProvider), ServiceLifetime.Scoped));
			return services;
		}

		/// <summary>
		/// Replaces the current <see cref="IValidatorProvider"/> registration in the service collection with the
		/// specified provider.
		/// </summary>
		/// <remarks>This method removes any existing <see cref="IValidatorProvider"/> registration and
		/// adds the specified provider as a singleton. Use this method to customize validation behavior by supplying a
		/// custom provider.</remarks>
		/// <param name="provider">The <see cref="IValidatorProvider"/> instance to register. Cannot be null.</param>
		/// <returns>The <see cref="IServiceCollection"/> instance, to allow for method chaining.</returns>
		public IServiceCollection AddXValidatorProvider(IValidatorProvider provider)
		{
			ArgumentNullException.ThrowIfNull(provider);
			services.Replace(new ServiceDescriptor(typeof(IValidatorProvider), provider));
			return services;
		}

		/// <summary>
		/// Adds the default Validator provider to the service collection.
		/// </summary>
		/// <remarks>Call this method during application startup to configure Validator support. This
		/// method registers the default implementation of the validator provider; to use a custom provider, use the
		/// generic overload.</remarks>
		/// <returns>The <see cref="IServiceCollection"/> instance with the XValidator provider registered. This enables
		/// Validator-based validation in the application's dependency injection container.</returns>
		public IServiceCollection AddXValidatorProvider() =>
			services.AddXValidatorProvider<ValidatorProvider>();

		/// <summary>
		/// Registers validators and their resolvers for types found in the specified assemblies, enabling validation support
		/// within the service collection.
		/// </summary>
		/// <remarks>This method dynamically registers validators for types implementing <see cref="IValidator{TArgument}"/> and
		/// IRequiresValidation. It also handles the registration of a default validator and a composite validator for each
		/// argument type. Ensure that the provided assemblies contain the necessary types for validation.</remarks>
		/// <param name="assemblies">An array of assemblies to scan for types that implement validation interfaces. If no assemblies are provided, the
		/// calling assembly is used.</param>
		/// <returns>The updated IServiceCollection instance, allowing for method chaining.</returns>
		[RequiresDynamicCode("Dynamic code generation is required for this method.")]
		[RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
		public IServiceCollection AddXValidators(params IEnumerable<Assembly> assemblies)
		{
			Assembly[] assembliesArray = assemblies as Assembly[] ?? [.. assemblies];
			assembliesArray = assembliesArray is { Length: > 0 } ? assembliesArray : [Assembly.GetCallingAssembly()];

			services.AddXCompositeValidator();
			services.AddXValidatorFactory();
			services.AddXValidatorProvider();
			services.AddTransient(typeof(IValidator<>), typeof(DefaultValidator<>));

			// register IValidatorResolver for each argument that implements IRequiresValidation
			var argumentTypes = assembliesArray
				 .SelectMany(assembly => assembly.GetTypes())
				 .Where(type => typeof(IRequiresValidation).IsAssignableFrom(type))
				 .Select(type => type)
				 .ToList();

			foreach (Type? type in argumentTypes)
			{
				_ = services.AddSingleton(
					typeof(IValidatorResolver),
					typeof(ValidatorResolver<>).MakeGenericType(type));
			}

			// register all sealed types that implement IValidator<TArgument>
			var validatorTypes = assembliesArray
				 .SelectMany(assembly => assembly.GetTypes())
				 .Where(type => type.IsSealed
					 && Array.Exists(type.GetInterfaces(),
						 interfaceType => interfaceType.IsGenericType
							 && interfaceType
								 .GetGenericTypeDefinition() == typeof(IValidator<>)))
				 .Select(type => new
				 {
					 ValidatorType = type,
					 ArgumentType = type.GetInterfaces()
						 .First(interfaceType => interfaceType.IsGenericType
							 && interfaceType
								 .GetGenericTypeDefinition() == typeof(IValidator<>))
						 .GetGenericArguments()[0]
				 })
				 .ToList();

			foreach (var validatorType in validatorTypes)
			{
				if (validatorType.ValidatorType.IsGenericType
					&& validatorType.ValidatorType.GetGenericTypeDefinition() != typeof(DefaultValidator<>))
				{
					_ = services.AddTransient(
						typeof(IValidator<>),
						validatorType.ValidatorType);

					continue;
				}

				_ = services.AddTransient(
					typeof(IValidator<>).MakeGenericType(validatorType.ArgumentType),
					validatorType.ValidatorType);

				if (argumentTypes.All(arg => arg != validatorType.ArgumentType))
				{
					_ = services.AddSingleton(
						typeof(IValidatorResolver),
						typeof(ValidatorResolver<>).MakeGenericType(validatorType.ArgumentType));

					_ = services.AddTransient(
						typeof(ICompositeValidator<>).MakeGenericType(validatorType.ArgumentType),
						typeof(CompositeValidator<>).MakeGenericType(validatorType.ArgumentType));
				}
			}

			return services;
		}
	}
}
