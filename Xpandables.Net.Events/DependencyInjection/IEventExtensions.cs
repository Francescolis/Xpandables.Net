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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Events;
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.States;
using Xpandables.Net.Tasks.Pipelines;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
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
        /// Adds the default <see cref="AggregateStore"/> implementation to the service collection for dependency injection.
        /// </summary>
        /// <remarks>Use this method to register the AggregateStore as the implementation for
        /// AggregateStore when configuring services. This enables consuming components to resolve AggregateStore via
        /// dependency injection.</remarks>
        /// <returns>The updated IServiceCollection instance with the AggregateStore service registered.</returns>
        public IServiceCollection AddXAggregateStore()
            => services.AddXAggregateStore<AggregateStore>();

        /// <summary>
        /// Registers the specified aggregate store implementation as a scoped service for the <see
        /// cref="IAggregateStore"/> interface.
        /// </summary>
        /// <remarks>Use this method to configure dependency injection for aggregate store
        /// implementations. Each request will receive a new instance of <typeparamref name="TAggregateStore"/> when
        /// resolving <see cref="IAggregateStore"/>.</remarks>
        /// <typeparam name="TAggregateStore">The type of the aggregate store to register. Must implement <see cref="IAggregateStore"/> and be a reference
        /// type.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the aggregate store service registration added.</returns>
        public IServiceCollection AddXAggregateStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAggregateStore>()
            where TAggregateStore : class, IAggregateStore
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IAggregateStore, TAggregateStore>();
            return services;
        }

        /// <summary>
        /// Registers the specified aggregate store implementation as the scoped service for <see
        /// cref="IAggregateStore"/> in the dependency injection container.
        /// </summary>
        /// <remarks>If an <see cref="IAggregateStore"/> service is already registered, this method will
        /// not overwrite the existing registration. The aggregate store type must be compatible with <see
        /// cref="IAggregateStore"/> and have accessible public constructors for instantiation by the service
        /// provider.</remarks>
        /// <param name="aggregateStoreType">The <see cref="Type"/> representing the aggregate store implementation to register. Must have public
        /// constructors and implement <see cref="IAggregateStore"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance with the aggregate store service registered.</returns>
        public IServiceCollection AddXAggregateStore([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type aggregateStoreType)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped(typeof(IAggregateStore), aggregateStoreType);
            return services;
        }

        /// <summary>
        /// Registers an <see cref="AggregateStore{TAggregate}"/> for the specified aggregate type in the dependency injection container.
        /// </summary>
        /// <remarks>Use this method to enable dependency injection of IAggregateStore for the specified
        /// aggregate type. The registration is added with scoped lifetime.</remarks>
        /// <typeparam name="TAggregate">The aggregate type to register. Must implement both IAggregate and <see cref="IAggregateFactory{TAggregate}"/> .</typeparam>
        /// <returns>The IServiceCollection instance with the aggregate store registration added.</returns>
        public IServiceCollection AddXAggregateStoreFor<TAggregate>()
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
        public IServiceCollection AddXAggregateStoreFor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type aggregateStoreType)
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
        public IServiceCollection AddXAggregateStoreFor() =>
            services.AddXAggregateStoreFor(typeof(AggregateStore<>));

        /// <summary>
        /// Registers the XPipeline event store event decorator in the dependency injection container.
        /// </summary>
        /// <remarks>This method adds the PipelineEventStoreEventDecorator to the service collection, allowing
        /// event store events to be processed through the XPipeline decorator mechanism. Use this method to enable event
        /// decoration in XPipeline-based event store scenarios.</remarks>
        /// <returns>The same IServiceCollection instance, enabling method chaining.</returns>
        public IServiceCollection AddXPipelineEventStoreEventDecorator()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXPipelineDecorator(typeof(PipelineEventStoreEventDecorator<>));
        }

        /// <summary>
        /// Registers the domain events pipeline decorator and its dependencies with the specified service collection.
        /// </summary>
        /// <remarks>This method adds the generic PipelineDomainEventsDecorator to the pipeline and registers the
        /// PendingDomainEventsBuffer for managing pending domain events. Call this method during application startup to
        /// enable domain event handling in the pipeline.</remarks>
        /// <returns>The same service collection instance, with the domain events pipeline decorator and its dependencies registered.</returns>
        public IServiceCollection AddXPipelineDomainEventsDecorator()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXPipelineDecorator(typeof(PipelineDomainEventsDecorator<>))
                .AddScoped<IPendingDomainEventsBuffer, PendingDomainEventsBuffer>();
        }

        /// <summary>
        /// Adds the outbox decorator for pipeline integration to the service collection, enabling buffering and reliable
        /// dispatch of integration events within the pipeline.
        /// </summary>
        /// <remarks>This method registers the outbox decorator and a scoped buffer for pending integration
        /// events, supporting reliable event handling in distributed systems. Call this method during application startup
        /// to ensure integration events are buffered and dispatched as part of the pipeline execution.</remarks>
        /// <returns>The same service collection instance, configured with the outbox decorator and event buffering services.</returns>
        public IServiceCollection AddXPipelineIntegrationOutboxDecorator()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXPipelineDecorator(typeof(PipelineIntegrationOutboxDecorator<>))
                .AddScoped<IPendingIntegrationEventsBuffer, PendingIntegrationEventsBuffer>();
        }


        /// <summary>
        /// Adds a snapshot-based implementation of the aggregate store to the service collection, enabling support for
        /// aggregate snapshots in the application's event sourcing infrastructure.
        /// </summary>
        /// <remarks>This method decorates the existing <see cref="IAggregateStore{T}"/> registration with
        /// a snapshot store implementation. Snapshot support can improve performance for aggregates with large event
        /// histories by reducing the number of events that must be replayed. Use this method when you want to enable
        /// snapshotting for aggregates that implement <see cref="IOriginator"/>.</remarks>
        /// <returns>The same <see cref="IServiceCollection"/> instance, allowing for method chaining.</returns>
        [RequiresUnreferencedCode("The snapshot store may not be fully referenced.")]
        [RequiresDynamicCode("The snapshot store may not be fully dynamic.")]
        public IServiceCollection AddXSnapshotStore() =>
            services.XTryDecorate(
                typeof(IAggregateStore<>),
                typeof(SnapshotStore<>),
                typeof(IOriginator));

        /// <summary>
        /// Registers the specified subscriber type as an implementation of <see cref="ISubscriber"/> with scoped
        /// lifetime in the service collection.
        /// </summary>
        /// <remarks>If an <see cref="ISubscriber"/> service is already registered, this method does not
        /// overwrite the existing registration. Use this method to enable dependency injection of custom subscriber
        /// implementations.</remarks>
        /// <typeparam name="TSubscriber">The subscriber type to register. Must be a non-abstract class that implements <see cref="ISubscriber"/> and
        /// has a public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the subscriber registration added.</returns>
        public IServiceCollection AddXSubscriber<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSubscriber>()
            where TSubscriber : class, ISubscriber
        {
            services.TryAdd(
                new ServiceDescriptor(
                    typeof(ISubscriber),
                    typeof(TSubscriber),
                    ServiceLifetime.Scoped));

            return services;
        }

        /// <summary>
        /// Adds the default XSubscriber implementation to the service collection for dependency injection.
        /// </summary>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the XSubscriber service registered.</returns>
        public IServiceCollection AddXSubscriber() =>
            services.AddXSubscriber<PublisherSubscriber>();

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
        /// Adds an implementation of IScheduler as a hosted service to the dependency injection container.
        /// </summary>
        /// <remarks>This method registers the specified scheduler type and ensures it is started as a
        /// hosted background service. Use this method to enable automatic lifecycle management of the scheduler within
        /// ASP.NET Core applications.</remarks>
        /// <typeparam name="TScheduler">The type of scheduler to register. Must be a class that implements IScheduler and has a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance with the scheduler and hosted service registered.</returns>
        public IServiceCollection AddXSchedulerHosted<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TScheduler>()
            where TScheduler : class, IScheduler =>
            services
                .AddXScheduler<TScheduler>()
                .AddHostedService(provider =>
                    provider.GetRequiredService<IScheduler>());

        /// <summary>
        /// Adds the default hosted Scheduler service to the dependency injection container.
        /// </summary>
        /// <remarks>This method registers the standard <see cref="Scheduler"/> implementation as a hosted
        /// service. Call this method during application startup to enable background scheduling
        /// functionality.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the Scheduler hosted service registered.</returns>
        public IServiceCollection AddXSchedulerHosted() =>
            services.AddXSchedulerHosted<Scheduler>();

        /// <summary>
        /// Registers the specified publisher type as the scoped implementation of <see cref="IPublisher"/> in the
        /// dependency injection container.
        /// </summary>
        /// <remarks>If an <see cref="IPublisher"/> service is already registered, this method does not
        /// overwrite the existing registration. Use this method to enable dependency injection of a custom publisher
        /// implementation within the application's scope.</remarks>
        /// <typeparam name="TPublisher">The publisher type to register. Must be a class that implements <see cref="IPublisher"/> and have a public
        /// constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the publisher registration added.</returns>
        public IServiceCollection AddXPublisher<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPublisher>()
            where TPublisher : class, IPublisher
        {
            services.TryAdd(
                new ServiceDescriptor(
                    typeof(IPublisher),
                    typeof(TPublisher),
                    ServiceLifetime.Scoped));

            return services;
        }

        /// <summary>
        /// Adds the default Publisher implementation to the service collection for dependency injection.
        /// </summary>
        /// <remarks>This method registers the PublisherSubscriber type as the implementation for
        /// XPublisher. Call this method during application startup to enable XPublisher features in your
        /// application.</remarks>
        /// <returns>The updated IServiceCollection instance with the Publisher services registered.</returns>
        public IServiceCollection AddXPublisher() =>
            services.AddXPublisher<PublisherSubscriber>();

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

            return services;
        }

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
        public async Task<IServiceCollection> AddXEventHandlers(params Assembly[] assemblies)
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