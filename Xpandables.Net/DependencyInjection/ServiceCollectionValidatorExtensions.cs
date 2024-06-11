
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Validators;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register 
/// <see cref="IValidator{TArgument}"/> implementations to the services.
/// </summary>
public static class ServiceCollectionValidatorExtensions
{
    internal readonly static MethodInfo AddValidatorMethod =
        typeof(ServiceCollectionValidatorExtensions)
        .GetMethod(nameof(AddXValidator))!;

    /// <summary>
    /// Registers the generic <see cref="IValidator{TArgument}"/> 
    /// and <see cref="ICompositeValidator{TArgument}"/>
    /// implementations to the services with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXValidatorGenerics(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(
            typeof(IValidator<>),
            typeof(Validator<>));
        services.TryAddTransient(
            typeof(ICompositeValidator<>),
            typeof(CompositeValidator<>));
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TValidator"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TArgument">The type of the argument to be validated
    /// .</typeparam>
    /// <typeparam name="TValidator">The type of the validator.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationFactory">The factory that creates the 
    /// validator.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXValidator<TArgument, TValidator>(
        this IServiceCollection services,
        Func<IServiceProvider, TValidator>? implementationFactory = default)
        where TValidator : class, IValidator<TArgument>
        where TArgument : notnull
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime
            <IValidator<TArgument>, TValidator>(
            implementationFactory,
            ServiceLifetime.Transient);

        return services;
    }

    /// <summary>
    /// Registers all <see cref="IValidator{TArgument}"/> implementations found in 
    /// the assemblies to the services with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXValidators(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IValidator<>),
            AddValidatorMethod,
            assemblies);
    }
}
