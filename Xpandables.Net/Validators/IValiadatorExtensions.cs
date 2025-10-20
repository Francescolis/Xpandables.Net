﻿/*******************************************************************************
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Validators;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring and registering services with an IServiceCollection.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of IServiceCollection,
/// enabling additional service registration patterns and configuration options commonly used in dependency injection
/// scenarios.</remarks>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IValiadatorExtensions
{
    /// <summary>
    /// Adds the validation services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the validation services will be added.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds Validator services to the current <see cref="IServiceCollection"/> for dependency injection.
        /// </summary>
        /// <remarks>This method registers the generic <see cref="IValidator{T}"/> and <see
        /// cref="ICompositeValidator{T}"/> services with a transient lifetime. Call this method during application
        /// startup to enable XValidator-based validation throughout the application.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with Validator services registered. This enables chaining of
        /// further service configuration calls.</returns>
        public IServiceCollection AddXValidator()
        {
            services.TryAdd(new ServiceDescriptor(typeof(IValidator<>), typeof(Validator<>), ServiceLifetime.Transient));
            services.TryAdd(new ServiceDescriptor(typeof(ICompositeValidator<>), typeof(CompositeValidator<>), ServiceLifetime.Transient));
            return services;
        }

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
        /// Registers all sealed implementations of <see cref="IValidator{TArgument}"/> found in the specified assemblies 
        /// with the dependency injection container.
        /// </summary>
        /// <remarks>This method uses reflection to locate and register all sealed types implementing
        /// <see cref="IValidator{TArgument}"/> in the provided assemblies. It also registers corresponding <see cref="ICompositeValidator{TArgument}"/> types for
        /// each discovered argument type. Dynamic code generation and unreferenced code may be required; see method
        /// attributes for details.</remarks>
        /// <param name="assemblies">An array of assemblies to scan for sealed <see cref="IValidator{TArgument}"/>> implementations. If no assemblies are provided, the
        /// calling assembly is used.</param>
        /// <returns>The IServiceCollection instance with the discovered validators registered.</returns>
        [RequiresDynamicCode("Dynamic code generation is required for this method.")]
        [RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
        public IServiceCollection AddXValidators(params Assembly[] assemblies)
        {
            assemblies = assemblies is { Length: > 0 } ? assemblies : [Assembly.GetCallingAssembly()];

            var validatorTypes = assemblies
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
                if (validatorType.ValidatorType.IsGenericType)
                {
                    _ = services.AddTransient(
                        typeof(IValidator<>),
                        validatorType.ValidatorType);

                    continue;
                }

                _ = services.AddTransient(
                    typeof(IValidator<>).MakeGenericType(validatorType.ArgumentType),
                    validatorType.ValidatorType);

                _ = services.AddTransient(
                    typeof(ICompositeValidator<>).MakeGenericType(validatorType.ArgumentType),
                    typeof(CompositeValidator<>).MakeGenericType(validatorType.ArgumentType));
            }

            return services;
        }
    }
}
