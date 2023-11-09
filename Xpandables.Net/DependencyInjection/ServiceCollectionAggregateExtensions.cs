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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.Defaults;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.IntegrationEvents;
using Xpandables.Net.Operations.Messaging;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the specified generic 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> type implementations 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="aggregateStoreType">The generic aggregate store.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXAggregateStore(
        this IServiceCollection services,
        Type aggregateStoreType)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IAggregateStore<,>),
                aggregateStoreType,
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default generic 
    /// <see cref="IAggregateStore{TAggregate, TAggregateId}"/> type implementations 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXAggregateStoreDefault(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddXAggregateStore(typeof(AggregateStore<,>));
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
    /// Adds the specified type as <see cref="ISnapshotStore"/> snapshot 
    /// store behavior to command handlers with scoped life time.
    /// </summary>
    /// <remarks>You need to define the <see cref="SnapShotOptions"/> in configuration file.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXSnapshotStore<TSnapshotStore>(this IServiceCollection services)
        where TSnapshotStore : class, ISnapshotStore
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ISnapshotStore, TSnapshotStore>();

        return services;
    }

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
    /// Adds the specified background service implementation event scheduler of <see cref="ITransientScheduler"/>
    /// to manage integration event publishing.
    /// </summary>
    /// <typeparam name="TTransientScheduler">The type that implements <see cref="ITransientScheduler"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientScheduler
        <TTransientScheduler>(this IServiceCollection services)
        where TTransientScheduler : BackgroundServiceBase<TTransientScheduler>, ITransientScheduler
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddXBackgroundService<ITransientScheduler, TTransientScheduler>();

        return services;
    }

    /// <summary>
    /// Adds the default background service implementation event 
    /// scheduler of <see cref="ITransientScheduler"/>
    /// to manage integration event publishing.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientScheduler(this IServiceCollection services)
        => services.AddXTransientScheduler<TransientScheduler>();

    /// <summary>
    /// Registers the implementation as <see cref="IDomainEventStore{TDomainEventRecord}"/> 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TDomainEventRecord">Type of the domain entity.</typeparam>
    /// <typeparam name="TDomainEventStore">The type of that 
    /// implements <see cref="IDomainEventStore{TDomainEventRecord}"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDomainEventStore
        <TDomainEventRecord, TDomainEventStore>(this IServiceCollection services)
        where TDomainEventRecord : class, IEntity
        where TDomainEventStore : class, IDomainEventStore<TDomainEventRecord>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IDomainEventStore<TDomainEventRecord>),
                typeof(TDomainEventStore),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TDomainEventMapper"/> as 
    /// <see cref="IDomainEventMapper{TAggregateId}"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TAggregateId">the type of aggregate Id.</typeparam>
    /// <typeparam name="TDomainEventMapper">The domain event mapper type implementation.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDomainEventMapper
        <TAggregateId, TDomainEventMapper>(this IServiceCollection services)
        where TDomainEventMapper : class, IDomainEventMapper<TAggregateId>
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IDomainEventMapper<TAggregateId>),
                typeof(TDomainEventMapper),
                ServiceLifetime.Scoped));

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
}
