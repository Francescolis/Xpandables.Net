
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

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides a set of static methods for <see cref="IServiceCollection"/> to 
/// add domain events services.
/// </summary>
public static class ServiceCollectionDomainExtensions
{
    internal readonly static MethodInfo AddDomainEventHandlerMethod =
        typeof(ServiceCollectionDomainExtensions)
        .GetMethod(nameof(AddXEventDomainHandler))!;

    /// <summary>
    /// Registers the 
    /// <see cref="EventDomainDuplicateDecorator{TEventDomain, TAggragateId}"/>
    /// decorator to handle duplicate domain events.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventDomainDuplicateDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .XTryDecorate(
                typeof(IEventDomainHandler<,>),
                typeof(EventDomainDuplicateDecorator<,>),
                typeof(IEventDomainDuplicate));
    }

    /// <summary>
    /// Registers the implementation as <see cref="IEventDomainStore"/> 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TDomainEventStore">The type of that 
    /// implements <see cref="IEventDomainStore"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventDomainStore
        <TDomainEventStore>(this IServiceCollection services)
        where TDomainEventStore : class, IEventDomainStore
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventDomainStore),
                typeof(TDomainEventStore),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default implementation as <see cref="IEventDomainStore"/>
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventDomainStore(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddXEventDomainStore
            <EventDomainStore<EventEntityDomain>>();
    }

    /// <summary>
    /// Registers the <typeparamref name="TDomainEventMapper"/> as 
    /// <see cref="IEventDomainMapper{TAggregateId}"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TAggregateId">the type of aggregate Id.</typeparam>
    /// <typeparam name="TDomainEventMapper">The domain event mapper type 
    /// implementation.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventDomainMapper
        <TAggregateId, TDomainEventMapper>(this IServiceCollection services)
        where TDomainEventMapper : class, IEventDomainMapper<TAggregateId>
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventDomainMapper<TAggregateId>),
                typeof(TDomainEventMapper),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default <see cref="IEventDomainPublisher{TAggregateId}"/> 
    /// implementation to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventDomainPublisher(
        this IServiceCollection services)
        => services.AddXEventDomainPublisher(typeof(EventDomainPublisher<>));

    /// <summary>
    /// Registers the <paramref name="domainEventPublisherType"/> as 
    /// <see cref="IEventDomainPublisher{TAggregateId}"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="domainEventPublisherType">The domain event publisher type 
    /// implementation.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventDomainPublisher(
        this IServiceCollection services, Type domainEventPublisherType)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventDomainPublisher<>),
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
    public static IServiceCollection AddXEventDomainHandler
        <TDomainEvent, TAggregateId, TDomainEventHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TDomainEventHandler>?
        implementationHandlerFactory = default)
        where TDomainEventHandler : class,
            IEventDomainHandler<TDomainEvent, TAggregateId>
        where TDomainEvent : notnull, IEventDomain<TAggregateId>
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .DoRegisterTypeServiceLifeTime
            <IEventDomainHandler<TDomainEvent, TAggregateId>, TDomainEventHandler>(
            implementationHandlerFactory);
    }

    /// <summary>
    /// Registers the <see cref="IEventDomainHandler{TDomainEvent, TAggregateId}"/> 
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
    public static IServiceCollection AddXEventDomainHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IEventDomainHandler<,>),
            AddDomainEventHandlerMethod,
            assemblies);
    }
}
