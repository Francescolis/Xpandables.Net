
/*******************************************************************************
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.States;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding event store services to the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionEventExtensions
{
    /// <summary>
    /// Adds the aggregate snapshot store to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the aggregate 
    /// snapshot store to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXAggregateSnapshotStore(
        this IServiceCollection services) =>
        services.XTryDecorate(
            typeof(IAggregateStore<>),
            typeof(SnapshotStore<>),
            typeof(IOriginator));

    /// <summary>
    /// Adds the specified aggregate store implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// aggregate store to.</param>
    /// <param name="aggregateStoreType">The type of the aggregate store 
    /// implementation.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXAggregateStore(
        this IServiceCollection services,
        Type aggregateStoreType)
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(IAggregateStore<>),
                aggregateStoreType,
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Adds the default aggregate store implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// aggregate store to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXAggregateStore(
        this IServiceCollection services) =>
        services.AddXAggregateStore(typeof(AggregateStore<>));

    /// <summary>
    /// Adds the specified event store implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEventStore">The type of the event store 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// event store to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventStore<TEventStore>(
        this IServiceCollection services)
        where TEventStore : class, IEventStore
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventStore),
                typeof(TEventStore),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Adds the specified event publisher implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEventPublisher">The type of the event publisher 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// event publisher to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventPublisher<TEventPublisher>(
        this IServiceCollection services)
        where TEventPublisher : class, IPublisher
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(IPublisher),
                typeof(TEventPublisher),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Adds the default event publisher implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// event publisher to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventPublisher(
        this IServiceCollection services) =>
        services.AddXEventPublisher<PublisherSubscriber>();

    /// <summary>
    /// Adds the specified event subscriber implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEventSubscriber">The type of the event subscriber 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// event subscriber to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventSubscriber<TEventSubscriber>(
        this IServiceCollection services)
        where TEventSubscriber : class, ISubscriber
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(ISubscriber),
                typeof(TEventSubscriber),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Adds the default event subscriber implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// event subscriber to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventSubscriber(
        this IServiceCollection services) =>
        services.AddXEventSubscriber<PublisherSubscriber>();

    /// <summary>
    /// Adds the specified event scheduler implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEventScheduler">The type of the event scheduler 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// event scheduler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventScheduler<TEventScheduler>(
        this IServiceCollection services)
        where TEventScheduler : class, IScheduler
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(IScheduler),
                typeof(TEventScheduler),
                ServiceLifetime.Singleton));

        return services;
    }

    /// <summary>
    /// Adds the default event scheduler implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// event scheduler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventScheduler(
        this IServiceCollection services) =>
        services.AddXEventScheduler<Scheduler>();

    /// <summary>
    /// Adds the specified event scheduler implementation that also implements
    /// <see cref="IHostedService"/> to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEventScheduler">The type of the event scheduler 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the event 
    /// scheduler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventSchedulerHosted<TEventScheduler>(
        this IServiceCollection services)
        where TEventScheduler : class, IScheduler, IHostedService =>
        services
            .AddXEventScheduler<TEventScheduler>()
            .AddHostedService(provider =>
                provider.GetRequiredService<IScheduler>());

    /// <summary>
    /// Adds the default event scheduler implementation that also implements
    /// <see cref="IHostedService"/> to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the event 
    /// scheduler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventSchedulerHosted(
        this IServiceCollection services) =>
        services.AddXEventSchedulerHosted<Scheduler>();

    /// <summary>
    /// Adds the specified event handler implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <typeparam name="TEventHandler">The type of the event handler 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// event handler to.</param>
    /// <param name="factory">The factory method to create the event handler 
    /// instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventHandler<TEvent, TEventHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TEventHandler>? factory)
        where TEvent : notnull, IEvent
        where TEventHandler : class, IEventHandler<TEvent>
    {
        if (factory is not null)
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(IEventHandler<TEvent>),
                    factory,
                    ServiceLifetime.Scoped));
        }
        else
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(IEventHandler<TEvent>),
                    typeof(TEventHandler),
                    ServiceLifetime.Scoped));
        }

        return services;
    }

    internal static readonly MethodInfo AddEventHandlerMethod =
        typeof(ServiceCollectionEventExtensions)
        .GetMethod(nameof(AddXEventHandler))!;

    /// <summary>
    /// Adds event handlers from the specified assemblies 
    /// to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the event 
    /// handlers to.</param>
    /// <param name="assemblies">The assemblies to scan for event handlers.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var eventHandlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type is { IsClass: true, IsAbstract: false, IsSealed: true }
                && type.GetInterfaces().Any(@interface =>
                    @interface.IsGenericType
                    && @interface.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .Select(type => new
            {
                InterfaceTypes = type.GetInterfaces()
                    .Where(@interface =>
                        @interface.IsGenericType
                        && @interface.GetGenericTypeDefinition() == typeof(IEventHandler<>)),
                EventHandlerType = type
            });

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            foreach (Type interfaceType in eventHandlerType.InterfaceTypes)
            {
                MethodInfo addEventHandlerMethod = AddEventHandlerMethod
                    .MakeGenericMethod(
                        interfaceType.GetGenericArguments()[0],
                        eventHandlerType.EventHandlerType);

                _ = addEventHandlerMethod.Invoke(null, [services, null]);
            }
        }

        return services;
    }
}
