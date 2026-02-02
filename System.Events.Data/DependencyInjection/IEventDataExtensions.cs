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

using Microsoft.Extensions.DependencyInjection.Extensions;

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
            return services.AddXEventStore<EventStore<DataEventDomain, DataEventSnapshot>>();
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
            return services.AddXOutboxStore<OutboxStore<DataEventOutbox>>();
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
            return services.AddXInboxStore<InboxStore<DataEventInbox>>();
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

        /// <summary>
        /// Registers the specified event converter factory type as a singleton implementation of <see
        /// cref="IEventConverterFactory"/> in the service collection.
        /// </summary>
        /// <remarks>If an <see cref="IEventConverterFactory"/> service is already registered, this method
        /// does not overwrite the existing registration. This method is typically used to enable custom event
        /// conversion logic in applications that consume event data.</remarks>
        /// <typeparam name="TEventConverterFactory">The type of the event converter factory to register. Must be a class that implements <see
        /// cref="IEventConverterFactory"/> and has a public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the event converter factory registration added.</returns>
        public IServiceCollection AddXEventConverterFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventConverterFactory>()
            where TEventConverterFactory : class, IEventConverterFactory
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddSingleton<IEventConverterFactory, TEventConverterFactory>();
            return services;
        }

        /// <summary>
        /// Adds a singleton implementation of IEventConverterContext to the service collection using the specified
        /// context type.
        /// </summary>
        /// <remarks>If an IEventConverterContext service is already registered, this method does not
        /// overwrite the existing registration. This method is typically used during application startup to configure
        /// event conversion services for dependency injection.</remarks>
        /// <typeparam name="TEventConverterContext">The type that implements IEventConverterContext to be registered as a singleton. Must have a public
        /// constructor.</typeparam>
        /// <returns>The IServiceCollection instance with the IEventConverterContext service registered.</returns>
        public IServiceCollection AddXEventConverterContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventConverterContext>()
            where TEventConverterContext : class, IEventConverterContext
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddSingleton<IEventConverterContext, TEventConverterContext>();
            return services;
        }

        /// <summary>
        /// Adds the default implementation of the event converter context to the service collection.
        /// </summary>
        /// <remarks>This method registers <see cref="IEventConverterContext"/> as a singleton service if
        /// it has not already been registered. Call this method during application startup to enable event conversion
        /// features.</remarks>
        /// <returns>The current <see cref="IServiceCollection"/> instance for method chaining.</returns>
        public IServiceCollection AddXEventConverterContext()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddXEventConverterContext<DefaultEventConverterContext>();
            return services;
        }

        /// <summary>
        /// Adds the default Event converter factory to the service collection.
        /// </summary>
        /// <remarks>Use this method to enable Event conversion capabilities in the application's
        /// dependency injection container. This is typically required for components that process or convert Event
        /// data.</remarks>
        /// <returns>The updated service collection with the Event converter factory registered.</returns>
        public IServiceCollection AddXEventConverterFactory() =>
            services.AddXEventConverterFactory<EventConverterFactory>()
                .AddSingleton<IEventConverter<DataEventDomain, IDomainEvent>, EventConverterDomain>()
                .AddSingleton<IEventConverter<DataEventOutbox, IIntegrationEvent>, EventConverterOutbox>()
                .AddSingleton<IEventConverter<DataEventInbox, IIntegrationEvent>, EventConverterInbox>()
                .AddSingleton<IEventConverter<DataEventSnapshot, ISnapshotEvent>, EventConverterSnapshot>();
    }
}