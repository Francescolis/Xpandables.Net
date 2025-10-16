
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
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Events;
using Xpandables.Net.Repositories;

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
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds the DataContextEvent to the service collection with the specified 
    /// options.
    /// </summary>
    /// <param name="services">The service collection to add the context to.</param>
    /// <param name="optionAction">An action to configure the 
    /// <see cref="DbContextOptionsBuilder"/>.</param>
    /// <returns>The same service collection so that multiple calls can be 
    /// chained.</returns>
    [RequiresUnreferencedCode("This method may be removed in a future version.")]
    public static IServiceCollection AddXDataContextEvent(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddXDataContext<DataContextEvent>(optionAction);
    }

    /// <summary>
    /// Adds the default implementation of the XEventStore to the specified service collection.
    /// </summary>
    /// <remarks>This method registers the default implementation of <see cref="EventStore{TEvent}"/> with the
    /// dependency injection container. It is a convenience method for adding the EventStore with a predefined event
    /// type.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the EventStore will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddXEventStore<DataContextEvent>();
    }

    /// <summary>
    /// Adds the EventStore service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method registers the EventStore service with the dependency injection container,  using
    /// the specified data context type. The data context type must inherit from <see cref="DataContext"/>.</remarks>
    /// <typeparam name="TDataContext">The type of the data context used by the event store. Must derive from <see cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the EventStore service will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStore<TDataContext>(this IServiceCollection services)
        where TDataContext : DataContext
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddScoped<IEventStore, EventStore<TDataContext>>();
    }

    /// <summary>
    /// Registers the default implementation of <see cref="IOutboxStore"/> using the specified  <typeparamref
    /// name="TDataContext"/> as the data context.
    /// </summary>
    /// <remarks>This method registers the <see cref="OutboxStore{TDataContext}"/> as a scoped service  for
    /// the <see cref="IOutboxStore"/> interface. Ensure that <typeparamref name="TDataContext"/>  is properly
    /// configured in the application's dependency injection container.</remarks>
    /// <typeparam name="TDataContext">The type of the data context to be used by the <see cref="OutboxStore{TDataContext}"/>.  Must derive from <see
    /// cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the <see cref="IOutboxStore"/> service is added.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance with the <see cref="IOutboxStore"/> service registered.</returns>
    public static IServiceCollection AddXOutboxStore<TDataContext>(this IServiceCollection services)
        where TDataContext : DataContext
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IOutboxStore, OutboxStore<TDataContext>>();
        return services;
    }

    /// <summary>
    /// Adds the default implementation of <see cref="IOutboxStore"/> to the service collection.
    /// </summary>
    /// <remarks>This method registers the <see cref="OutboxStore{T}"/> implementation of <see
    /// cref="IOutboxStore"/>  with a scoped lifetime. It is intended to be used in applications that require outbox
    /// pattern support.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the outbox store will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXOutboxStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IOutboxStore, OutboxStore<DataContextEvent>>();
        return services;
    }
}
