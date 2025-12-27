/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using System.Events;
using System.Events.Aggregates;
using System.Events.Domain;
using System.Events.Integration;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering event store and outbox store services with an <see
/// cref="IServiceCollection"/> in applications using the XEvent and Outbox patterns.
/// </summary>
/// <remarks>These extension methods simplify the configuration of event sourcing and outbox services by
/// registering default or custom implementations with the dependency injection container. Use these methods to add
/// support for event storage and outbox processing in your application's service pipeline.</remarks>
public static class IEventExtensions
{
    internal static readonly MethodInfo AddEventHandlerMethod =
        typeof(IEventExtensions).GetMethod(nameof(AddXEventHandler))!;

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds services required for event context propagation and enrichment to the dependency injection container.
        /// <code>
        /// If you want to set correlation/causation for a scope
        /// 
        ///     var accessor = app.Services.GetRequiredService&lt;IEventContextAccessor&gt;();
        ///
        ///     using var _ = accessor.BeginScope(new EventContext
        ///     {
        ///         CorrelationId = Guid.CreateVersion7(),
        ///         CausationId = Guid.CreateVersion7()
        ///     });
        ///
        ///     // any aggregates saved in this logical call-path get enriched automatically
        /// </code>
        /// </summary>
        /// <remarks>This method registers implementations for event context access and you need to register the enrichment,
        /// enabling event handlers to access contextual information during event processing. Call this method during
        /// application startup to ensure event context features are available throughout the application's
        /// lifetime.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with event context services registered.</returns>
        public IServiceCollection AddXEventContextAccessor()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<AsyncLocalEventContextAccessor>();
            services.TryAddSingleton<IEventContextAccessor>(sp => sp.GetRequiredService<AsyncLocalEventContextAccessor>());

