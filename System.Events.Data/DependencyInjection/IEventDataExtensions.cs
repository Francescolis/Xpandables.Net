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

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering ADO.NET event store services.
/// </summary>
public static class IEventDataExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the default ADO.NET event store using default entity types.
        /// </summary>
        /// <returns>The updated service collection.</returns>
        [RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
        [UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
        public IServiceCollection AddXEventStore()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXEventStore<System.Events.Data.EventStore<EntityEventDomain, EntityEventSnapshot>>();
        }

        /// <summary>
        /// Registers the default ADO.NET outbox store using default entity types.
        /// </summary>
        /// <returns>The updated service collection.</returns>
        [RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
        [UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
        public IServiceCollection AddXOutboxStore()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXOutboxStore<System.Events.Data.OutboxStore<EntityEventOutbox>>();
        }

        /// <summary>
        /// Registers the default ADO.NET inbox store using default entity types.
        /// </summary>
        /// <returns>The updated service collection.</returns>
        [RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
        [UnconditionalSuppressMessage("Trimming", "IL2066", Justification = "ADO.NET stores are referenced explicitly.")]
        public IServiceCollection AddXInboxStore()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXInboxStore<System.Events.Data.InboxStore<EntityEventInbox>>();
        }

        /// <summary>
        /// Registers default event stores (event store, outbox store, inbox store) for ADO.NET.
        /// </summary>
        /// <returns>The updated service collection.</returns>
        [RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
        [RequiresUnreferencedCode("ADO.NET event store registration uses generic instantiation.")]
        public IServiceCollection AddXEventStores()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXEventStore()
                .AddXOutboxStore()
                .AddXInboxStore();
        }
    }
}
