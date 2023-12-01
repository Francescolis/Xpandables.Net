
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

using Xpandables.Net.IntegrationEvents;
using Xpandables.Net.IntegrationEvents.Messaging;
using Xpandables.Net.Messaging;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register integration event services in the dependency injection container.
/// </summary>
public static class ServiceCollectionIntegrationEventExtensions
{
    internal readonly static MethodInfo AddIntegrationEventHandlerMethod
        = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddXIntegrationEventHandler))!;

    /// <summary>
    /// Adds the <typeparamref name="TIntegrationEventHandler"/> to the services 
    /// with scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TIntegrationEvent">The type of the integration event.</typeparam>
    /// <typeparam name="TIntegrationEventHandler">The type of the integration event handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that creates the integration event handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventHandler<TIntegrationEvent, TIntegrationEventHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TIntegrationEventHandler>? implementationHandlerFactory = default)
        where TIntegrationEventHandler : class, IIntegrationEventHandler<TIntegrationEvent>
        where TIntegrationEvent : notnull, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(services);

        services.DoRegisterTypeServiceLifeTime
            <IIntegrationEventHandler<TIntegrationEvent>, TIntegrationEventHandler>(
            implementationHandlerFactory);

        services.AddScoped<IntegrationEventHandler<TIntegrationEvent>>(
            provider => provider
                .GetRequiredService<IIntegrationEventHandler<TIntegrationEvent>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Adds the <see cref="IIntegrationEventHandler{TIntegrationEvent}"/> 
    /// implementations to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IIntegrationEventHandler<>),
            AddIntegrationEventHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Adds the specified type as <see cref="IIntegrationEventSourcing"/> event sourcing with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventSourcing
        <TIntegrationEventSourcing>(this IServiceCollection services)
        where TIntegrationEventSourcing : class, IIntegrationEventSourcing
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IIntegrationEventSourcing, TIntegrationEventSourcing>();

        return services;
    }

    /// <summary>
    /// Adds the default type as <see cref="IIntegrationEventSourcing"/> event sourcing with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventSourcing(this IServiceCollection services)
        => services.AddXIntegrationEventSourcing<IntegrationEventSourcing>();

    /// <summary>
    /// Adds <see cref="IIntegrationEventOutbox"/> transient integration 
    /// event Outbox behavior to command handlers with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventOutbox(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IIntegrationEventOutbox, IntegrationEventOutbox>();

        return services;
    }

    /// <summary>
    /// Registers the implementation as <see cref="IIntegrationEventStore"/> to the services with scope life time.
    /// </summary>
    /// <typeparam name="TIntegrationEventStore">The type of that implements <see cref="IIntegrationEventStore"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventStore
        <TIntegrationEventStore>(this IServiceCollection services)
        where TIntegrationEventStore : class, IIntegrationEventStore
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IIntegrationEventStore),
                typeof(TIntegrationEventStore),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default <see cref="IIntegrationEventPublisher"/> implementation to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventPublisher(this IServiceCollection services)
        => services.AddXIntegrationEventPublisher<IntegrationEventPublisher>();

    /// <summary>
    /// Registers the <typeparamref name="TIntegrationEventPublisher"/> 
    /// as <see cref="IIntegrationEventPublisher"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TIntegrationEventPublisher">The integration event publisher type implementation.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventPublisher<TIntegrationEventPublisher>(
        this IServiceCollection services)
        where TIntegrationEventPublisher : class, IIntegrationEventPublisher
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IIntegrationEventPublisher),
                typeof(TIntegrationEventPublisher),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TTransientPublisher"/> type as <see cref="ITransientPublisher"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TTransientPublisher">The type that implements <see cref="ITransientPublisher"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientPublisher<TTransientPublisher>(this IServiceCollection services)
        where TTransientPublisher : class, ITransientPublisher
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITransientPublisher, TTransientPublisher>();
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TTransientSubscriber"/> type as <see cref="ITransientSubscriber"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TTransientSubscriber">The type that implements <see cref="ITransientSubscriber"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientSubscriber<TTransientSubscriber>(this IServiceCollection services)
        where TTransientSubscriber : class, ITransientSubscriber
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITransientSubscriber, TTransientSubscriber>();
        return services;
    }

    /// <summary>
    /// Registers the default <see cref="ITransientSubscriber"/> implementation to the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientSubscriber(this IServiceCollection services)
        => services.AddXTransientPublisher<TransientPublisherSubscriber>();

}
