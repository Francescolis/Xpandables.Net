
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
using Xpandables.Net.Aggregates.Internals;
using Xpandables.Net.Decorators;
using Xpandables.Net.Distribution;
using Xpandables.Net.Distribution.Internals;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides a set of static methods for <see cref="IServiceCollection"/> to 
/// add domain events services.
/// </summary>
public static class ServiceCollectionEventExtensions
{
    internal static readonly MethodInfo AddEventHandlerMethod =
        typeof(ServiceCollectionEventExtensions)
        .GetMethod(nameof(AddXEventHandler))!;

    /// <summary>
    /// Registers the 
    /// <see cref="EventDuplicateHandlerDecorator{TEvent}"/>
    /// decorator to handle duplicate events.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventDuplicateDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .XTryDecorate(
                typeof(IEventHandler<>),
                typeof(EventDuplicateHandlerDecorator<>),
                typeof(IEventDuplicateDecorator));
    }

    /// <summary>
    /// Registers the implementation as <see cref="IEventStore"/> 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TEventStore">The type of that 
    /// implements <see cref="IEventStore"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventStore
        <TEventStore>(this IServiceCollection services)
        where TEventStore : class, IEventStore
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventStore),
                typeof(TEventStore),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default implementation as <see cref="IEventStore"/>
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventStore(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddXEventStore<EventStore>();
    }

    /// <summary>
    /// Registers the implementation as <see cref="IEventRepository"/> 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TEventRepository">The type of that 
    /// implements <see cref="IEventRepository"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventRepository
        <TEventRepository>(this IServiceCollection services)
        where TEventRepository : class, IEventRepository
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventRepository),
                typeof(TEventRepository),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TDomainEventMapper"/> as 
    /// <see cref="IEventDomainMapper"/> type implementation 
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
        where TDomainEventMapper : class, IEventDomainMapper
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventDomainMapper),
                typeof(TDomainEventMapper),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default <see cref="IEventPublisher"/> 
    /// implementation to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventPublisher(
        this IServiceCollection services)
        => services.AddXEventPublisher(typeof(EventPublisherSubscriber));

    /// <summary>
    /// Registers the <paramref name="eventPublisherType"/> as 
    /// <see cref="IEventPublisher"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="eventPublisherType">The domain event publisher type 
    /// implementation.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventPublisher(
        this IServiceCollection services, Type eventPublisherType)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventPublisher),
                eventPublisherType,
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TEventHandler"/> to the services 
    /// with scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event</typeparam>
    /// <typeparam name="TEventHandler">The type of the domain 
    /// event handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that creates the 
    /// domain event handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventHandler<TEvent, TEventHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TEventHandler>?
        implementationHandlerFactory = default)
        where TEventHandler : class, IEventHandler<TEvent>
        where TEvent : notnull, IEvent
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .DoRegisterTypeServiceLifeTime
            <IEventHandler<TEvent>, TEventHandler>(
            implementationHandlerFactory);
    }

    /// <summary>
    /// Registers the <see cref="IEventHandler{TEvent}"/> 
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
    public static IServiceCollection AddXEventHandlers(
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
            typeof(IEventHandler<>),
            AddEventHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the <typeparamref name="TEventSubscriber"/> type as 
    /// <see cref="IEventSubscriber"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TEventSubscriber">The type that implements 
    /// <see cref="IEventSubscriber"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventSubscriber
        <TEventSubscriber>(this IServiceCollection services)
        where TEventSubscriber : class, IEventSubscriber
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IEventSubscriber, TEventSubscriber>();
        return services;
    }

    /// <summary>
    /// Registers the default <see cref="IEventSubscriber"/> implementation 
    /// to the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventSubscriber(
        this IServiceCollection services)
        => services.AddXEventSubscriber<EventPublisherSubscriber>();


}
