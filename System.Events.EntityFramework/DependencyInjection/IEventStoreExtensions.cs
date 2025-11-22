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
        /// Adds the default implementation of the EventStore to the specified service collection.
        /// </summary>
        /// <remarks>This method registers the default implementation of <see cref="EventStore"/> with the
        /// dependency injection container. It is a convenience method for adding the EventStore with a predefined event
        /// type.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddXEventStore()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXEventStore<EventStore>();
        }

        /// <summary>
        /// Adds the EventStore service to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>This method registers the EventStore service with the dependency injection container,  using
        /// the specified data context type. The data context type must inherit from <see cref="EventDataContext"/>.</remarks>
        /// <typeparam name="TEventStore">The type of the data context used by the event store. Must derive from <see cref="EventDataContext"/>.</typeparam>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddXEventStore<TEventStore>()
            where TEventStore : class, IEventStore
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddScoped<IEventStore, EventStore>();
            return services;
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
            services.AddScoped<IOutboxStore, TOutboxStore>();
            return services;
        }

        /// <summary>
        /// Adds the default implementation of <see cref="IOutboxStore"/> to the service collection.
        /// </summary>
        /// <remarks>This method registers the <see cref="OutboxStore"/> implementation of <see
        /// cref="IOutboxStore"/>  with a scoped lifetime. It is intended to be used in applications that require outbox
        /// pattern support.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
        public IServiceCollection AddXOutboxStore()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXOutboxStore<OutboxStore>();
        }
    }
}