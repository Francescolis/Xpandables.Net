
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides a set of static methods for <see cref="IServiceCollection"/> to 
/// add aggregates services.
/// </summary>
public static class ServiceCollectionAggregateExtensions
{
    /// <summary>
    /// Configures the <see cref="EventOptions"/> with the default values.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXEventOptions(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .Configure<EventOptions>(EventOptions.Default);
    }

    /// <summary>
    /// Configures the <see cref="EventOptions"/> with the specified values.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureOptions">The configuration options.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventOptions(
        this IServiceCollection services,
        Action<EventOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        return services
            .Configure(configureOptions);
    }

    /// <summary>
    /// Registers the <typeparamref name="TUnitOfWork"/> type class reference
    /// as <see cref="IUnitOfWork"/> for aggregate.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXUnitOfWorkAggregate<TUnitOfWork>(
        this IServiceCollection services)
        where TUnitOfWork : class, IUnitOfWork
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddXUnitOfWorkKeyed<TUnitOfWork>(EventOptions.UnitOfWorkKey);
    }

    /// <summary>
    /// Registers the specified generic 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> type 
    /// implementations 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="aggregateStoreType">The generic aggregate store.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXAggregateStore(
        this IServiceCollection services,
        Type aggregateStoreType)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IAggregateStore<,>),
                aggregateStoreType,
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default generic 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> type 
    /// implementations 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXAggregateStore(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddXAggregateStore(typeof(AggregateStore<,>));
    }

    /// <summary>
    /// Registers the default snapShot to the 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> 
    /// type implementation, that adds snapShot behavior to aggregate store. 
    /// You may need to define the <see cref="SnapshotOptions"/> 
    /// in the configuration file.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXAggregateStoreSnapshot(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddXAggregateStoreSnapshot(
                typeof(AggregateStoreSnapshot<,>));
    }

    /// <summary>
    /// Registers the specified snapShot to the 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> 
    /// type implementation, that adds snapShot behavior to aggregate store. 
    /// You may need to define the <see cref="SnapshotOptions"/> 
    /// in the configuration file.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="snapshotType">The snapshot type.
    /// Must be generic.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXAggregateStoreSnapshot(
        this IServiceCollection services, Type snapshotType)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped(typeof(IAggregateStoreSnapshot<,>), snapshotType);
    }
}
