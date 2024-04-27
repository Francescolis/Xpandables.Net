
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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.Decorators;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.SnapShots;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides a set of static methods for <see cref="IServiceCollection"/> to 
/// add aggregates services.
/// </summary>
public static class ServiceCollectionAggregateExtensions
{
    internal readonly static MethodInfo AddDomainEventHandlerMethod =
        typeof(ServiceCollectionAggregateExtensions)
        .GetMethod(nameof(AddXDomainEventHandler))!;

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
    /// Registers the default transactional aggregagte to the 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> 
    /// type implementation, that adds transaction behavior to aggregate store. 
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXAggregateStoreTransactional(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddXAggregateStoreTransactional(
                typeof(AggregateStoreTransactional<,>));
    }

    /// <summary>
    /// Registers the specified transactional aggregate to the 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> 
    /// type implementation, that adds transaction behavior to aggregate store. 
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="transactionalType">The transactional type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXAggregateStoreTransactional(
        this IServiceCollection services,
        Type transactionalType)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddScoped(
                typeof(IAggregateStoreTransactional<,>),
                transactionalType);
    }

    /// <summary>
    /// Registers the default snapShot to the 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> 
    /// type implementation, that adds snapShot behavior to aggregate store. 
    /// You may need to define the <see cref="SnapShotOptions"/> 
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
    /// You may need to define the <see cref="SnapShotOptions"/> 
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


    /// <summary>
    /// Registers the implementation 
    /// <typeparamref name="TAggregateTransactional"/> for
    /// <see cref="IAggregateTransactional"/> type 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TAggregateTransactional">The type of that
    /// implements <see cref="IAggregateTransactional"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXAggregateTransactional
        <TAggregateTransactional>(
        this IServiceCollection services)
        where TAggregateTransactional : class, IAggregateTransactional
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped<IAggregateTransactional, TAggregateTransactional>();
    }

    /// <summary>
    /// Registers the implementation as <see cref="IDomainEventStore"/> 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TDomainEventStore">The type of that 
    /// implements <see cref="IDomainEventStore"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXDomainEventStore
        <TDomainEventStore>(this IServiceCollection services)
        where TDomainEventStore : class, IDomainEventStore
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IDomainEventStore),
                typeof(TDomainEventStore),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TDomainEventMapper"/> as 
    /// <see cref="IDomainEventMapper{TAggregateId}"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TAggregateId">the type of aggregate Id.</typeparam>
    /// <typeparam name="TDomainEventMapper">The domain event mapper type 
    /// implementation.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXDomainEventMapper
        <TAggregateId, TDomainEventMapper>(this IServiceCollection services)
        where TDomainEventMapper : class, IDomainEventMapper<TAggregateId>
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IDomainEventMapper<TAggregateId>),
                typeof(TDomainEventMapper),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default <see cref="IDomainEventPublisher{TAggregateId}"/> 
    /// implementation to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXDomainEventPublisher(
        this IServiceCollection services)
        => services.AddXDomainEventPublisher(typeof(DomainEventPublisher<>));

    /// <summary>
    /// Registers the <paramref name="domainEventPublisherType"/> as 
    /// <see cref="IDomainEventPublisher{TAggregateId}"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="domainEventPublisherType">The domain event publisher type 
    /// implementation.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXDomainEventPublisher(
        this IServiceCollection services, Type domainEventPublisherType)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IDomainEventPublisher<>),
                domainEventPublisherType,
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TDomainEventHandler"/> to the services 
    /// with scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TDomainEvent">The type of the domain event</typeparam>
    /// <typeparam name="TAggregateId">the type of aggregate Id.</typeparam>
    /// <typeparam name="TDomainEventHandler">The type of the domain 
    /// event handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that creates the 
    /// domain event handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXDomainEventHandler
        <TDomainEvent, TAggregateId, TDomainEventHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TDomainEventHandler>?
        implementationHandlerFactory = default)
        where TDomainEventHandler : class,
            IDomainEventHandler<TDomainEvent, TAggregateId>
        where TDomainEvent : notnull, IDomainEvent<TAggregateId>
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services
            .DoRegisterTypeServiceLifeTime
            <IDomainEventHandler<TDomainEvent, TAggregateId>, TDomainEventHandler>(
            implementationHandlerFactory);

        return services
            .AddScoped<DomainEventHandler<TDomainEvent, TAggregateId>>(
            provider => provider
                .GetRequiredService
                <IDomainEventHandler<TDomainEvent, TAggregateId>>()
                .HandleAsync);
    }

    /// <summary>
    /// Registers the <see cref="IDomainEventHandler{TDomainEvent, TAggregateId}"/> 
    /// implementations to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXDomainEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IDomainEventHandler<,>),
            AddDomainEventHandlerMethod,
            assemblies);
    }
}