            return services;
        }

        /// <summary>
        /// Adds a scoped domain event enricher of the specified type to the service collection.
        /// </summary>
        /// <remarks>If an IDomainEventEnricher service has not already been registered, this method
        /// registers the specified type as a scoped service. This enables dependency injection of the domain event
        /// enricher throughout the application.</remarks>
        /// <typeparam name="TDomainEventEnricher">The type of the domain event enricher to register. Must implement the IDomainEventEnricher interface and
        /// have a public constructor.</typeparam>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddXDomainEventEnricher<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDomainEventEnricher>()
            where TDomainEventEnricher : class, IDomainEventEnricher
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IDomainEventEnricher, TDomainEventEnricher>();
            return services;
        }

        /// <summary>
        /// Adds the specified integration event enricher type to the service collection for dependency injection.
        /// </summary>
        /// <remarks>This method registers the specified integration event enricher as a scoped service.
        /// Subsequent calls to resolve IIntegrationEventEnricher will return an instance of the specified type. Use
        /// this method to enable custom enrichment of integration events within the application's dependency injection
        /// pipeline.</remarks>
        /// <typeparam name="TIntegrationEventEnricher">The type of the integration event enricher to register. Must implement the IIntegrationEventEnricher
        /// interface and have a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance with the integration event enricher registered.</returns>
        public IServiceCollection AddXIntegrationEventEnricher<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TIntegrationEventEnricher>()
            where TIntegrationEventEnricher : class, IIntegrationEventEnricher
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IIntegrationEventEnricher, TIntegrationEventEnricher>();
            return services;
        }

        /// <summary>
        /// Adds the default integration event enricher to the service collection for use with X integration events.
        /// </summary>
        /// <remarks>This method registers <see cref="DefaultIntegrationEventEnricher"/> as the
        /// implementation for integration event enrichment. Call this method during application startup to enable
        /// enrichment of X integration events. This method is intended to be used as part of the dependency injection
        /// setup.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the default integration event enricher registered.</returns>
        public IServiceCollection AddXIntegrationEventEnricher()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXIntegrationEventEnricher<DefaultIntegrationEventEnricher>();
        }

        /// <summary>
        /// Adds the default domain event enricher to the service collection for use with cross-domain event processing.
        /// </summary>
        /// <remarks>This method registers <see cref="DefaultDomainEventEnricher"/> as the implementation
        /// for domain event enrichment. Call this method during application startup to enable domain event enrichment
        /// features.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the default domain event enricher registered.</returns>
        public IServiceCollection AddXDomainEventEnricher()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXDomainEventEnricher<DefaultDomainEventEnricher>();
        }

        /// <summary>
        /// Registers an <see cref="AggregateStore{TAggregate}"/> for the specified aggregate type in the dependency injection container.
        /// </summary>
        /// <remarks>Use this method to enable dependency injection of IAggregateStore for the specified
        /// aggregate type. The registration is added with scoped lifetime.</remarks>
        /// <typeparam name="TAggregate">The aggregate type to register. Must implement both IAggregate and <see cref="IAggregateFactory{TAggregate}"/> .</typeparam>
        /// <returns>The IServiceCollection instance with the aggregate store registration added.</returns>
        public IServiceCollection AddXAggregateStore<TAggregate>()
            where TAggregate : class, IAggregate, IAggregateFactory<TAggregate>
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IAggregateStore<TAggregate>, AggregateStore<TAggregate>>();
            return services;
        }

        /// <summary>
        /// Registers the specified aggregate store implementation as a scoped service for <see cref="IAggregateStore{TAggregate}"/> in the
        /// dependency injection container.
        /// </summary>
        /// <remarks>Use this method to configure a custom aggregate store for dependency injection. The
        /// registration is scoped, meaning a new instance is created per request or scope.</remarks>
        /// <param name="aggregateStoreType">The type that implements the aggregate store. Must have a public constructor and implement
        /// IAggregateStore for the relevant aggregate types.</param>
        /// <returns>The IServiceCollection instance with the aggregate store registration added. This enables further chaining
        /// of service registrations.</returns>
        public IServiceCollection AddXAggregateStore([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type aggregateStoreType)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped(typeof(IAggregateStore<>), aggregateStoreType);
            return services;
        }

        /// <summary>
        /// Adds the default <see cref="AggregateStore{TAggregate}"/> implementation to the service collection for dependency injection.
        /// </summary>
        /// <remarks>This method registers the generic AggregateStore implementation as the service for
        /// XAggregateStore. Call this method during application startup to enable aggregate store functionality in your
        /// application.</remarks>
        /// <returns>The updated IServiceCollection instance with the AggregateStore service registered.</returns>
        public IServiceCollection AddXAggregateStore() =>
            services.AddXAggregateStore(typeof(AggregateStore<>));

        /// <summary>
        /// Registers the specified subscriber type as an implementation of <see cref="IEventSubscriber"/> with scoped
        /// lifetime in the service collection.
        /// </summary>
        /// <param name="lifetime">The service lifetime for the publisher registration. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
        /// <remarks>If an <see cref="IEventSubscriber"/> service is already registered, this method does not
        /// overwrite the existing registration. Use this method to enable dependency injection of custom subscriber
        /// implementations.</remarks>
        /// <typeparam name="TEventSubscriber">The subscriber type to register. Must be a non-abstract class that implements <see cref="IEventSubscriber"/> and
        /// has a public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the subscriber registration added.</returns>
        public IServiceCollection AddXEventSubscriber<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventSubscriber>(
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TEventSubscriber : class, IEventSubscriber
        {
            services.TryAdd(
                new ServiceDescriptor(
                    typeof(IEventSubscriber),
                    typeof(TEventSubscriber),
                    lifetime));

            return services;
        }

        /// <summary>
        /// Adds the default XSubscriber implementation to the service collection for dependency injection.
        /// </summary>
        /// <param name="lifetime">The service lifetime for the publisher registration. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the XSubscriber service registered.</returns>
        public IServiceCollection AddXEventSubscriber(ServiceLifetime lifetime = ServiceLifetime.Singleton) =>
            services.AddXEventSubscriber<EventPublisherSubscriber>(lifetime);

        /// <summary>
        /// Registers the specified scheduler implementation as a singleton service for dependency injection.
        /// </summary>
        /// <remarks>This method adds the <typeparamref name="TScheduler"/> implementation as the
        /// singleton service for <see cref="IScheduler"/>. If a scheduler service is already registered, it will not be
        /// replaced.</remarks>
        /// <typeparam name="TScheduler">The type of scheduler to register. Must implement <see cref="IScheduler"/> and have a public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the scheduler service registered.</returns>
        public IServiceCollection AddXScheduler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TScheduler>()
            where TScheduler : class, IScheduler
        {
            services.TryAdd(
                new ServiceDescriptor(
                    typeof(IScheduler),
                    typeof(TScheduler),
                    ServiceLifetime.Singleton));

            return services;
        }

        /// <summary>
        /// Adds the default Scheduler implementation to the service collection for dependency injection.
        /// </summary>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with Scheduler services registered.</returns>
        public IServiceCollection AddXScheduler() =>
            services.AddXScheduler<Scheduler>();

        /// <summary>
        /// Adds an implementation of IHostedScheduler as a hosted service to the dependency injection container.
        /// </summary>
        /// <remarks>This method registers the specified scheduler type and ensures it is started as a
        /// hosted background service. Use this method to enable automatic lifecycle management of the scheduler within
        /// ASP.NET Core applications.</remarks>
        /// <typeparam name="THostedScheduler">The type of scheduler to register. Must be a class that implements IHostedScheduler and has a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance with the scheduler and hosted service registered.</returns>
        public IServiceCollection AddXHostedScheduler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THostedScheduler>()
            where THostedScheduler : class, IHostedScheduler =>
            services
                .AddSingleton<IHostedScheduler, THostedScheduler>()
                .AddHostedService(provider =>
                    provider.GetRequiredService<IHostedScheduler>());

        /// <summary>
        /// Adds the default hosted Scheduler service to the dependency injection container.
        /// </summary>
        /// <remarks>This method registers the standard <see cref="Scheduler"/> implementation as a hosted
        /// service. Call this method during application startup to enable background scheduling
        /// functionality.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the Scheduler hosted service registered.</returns>
        public IServiceCollection AddXHostedScheduler() =>
            services.AddXHostedScheduler<HostedScheduler>();

        /// <summary>
        /// Registers the specified event bus implementation and related services with the dependency injection
        /// container.
        /// </summary>
        /// <remarks>This method adds the specified event bus as a singleton service for IEventBus and
        /// registers EventBusPublisher as a scoped service for IEventPublisher. Call this method during application
        /// startup to enable event bus functionality.</remarks>
        /// <typeparam name="TEventBus">The type of the event bus to register. Must implement the IEventBus interface and have a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance for chaining additional service registrations.</returns>
        public IServiceCollection AddXEventBus<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventBus>()
             where TEventBus : class, IEventBus
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<IEventBus, TEventBus>();
            services.TryAddScoped<IEventPublisher, EventBusPublisher>();

            return services;
        }

        /// <summary>
        /// Registers a composite <see cref="IEventPublisher"/> that invokes all currently registered publishers.
        /// <code>
        /// In-process subscribers + external bus
        ///     services.AddXEventPublisher();     // in-process handler dispatch (EventPublisherSubscriber)
        ///     services.AddXEventBus&lt;MyBus&gt;();    // external bus publisher (EventBusPublisher)
        ///     services.AddXCompositeEventPublisher();
        /// </code>
        /// </summary>
        /// <remarks>
        /// This is intended for scenarios where the same event must be published to multiple targets
        /// (e.g. in-process subscribers + external bus).
        /// </remarks>
        public IServiceCollection AddXCompositeEventPublisher()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddEnumerable(ServiceDescriptor.Scoped<IEventPublisher, EventPublisherSubscriber>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IEventPublisher, EventBusPublisher>());

            services.Replace(ServiceDescriptor.Scoped<IEventPublisher>(sp =>
            {
                var publishers = sp.GetServices<IEventPublisher>()
                    .Where(static p => p is not CompositeEventPublisher)
                    .ToArray();

                return new CompositeEventPublisher(publishers);
            }));

            return services;
        }

        /// <summary>
        /// Registers the specified publisher type as the scoped implementation of <see cref="IEventPublisher"/> in the
        /// dependency injection container.
        /// </summary>
        /// <remarks>If an <see cref="IEventPublisher"/> service is already registered, this method does not
        /// overwrite the existing registration. Use this method to enable dependency injection of a custom publisher
        /// implementation within the application's scope.</remarks>
        /// <typeparam name="TEventPublisher">The publisher type to register. Must be a class that implements <see cref="IEventPublisher"/> and have a public
        /// constructor.</typeparam>
        /// <param name="lifetime">The service lifetime for the publisher registration. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance with the publisher registration added.</returns>
        public IServiceCollection AddXEventPublisher<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventPublisher>(
                ServiceLifetime lifetime = ServiceLifetime.Scoped)
                where TEventPublisher : class, IEventPublisher
        {
            services.TryAdd(
                new ServiceDescriptor(
                    typeof(IEventPublisher),
                    typeof(TEventPublisher),
                    lifetime));

            return services;
        }

        /// <summary>
        /// Adds the default Publisher implementation to the service collection for dependency injection.
        /// It also registers the same instance as an IEventSubscriber, along with the necessary event handler registries.
        /// <code language="csharp"> Sample usage:
        /// 
        ///     var dynamicRegistry = app.Services.GetRequiredService&lt;DynamicEventHandlerRegistry&gt;();
        ///     dynamicRegistry.Register(new[] { new PluginEventHandler() }); // runtime registration
        ///     
        ///     // Later:
        ///     dynamicRegistry.Unregister&lt;PluginEvent&gt;();
        /// </code>
        /// </summary>
        /// <param name="registryMode">The event registry mode to use. Defaults to <see cref="EventRegistryMode.Default"/>.</param>
        /// <param name="lifetime">The service lifetime for the publisher registration. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
        /// <remarks>This method registers the PublisherSubscriber type as the implementation for
        /// IEventPublisher. Call this method during application startup to enable Publisher features in your
        /// application.</remarks>
        /// <returns>The updated IServiceCollection instance with the Publisher services registered.</returns>
        public IServiceCollection AddXEventPublisher(
            EventRegistryMode registryMode = EventRegistryMode.Default,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            services
                .AddXEventPublisher<EventPublisherSubscriber>(lifetime)
                .Add(new ServiceDescriptor(
                    typeof(IEventSubscriber),
                    sp => (EventPublisherSubscriber)sp.GetRequiredService<IEventPublisher>(),
                    lifetime));

            _ = registryMode switch
            {
                EventRegistryMode.Default => services.AddScoped<IEventHandlerRegistry, EventHandlerRegistry>(),
                EventRegistryMode.Static => services.AddSingleton<IEventHandlerRegistry, StaticEventHandlerRegistry>(),
                EventRegistryMode.Dynamic => services.AddSingleton<IEventHandlerRegistry, DynamicEventHandlerRegistry>(),
                _ => services
                    .AddSingleton<StaticEventHandlerRegistry>()
                    .AddSingleton<DynamicEventHandlerRegistry>()
                    .AddSingleton<IEventHandlerRegistry, CompositeEventHandlerRegistry>(),
            };

            return services;
        }

        /// <summary>
        /// Registers a specific event handler for the specified event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to handle.</typeparam>
        /// <typeparam name="TEventHandler">The type of event handler implementation.</typeparam>
        /// <param name="factory">An optional factory function to create the event handler instance. If null, the handler is registered using its type.</param>
        /// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
        /// <remarks>
        /// The event handler is registered with a scoped lifetime. Use this method to explicitly register
        /// a handler for a specific event type, or use <see cref="AddXEventHandlers"/> to automatically
        /// discover and register all event handlers from assemblies.
        /// </remarks>
        public IServiceCollection AddXEventHandler<TEvent, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventHandler>(Func<IServiceProvider, TEventHandler>? factory)
            where TEvent : class, IEvent
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

            return services.AddXEventHandlerWrapper<TEvent>();
        }

        /// <summary>
        /// Registers an event handler wrapper for the specified event type in the dependency injection container.
        /// </summary>
        /// <remarks>This method adds a singleton registration for <see cref="IEventHandlerWrapper"/>
        /// using <see cref="EventHandlerWrapper{TEvent}"/>. Call this method to enable handling of events of type
        /// <typeparamref name="TEvent"/> via the wrapper mechanism. This method is automatically called when registering event handlers
        /// using <see cref="AddXEventHandler{TEvent,TEventHandler}"/>.</remarks>
        /// <typeparam name="TEvent">The event type for which the handler wrapper is registered. Must implement <see cref="IEvent"/> and be a
        /// reference type.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the event handler wrapper registration added.</returns>
        public IServiceCollection AddXEventHandlerWrapper<TEvent>()
            where TEvent : class, IEvent =>
            services.AddScoped<IEventHandlerWrapper, EventHandlerWrapper<TEvent>>();

        /// <summary>
        /// Registers all sealed event handler classes implementing the <see cref="IEventHandler{TEvent}"/> interface from the
        /// specified assemblies into the service collection.
        /// </summary>
        /// <remarks>Only sealed, non-abstract classes implementing <see cref="IEventHandler{TEvent}"/> are registered.
        /// This method requires dynamic code and may not be compatible with trimming or ahead-of-time compilation
        /// scenarios.</remarks>
        /// <param name="assemblies">An array of assemblies to scan for event handler types. If no assemblies are provided, the calling assembly
        /// is used by default.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated IServiceCollection
        /// with the discovered event handlers registered.</returns>
        [RequiresUnreferencedCode("The event handlers may not be fully referenced.")]
        [RequiresDynamicCode("The event handlers may not be fully referenced.")]
        public IServiceCollection AddXEventHandlers(params Assembly[] assemblies)
        {
            assemblies = assemblies is { Length: > 0 } ? assemblies : [Assembly.GetCallingAssembly()];

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

    internal readonly record struct HandlerType(
        Type Type,
        IEnumerable<Type> Interfaces);
}