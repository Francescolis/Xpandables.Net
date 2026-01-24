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
using System.Entities;
using System.Events.Data;

using Microsoft.EntityFrameworkCore;

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
        /// Adds the <see cref="EventDataContext"/>to the service collection with the specified 
        /// options.
        /// </summary>
        /// <param name="optionAction">An action to configure the 
        /// <see cref="DbContextOptionsBuilder"/>.</param>
        /// <returns>The same service collection so that multiple calls can be 
        /// chained.</returns>
        [RequiresUnreferencedCode("This context may be used with unreferenced code.")]
        public IServiceCollection AddXEventDataContext(Action<DbContextOptionsBuilder> optionAction)
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXDataContext<EventDataContext>(optionAction);
        }

        /// <summary>
        /// Registers an event repository for the specified entity event type in the dependency injection container.
        /// </summary>
        /// <remarks>This method adds an event repository for TEntityEvent to the service collection,
        /// enabling event management for entities of the specified type. Ensure that TEntityEvent is a valid class
        /// implementing IEntityEvent before calling this method.</remarks>
        /// <typeparam name="TEntityEvent">The type of the entity event to be managed by the repository. Must be a class that implements the
        /// IEntityEvent interface.</typeparam>
        /// <returns>An IServiceCollection instance that can be used to further configure services.</returns>
        public IServiceCollection AddXEventRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntityEvent>()
            where TEntityEvent : class, IEntityEvent
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXRepository<IRepository<TEntityEvent>, EventRepository<TEntityEvent>>();
        }

        /// <summary>
        /// Registers repositories for handling domain events, event inbox, event outbox, and entity event snapshots
        /// within the service collection to support event-driven architecture.
        /// </summary>
        /// <remarks>This method requires the service collection to be initialized before calling. It adds
        /// repositories for EntityDomainEvent, EntityEventInbox, EntityEventOutbox, and IEntityEventSnapshot, allowing
        /// the application to process and store domain events and related entities.</remarks>
        /// <returns>The updated IServiceCollection instance, enabling further configuration of services.</returns>
        public IServiceCollection AddXEventRepositories()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXEventRepository<EntityDomainEvent>()
                .AddXEventRepository<EntityEventInbox>()
                .AddXEventRepository<EntityEventOutbox>()
                .AddXEventRepository<IEntityEventSnapshot>();
        }
    }
}