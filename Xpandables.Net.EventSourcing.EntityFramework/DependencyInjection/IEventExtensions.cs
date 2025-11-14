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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.EventSourcing.Repositories;

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
    extension(IServiceCollection services)
    {
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
        /// Adds the default XEvent converter factory to the service collection.
        /// </summary>
        /// <remarks>Use this method to enable XEvent conversion capabilities in the application's
        /// dependency injection container. This is typically required for components that process or convert XEvent
        /// data.</remarks>
        /// <returns>The updated service collection with the XEvent converter factory registered.</returns>
        public IServiceCollection AddXEventConverterFactory() =>
            services.AddXEventConverterFactory<EventConverterFactory>()
                .AddSingleton<IEventConverter, EventConverterDomain>()
                .AddSingleton<IEventConverter, EventConverterIntegration>()
                .AddSingleton<IEventConverter, EventConverterSnapshot>();
    }
}