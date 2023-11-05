
/************************************************************************************************************
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
************************************************************************************************************/
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Operations.Specifications;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides with methods to register services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    internal readonly static MethodInfo AddSpecificationMethod = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddXSpecification))!;

    /// <summary>
    /// Registers the generic <see cref="ICompositeSpecification{TSource}"/>
    /// implementations to the services with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXSpecificationComposite(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(typeof(ICompositeSpecification<>), typeof(CompositeSpecification<>));
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TSpecification"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TArgument">The type of the argument to be checked.</typeparam>
    /// <typeparam name="TSpecification">The type of the specification.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationFactory">The factory that creates the visitor.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXSpecification<TArgument, TSpecification>(
        this IServiceCollection services,
        Func<IServiceProvider, TSpecification>? implementationFactory = default)
        where TSpecification : class, ISpecification<TArgument>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.DoRegisterTypeServiceLifeTime<ISpecification<TArgument>, TSpecification>(
            implementationFactory,
            ServiceLifetime.Transient);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="ISpecification{TSource}"/> to the services with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXSpecifications(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(ISpecification<>),
            AddSpecificationMethod,
            assemblies);
    }
}
