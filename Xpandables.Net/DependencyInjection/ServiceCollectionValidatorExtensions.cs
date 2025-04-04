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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for adding validators to the service collection.
/// </summary>
public static class ServiceCollectionValidatorExtensions
{
    /// <summary>
    /// Adds the default generic validator to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the validators 
    /// to.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the service 
    /// collection is null.</exception>
    public static IServiceCollection AddXValidatorDefault(
        this IServiceCollection services) =>
        services
        .AddTransient(typeof(IValidator<>), typeof(Validator<>))
        .AddTransient(typeof(ICompositeValidator<>), typeof(CompositeValidator<>));

    /// <summary>
    /// Adds the specified validator provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the validator to.</param>
    /// <typeparam name="TValidatorProvider">The type of the validator provider
    /// to add.</typeparam>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXValidatorProvider<TValidatorProvider>(
        this IServiceCollection services)
        where TValidatorProvider : class, IValidatorProvider =>
        services.AddScoped<IValidatorProvider, TValidatorProvider>();

    /// <summary>
    /// Adds the default validator provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the validator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXValidatorProvider(
        this IServiceCollection services) =>
        services.AddXValidatorProvider<ValidatorProvider>();

    /// <summary>
    /// Adds validators from the specified assemblies to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the validators to.</param>
    /// <param name="assemblies">The assemblies to scan for validators.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the service 
    /// collection is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no assemblies are 
    /// provided.</exception>
    public static IServiceCollection AddXValidators(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

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
                typeof(IValidator<>)
                    .MakeGenericType(validatorType.ArgumentType),
                validatorType.ValidatorType);

            _ = services.AddTransient(
                typeof(ICompositeValidator<>)
                    .MakeGenericType(validatorType.ArgumentType),
                typeof(CompositeValidator<>)
                    .MakeGenericType(validatorType.ArgumentType));
        }

        return services;
    }
}
