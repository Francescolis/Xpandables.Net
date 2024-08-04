
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

using Xpandables.Net.Visitors;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register 
/// <see cref="IVisitor{TElement}"/> implementations to the services.
/// </summary>
public static class ServiceCollectionVisitorExtensions
{
    internal static readonly MethodInfo AddVisitorMethod =
        typeof(ServiceCollectionVisitorExtensions)
        .GetMethod(nameof(AddXVisitor))!;

    /// <summary>
    /// Registers the <typeparamref name="TVisitor"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TElement">The type of the argument to be visited
    /// .</typeparam>
    /// <typeparam name="TVisitor">The type of the visitor.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationFactory">The factory that creates the 
    /// visitor.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXVisitor<TElement, TVisitor>(
        this IServiceCollection services,
        Func<IServiceProvider, TVisitor>? implementationFactory = default)
        where TVisitor : class, IVisitor<TElement>
        where TElement : notnull, IVisitable
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime<IVisitor<TElement>, TVisitor>(
            implementationFactory,
            ServiceLifetime.Transient);

        return services;
    }

    /// <summary>
    /// Adds the <see cref="IVisitor{TElement}"/> implementations to the services 
    /// with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXVisitors(
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
            typeof(IVisitor<>),
            AddVisitorMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the generic <see cref="ICompositeVisitor{TElement}"/>
    /// implementations to the services with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXVisitorComposite(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(
            typeof(ICompositeVisitor<>),
            typeof(CompositeVisitor<>));
        return services;
    }
}
