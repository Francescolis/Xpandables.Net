
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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Xpandables.Net;
using Xpandables.Net.Events;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides an implementation of an aggregate store that loads and saves aggregates using an event store and manages
/// pending domain events.
/// </summary>
/// <remarks>This class is typically used in domain-driven design architectures to manage the persistence and
/// reconstruction of aggregates from event streams. It ensures that aggregates are loaded from their event history and
/// that uncommitted domain events are properly tracked and dispatched after saving.</remarks>
/// <param name="eventStore">The event store used to persist and retrieve aggregate events.</param>
/// <param name="domainEvents">The collection used to track and dispatch pending domain events after aggregates are saved.</param>
/// <param name="cacheTypeResolver">The type resolver used to resolve aggregate types by name.</param>
public sealed class AggregateStore(
    IEventStore eventStore,
    IPendingDomainEventsBuffer domainEvents,
    ICacheTypeResolver cacheTypeResolver) : IAggregateStore
{
    private static readonly MemoryAwareCache<string, MethodInfo> _aggregateTypeCache = new();
    private readonly IEventStore _eventStore = eventStore;
    private readonly ICacheTypeResolver _cacheTypeResolver = cacheTypeResolver;

    /// <inheritdoc />
    public async Task<IAggregate> LoadAsync(
        Guid streamId,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(streamId, Guid.Empty);

        ReadStreamRequest request = new()
        {
            StreamId = streamId,
            FromVersion = -1,
            MaxCount = int.MaxValue
        };

        var history = await _eventStore
            .ReadStreamAsync(request, cancellationToken)
            .Select(e => e.Event)
            .OfType<IDomainEvent>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (history.Count == 0)
        {
            throw new ValidationException(new ValidationResult(
                "No events were found for the specified aggregate.",
                [nameof(streamId)]), null, streamId);
        }

        string aggregateTypeName = history.First().StreamName;
        MethodInfo aggregateFactory = ResolveAggregateFactoryMethod(aggregateTypeName);
        var aggregate = (IAggregate)aggregateFactory.Invoke(null, null)!;

        aggregate.Replay(history);

        if (aggregate.IsEmpty)
        {
            throw new ValidationException(new ValidationResult(
                "Events were found but the aggregate could not be rehydrated.",
                [nameof(streamId)]), null, streamId);
        }

        return aggregate;
    }

    /// <inheritdoc />
    public async Task SaveAsync(
        IAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var pending = aggregate.DequeueUncommittedEvents();
        if (pending.Count == 0) return;

        // The aggregate.StreamVersion has already been advanced by pending.Count.
        // expectedVersion must reflect the persisted version before those events.
        var expectedVersion = aggregate.StreamVersion - pending.Count;

        AppendRequest request = new()
        {
            StreamId = aggregate.StreamId,
            Events = pending,
            ExpectedVersion = expectedVersion
        };

        await _eventStore.AppendToStreamAsync(request, cancellationToken).ConfigureAwait(false);

        domainEvents.AddRange(pending, aggregate.MarkEventsAsCommitted);
    }

    /// <summary>
    /// Resolves the public static Initialize method of an aggregate type by its name, ensuring the type implements
    /// IAggregateFactory.
    /// </summary>
    /// <remarks>This method requires the aggregate type to be available at runtime and referenced by name.
    /// The resolved type must implement IAggregateFactory and declare a public static Initialize method with no
    /// parameters. May use reflection and dynamic code; ensure all required types are preserved if using trimming or
    /// AOT compilation.</remarks>
    /// <param name="name">The fully qualified name of the aggregate type to resolve. Cannot be null.</param>
    /// <returns>A MethodInfo representing the public static Initialize method of the specified aggregate type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified type cannot be found, does not implement IAggregateFactory, or does not have a public
    /// static Initialize method.</exception>
    [RequiresUnreferencedCode("May use unreferenced code to resolve type.")]
    [RequiresDynamicCode("May use dynamic code to resolve type.")]
    public MethodInfo ResolveAggregateFactoryMethod(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _aggregateTypeCache.GetOrAdd(name, name =>
        {
            Type eventType = _cacheTypeResolver.TryResolve(name)
            ?? throw new InvalidOperationException(
                $"The aggregate type '{name}' could not be found. Ensure it is referenced and available at runtime.");

            if (!typeof(IAggregateFactory).IsAssignableFrom(eventType))
            {
                throw new InvalidOperationException(
                    $"The aggregate type '{eventType.FullName}' does not implement IAggregateFactory.");
            }

            var initializeMethod = eventType.GetMethod(
                "Initialize",
                BindingFlags.Public | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null) ?? throw new InvalidOperationException(
                    $"The aggregate type '{eventType.FullName}' does not have a public static Initialize method.");

            return initializeMethod;
        })
            ?? throw new InvalidOperationException(
                $"The aggregate type '{name}' could not be found. Ensure it is referenced and available at runtime.");
    }
}

/// <summary>
/// Provides an implementation of an aggregate store that loads and saves aggregates using an event store and manages
/// pending domain events.
/// </summary>
/// <remarks>This class is typically used in domain-driven design architectures to manage the persistence and
/// reconstruction of aggregates from event streams. It ensures that aggregates are loaded from their event history and
/// that uncommitted domain events are properly tracked and dispatched after saving.</remarks>
/// <typeparam name="TAggregate">The type of aggregate managed by the store. Must implement <see cref="IAggregate"/> and have a parameterless
/// constructor.</typeparam>
/// <param name="eventStore">The event store used to persist and retrieve aggregate events.</param>
/// <param name="domainEvents">The collection used to track and dispatch pending domain events after aggregates are saved.</param>
public sealed class AggregateStore<TAggregate>(
    IEventStore eventStore,
    IPendingDomainEventsBuffer domainEvents) : IAggregateStore<TAggregate>
    where TAggregate : class, IAggregate, IAggregateFactory<TAggregate>
{
    private readonly IEventStore _eventStore = eventStore;

    /// <inheritdoc />
    public async Task<TAggregate> LoadAsync(
        Guid streamId,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(streamId, Guid.Empty);

        ReadStreamRequest request = new()
        {
            StreamId = streamId,
            FromVersion = -1,
            MaxCount = int.MaxValue
        };

        var history = await _eventStore
            .ReadStreamAsync(request, cancellationToken)
            .Select(e => e.Event)
            .OfType<IDomainEvent>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var aggregate = TAggregate.Initialize();

        aggregate.Replay(history);

        if (aggregate.IsEmpty)
        {
            throw new ValidationException(new ValidationResult(
                "The aggregate was not found.",
                [nameof(streamId)]), null, streamId);
        }

        return aggregate;
    }

    /// <inheritdoc />
    public async Task SaveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var pending = aggregate.DequeueUncommittedEvents();
        if (pending.Count == 0) return;

        // The aggregate.StreamVersion has already been advanced by pending.Count.
        // expectedVersion must reflect the persisted version before those events.
        var expectedVersion = aggregate.StreamVersion - pending.Count;

        AppendRequest request = new()
        {
            StreamId = aggregate.StreamId,
            Events = pending,
            ExpectedVersion = expectedVersion
        };

        await _eventStore.AppendToStreamAsync(request, cancellationToken).ConfigureAwait(false);

        domainEvents.AddRange(pending, aggregate.MarkEventsAsCommitted);
    }
}