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
using System.Events.Data;
using System.Events.Domain;
using System.Events.Integration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods for registering Entity Framework repository services.
/// </summary>
public static class IEventStoreExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the specified data context type to the service collection with configurable options and lifetimes.
        /// </summary>
        /// <remarks>Use this method to register a custom data context for dependency injection, allowing
        /// configuration of its options and service lifetimes. This is typically called during application
        /// startup.</remarks>
        /// <typeparam name="TDataContext">The type of the data context to register. Must inherit from DataContext.</typeparam>
        /// <param name="optionsAction">An optional action to configure the DbContext options for the data context. If null, default options are
        /// used.</param>
        /// <param name="contextLifetime">The lifetime with which to register the data context. The default is Scoped.</param>
        /// <param name="optionsLifetime">The lifetime with which to register the options instance. The default is Scoped.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddXEventDataContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TDataContext>(
            Action<DbContextOptionsBuilder>? optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TDataContext : EventDataContext =>
            services.AddDbContext<TDataContext>(
                optionsAction, contextLifetime, optionsLifetime);

        /// <summary>
        /// Adds the <see cref="EventStoreDataContext"/>to the service collection with the specified 
        /// options.
        /// </summary>
        /// <param name="optionAction">An action to configure the 
        /// <see cref="DbContextOptionsBuilder"/>.</param>
        /// <returns>The same service collection so that multiple calls can be 
        /// chained.</returns>
        [RequiresUnreferencedCode("This context may be used with unreferenced code.")]
        public IServiceCollection AddXEventStoreDataContext(Action<DbContextOptionsBuilder> optionAction)
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXEventDataContext<EventStoreDataContext>(optionAction);
        }

        /// <summary>
        /// Adds the <see cref="OutboxStoreDataContext"/>to the service collection with the specified 
        /// options.
        /// </summary>
        /// <param name="optionAction">An action to configure the 
        /// <see cref="DbContextOptionsBuilder"/>.</param>
        /// <returns>The same service collection so that multiple calls can be 
        /// chained.</returns>
        [RequiresUnreferencedCode("This context may be used with unreferenced code.")]
        public IServiceCollection AddXOutboxStoreDataContext(Action<DbContextOptionsBuilder> optionAction)
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXEventDataContext<OutboxStoreDataContext>(optionAction);
        }

        /// <summary>
        /// Adds a scoped implementation of the specified event store data context factory to the service collection.
        /// </summary>
        /// <remarks>This method registers the specified factory type as the implementation for
        /// IEventStoreDataContextFactory using scoped lifetime. Subsequent requests for IEventStoreDataContextFactory
        /// will resolve to the registered factory type.</remarks>
        /// <typeparam name="TEventStoreDataContextFactory">The type of the event store data context factory to register. Must implement IEventStoreDataContextFactory
        /// and have a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance with the event store data context factory registered.</returns>
        public IServiceCollection AddXEventStoreDataContextFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventStoreDataContextFactory>()
            where TEventStoreDataContextFactory : class, IEventStoreDataContextFactory
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IEventStoreDataContextFactory, TEventStoreDataContextFactory>();
            return services;
        }

        /// <summary>
        /// Adds the default implementation of IEventStoreDataContextFactory to the service collection with scoped
        /// lifetime. 
        /// </summary>
        /// <remarks>Call this method to enable dependency injection for event store data context
        /// factories in your application. This method should be called once during service configuration.</remarks>
        /// <returns>The IServiceCollection instance with the IEventStoreDataContextFactory service registered.</returns>
        public IServiceCollection AddXEventStoreDataContextFactory()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IEventStoreDataContextFactory, EventStoreDataContextFactory>();
            return services;
        }

        /// <summary>
        /// Adds the specified implementation of the IOutboxStoreDataContextFactory interface to the service collection
        /// with scoped lifetime.
        /// </summary>
        /// <remarks>If an IOutboxStoreDataContextFactory service has not already been registered, this
        /// method registers TOutboxStoreDataContextFactory as its implementation. This method is intended for use with
        /// dependency injection in ASP.NET Core applications.</remarks>
        /// <typeparam name="TOutboxStoreDataContextFactory">The type that implements IOutboxStoreDataContextFactory to register. Must have a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance for chaining further configuration.</returns>
        public IServiceCollection AddXOutboxStoreDataContextFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TOutboxStoreDataContextFactory>()
            where TOutboxStoreDataContextFactory : class, IOutboxStoreDataContextFactory
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IOutboxStoreDataContextFactory, TOutboxStoreDataContextFactory>();
            return services;
        }

        /// <summary>
        /// Adds the default implementation of the IOutboxStoreDataContextFactory to the service collection with scoped
        /// lifetime.
        /// </summary>
        /// <remarks>Call this method to enable dependency injection for outbox store data context
        /// factories in your application. This method registers OutboxStoreDataContextFactory as the implementation for
        /// IOutboxStoreDataContextFactory if it has not already been registered.</remarks>
        /// <returns>The IServiceCollection instance with the IOutboxStoreDataContextFactory service registered.</returns>
        public IServiceCollection AddXOutboxStoreDataContextFactory()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IOutboxStoreDataContextFactory, OutboxStoreDataContextFactory>();
            return services;
        }

        /// <summary>
        /// Adds an EventStore implementation for the specified domain event and snapshot event types to the service
        /// collection.
        /// </summary>
        /// <typeparam name="TEntityDomainEvent">The type representing the domain event. Must implement the IEntityEventDomain interface and have a public
        /// constructor.</typeparam>
        /// <typeparam name="TEntitySnapShotEvent">The type representing the snapshot event. Must implement the IEntityEventSnapshot interface and have a
        /// public constructor.</typeparam>
        /// <returns>The IServiceCollection instance with the XEventStore service registered. This enables further configuration
        /// of the service collection.</returns>
        public IServiceCollection AddXEventStoreOfType<[DynamicallyAccessedMembers(EntityEvent.DynamicallyAccessedMemberTypes)] TEntityDomainEvent, [DynamicallyAccessedMembers(EntityEvent.DynamicallyAccessedMemberTypes)] TEntitySnapShotEvent>()
            where TEntityDomainEvent : class, IEntityEventDomain
            where TEntitySnapShotEvent : class, IEntityEventSnapshot
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXEventStore<EventStore<TEntityDomainEvent, TEntitySnapShotEvent>>();
        }

        /// <summary>
        /// Adds the specified event store implementation to the service collection with a scoped lifetime.
        /// </summary>
        /// <remarks>This method registers the specified event store type as the implementation for
        /// IEventStore. If an IEventStore service is already registered, this method does not overwrite the existing
        /// registration.</remarks>
        /// <typeparam name="TEventStore">The type of the event store to register. Must implement the IEventStore interface and have a public
        /// constructor.</typeparam>
        /// <returns>The IServiceCollection instance for chaining additional service configuration.</returns>
        public IServiceCollection AddXEventStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventStore>()
            where TEventStore : class, IEventStore
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IEventStore, TEventStore>();
            return services;
        }

        /// <summary>
        /// Adds the default XEventStore services for handling entity domain and snapshot events to the current service
        /// collection.
        /// </summary>
        /// <remarks>This method registers the XEventStore infrastructure using the default event types
        /// <see cref="EntityDomainEvent"/> and <see cref="EntitySnapshotEvent"/>. Call this method during application
        /// startup to enable event sourcing features for entities.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with XEventStore services registered. This enables further
        /// configuration of the service collection.</returns>
        public IServiceCollection AddXEventStore()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXEventStoreDataContextFactory()
                .AddXEventStoreOfType<EntityDomainEvent, EntitySnapshotEvent>();
        }

        /// <summary>
        /// Registers the specified outbox store implementation as a scoped service in the dependency injection container.
        /// </summary>
        /// <remarks>This method adds <typeparamref name="TOutboxStore"/> as the implementation for <see
        /// cref="IOutboxStore"/> with a scoped lifetime. Use this method to configure custom outbox store implementations
        /// for dependency injection.</remarks>
        /// <typeparam name="TOutboxStore">The type of outbox store to register. Must implement <see cref="IOutboxStore"/>.</typeparam>
        /// <returns>The same <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
        public IServiceCollection AddXOutboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TOutboxStore>()
            where TOutboxStore : class, IOutboxStore
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IOutboxStore, TOutboxStore>();
            return services;
        }

        /// <summary>
        /// Adds the default implementation of <see cref="IOutboxStore"/> to the service collection.
        /// </summary>
        /// <remarks>This method registers the <see cref="OutboxStore{TEntityIntegrationEvent}"/> implementation of <see
        /// cref="IOutboxStore"/>  with a scoped lifetime. It is intended to be used in applications that require outbox
        /// pattern support.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddXOutboxStoreOfType<[DynamicallyAccessedMembers(EntityEvent.DynamicallyAccessedMemberTypes)] TEntityEventIntegration>()
            where TEntityEventIntegration : class, IEntityEventOutbox
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXOutboxStore<OutboxStore<TEntityEventIntegration>>();
        }

        /// <summary>
        /// Adds the default XOutbox store for handling integration events to the service collection.
        /// </summary>
        /// <remarks>This method registers the XOutbox store using the default event type, <see
        /// cref="EntityEventOutbox"/>. Call this method during application startup to enable outbox pattern
        /// support for integration events.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the XOutbox store services registered.</returns>
        public IServiceCollection AddXOutboxStore()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXOutboxStoreDataContextFactory()
                .AddXOutboxStoreOfType<EntityEventOutbox>();
        }

        /// <summary>
        /// Adds the <see cref="InboxStoreDataContext"/> to the service collection with the specified options.
        /// </summary>
        /// <param name="optionAction">An action to configure the <see cref="DbContextOptionsBuilder"/>.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        [RequiresUnreferencedCode("This context may be used with unreferenced code.")]
        public IServiceCollection AddXInboxStoreDataContext(Action<DbContextOptionsBuilder> optionAction)
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXEventDataContext<InboxStoreDataContext>(optionAction);
        }

        /// <summary>
        /// Adds the specified implementation of the <see cref="IInboxStoreDataContextFactory"/> interface 
        /// to the service collection with scoped lifetime.
        /// </summary>
        /// <typeparam name="TInboxStoreDataContextFactory">The type that implements <see cref="IInboxStoreDataContextFactory"/> to register.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance for chaining further configuration.</returns>
        public IServiceCollection AddXInboxStoreDataContextFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInboxStoreDataContextFactory>()
            where TInboxStoreDataContextFactory : class, IInboxStoreDataContextFactory
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IInboxStoreDataContextFactory, TInboxStoreDataContextFactory>();
            return services;
        }

        /// <summary>
        /// Adds the default implementation of the <see cref="IInboxStoreDataContextFactory"/> to the service 
        /// collection with scoped lifetime.
        /// </summary>
        /// <returns>The <see cref="IServiceCollection"/> instance with the <see cref="IInboxStoreDataContextFactory"/> service registered.</returns>
        public IServiceCollection AddXInboxStoreDataContextFactory()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IInboxStoreDataContextFactory, InboxStoreDataContextFactory>();
            return services;
        }

        /// <summary>
        /// Registers the specified inbox store implementation as a scoped service in the dependency injection container.
        /// </summary>
        /// <typeparam name="TInboxStore">The type of inbox store to register. Must implement <see cref="IInboxStore"/>.</typeparam>
        /// <returns>The same <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
        public IServiceCollection AddXInboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInboxStore>()
            where TInboxStore : class, IInboxStore
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IInboxStore, TInboxStore>();
            return services;
        }

        /// <summary>
        /// Adds the <see cref="InboxStore{TEntityEventInbox}"/> implementation of <see cref="IInboxStore"/>
        /// for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntityEventInbox">The inbox entity type. Must implement <see cref="IEntityEventInbox"/>.</typeparam>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddXInboxStoreOfType<[DynamicallyAccessedMembers(EntityEvent.DynamicallyAccessedMemberTypes)] TEntityEventInbox>()
            where TEntityEventInbox : class, IEntityEventInbox
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXInboxStore<InboxStore<TEntityEventInbox>>();
        }

        /// <summary>
        /// Adds the default inbox store for handling integration event idempotency to the service collection.
        /// </summary>
        /// <remarks>
        /// This method registers the inbox store using the default event type <see cref="EntityEventInbox"/>.
        /// The inbox pattern ensures exactly-once delivery by deduplicating incoming events based on 
        /// (EventId, Consumer) composite key.
        /// </remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the inbox store services registered.</returns>
        public IServiceCollection AddXInboxStore()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXInboxStoreDataContextFactory()
                .AddXInboxStoreOfType<EntityEventInbox>();
        }
    }
}