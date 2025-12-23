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
using System.Collections.Frozen;
using System.Events.Domain;
using System.Events.Integration;

namespace System.Events.Data;

/// <summary>
/// Factory that returns the appropriate converter for a given entity event type.
/// </summary>
public sealed class EventConverterFactory : IEventConverterFactory
{
    private readonly FrozenDictionary<Type, object> _converters;

    /// <inheritdoc/>
    public IEventConverterContext ConverterContext { get; }

    /// <summary>
    /// Initializes a new instance of the EventConverterFactory class with the specified event converters.
    /// </summary>
    /// <param name="converterContext">The context for event conversion. Cannot be null.</param>
    /// <param name="domainConverter">The event converter used to convert EntityDomainEvent instances to IDomainEvent instances. Cannot be null.</param>
    /// <param name="integrationConverter">The event converter used to convert EntityIntegrationEvent instances to IIntegrationEvent instances. Cannot be
    /// null.</param>
    /// <param name="snapshotConverter">The event converter used to convert EntitySnapshotEvent instances to ISnapshotEvent instances. Cannot be null.</param>
    public EventConverterFactory(
        IEventConverterContext converterContext,
        IEventConverter<EntityDomainEvent, IDomainEvent> domainConverter,
        IEventConverter<EntityIntegrationEvent, IIntegrationEvent> integrationConverter,
        IEventConverter<EntitySnapshotEvent, ISnapshotEvent> snapshotConverter)
    {
        ArgumentNullException.ThrowIfNull(domainConverter);
        ArgumentNullException.ThrowIfNull(integrationConverter);
        ArgumentNullException.ThrowIfNull(snapshotConverter);

        ConverterContext = converterContext ?? throw new ArgumentNullException(nameof(converterContext));

        _converters = new Dictionary<Type, object>
        {
            [typeof(EntityDomainEvent)] = domainConverter,
            [typeof(EntityIntegrationEvent)] = integrationConverter,
            [typeof(EntitySnapshotEvent)] = snapshotConverter
        }.ToFrozenDictionary();
    }

    /// <inheritdoc/>
    public IEventConverter<TEntityEventDomain, IDomainEvent> GetDomainEventConverter<TEntityEventDomain>()
        where TEntityEventDomain : class, IEntityEventDomain
    {
        if (_converters.TryGetValue(typeof(TEntityEventDomain), out object? converter) &&
            converter is IEventConverter<TEntityEventDomain, IDomainEvent> typed)
        {
            return typed;
        }

        throw new InvalidOperationException(
            $"No converter registered for entity event type '{typeof(TEntityEventDomain).Name}'.");
    }

    /// <inheritdoc/>
    public IEventConverter<TEntityIntegrationEvent, IIntegrationEvent> GetIntegrationEventConverter<TEntityIntegrationEvent>()
        where TEntityIntegrationEvent : class, IEntityEventIntegration
    {
        if (_converters.TryGetValue(typeof(TEntityIntegrationEvent), out object? converter) &&
            converter is IEventConverter<TEntityIntegrationEvent, IIntegrationEvent> typed)
        {
            return typed;
        }

        throw new InvalidOperationException(
            $"No converter registered for entity event type '{typeof(TEntityIntegrationEvent).Name}'.");
    }

    /// <inheritdoc/>
    public IEventConverter<TEntitySnapshotEvent, ISnapshotEvent> GetSnapshotEventConverter<TEntitySnapshotEvent>()
        where TEntitySnapshotEvent : class, IEntityEventSnapshot
    {
        if (_converters.TryGetValue(typeof(TEntitySnapshotEvent), out object? converter) &&
            converter is IEventConverter<TEntitySnapshotEvent, ISnapshotEvent> typed)
        {
            return typed;
        }

        throw new InvalidOperationException(
            $"No converter registered for entity event type '{typeof(TEntitySnapshotEvent).Name}'.");
    }
}
