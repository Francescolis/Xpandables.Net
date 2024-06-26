﻿/*******************************************************************************
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
********************************************************************************/
using System.Text.Json;

using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents the <see cref="EventEntityDomain"/> converter.
/// </summary>
public sealed class EventEntityDomainConverter : EventConverter<EventEntityDomain>
{
    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(EventEntityDomain);

    ///<inheritdoc/>
    public override IEvent ConvertFrom(
        EventEntityDomain entity,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Type eventType = Type.GetType(entity.EventTypeFullName, true)
            ?? throw new InvalidOperationException(
                $"Type '{entity.EventTypeName}' not found.");

            object? @event = JsonSerializer
                .Deserialize(entity.Data, eventType, options);

            return @event as IEventDomain
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize '{entity.EventTypeName}'.");
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                I18nXpandables.EventConverterFailedToDeserialize
                    .StringFormat(entity.EventTypeName), exception);
        }
    }

    ///<inheritdoc/>
    public override EventEntityDomain ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IEventDomain eventDomain = @event.As<IEventDomain>()
            ?? throw new InvalidOperationException(
                $"Event {@event.GetType().Name} is not an event domain.");

        Guid aggregateId = eventDomain.AggregateId;
        ulong version = eventDomain.Version;
        string eventTypeName = eventDomain.GetTypeName();
        string eventTypeFullName = eventDomain.GetTypeFullName();
        JsonDocument data = eventDomain.ToJsonDocument(options);
        string aggregateTypeName = eventDomain.GetTypeName();

        return new EventEntityDomain(
            eventDomain.Id,
            eventTypeName,
            eventTypeFullName,
            version,
            data,
            aggregateId,
            aggregateTypeName);
    }
}

/// <summary>
/// Represents the <see cref="EventEntityIntegration"/> converter.
/// </summary>
public sealed class EventEntityIntegrationConverter :
    EventConverter<EventEntityIntegration>
{
    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(EventEntityIntegration);

    ///<inheritdoc/>
    public override IEvent ConvertFrom(
        EventEntityIntegration entity,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Type eventType = Type.GetType(entity.EventTypeFullName, true)
         ?? throw new InvalidOperationException(
             $"Type '{entity.EventTypeName}' not found.");

            object? @event = JsonSerializer
                .Deserialize(entity.Data, eventType, options);

            return @event as IEventIntegration
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize '{entity.EventTypeName}'.");
        }
        catch (Exception exception)
           when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                I18nXpandables.EventConverterFailedToDeserialize
                    .StringFormat(entity.EventTypeName), exception);
        }
    }

    ///<inheritdoc/>
    public override EventEntityIntegration ConvertTo(IEvent @event, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IEventIntegration eventIntegration = @event.As<IEventIntegration>()
              ?? throw new InvalidOperationException(
                  $"Event {@event.GetType().Name} is not an integration event.");

        string eventTypeName = eventIntegration.GetTypeName();
        string eventTypeFullName = eventIntegration.GetTypeFullName();
        JsonDocument data = eventIntegration.ToJsonDocument(options);
        ulong version = eventIntegration.Version;

        return new EventEntityIntegration(
            eventIntegration.Id,
            eventTypeName,
            eventTypeFullName,
            version,
            data);
    }
}

/// <summary>
/// Represents the <see cref="EventEntitySnapshot"/> converter.
/// </summary>
public sealed class EventEntitySnapshotConverter :
    EventConverter<EventEntitySnapshot>
{
    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(EventEntitySnapshot);

    ///<inheritdoc/>
    public override IEvent ConvertFrom(
        EventEntitySnapshot entity,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Type eventType = Type.GetType(entity.EventTypeFullName, true)
        ?? throw new InvalidOperationException(
            $"Type '{entity.EventTypeName}' not found.");

            object? @event = JsonSerializer
                .Deserialize(entity.Data, eventType, options);

            return @event as IEventSnapshot
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize '{entity.EventTypeName}'.");
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                I18nXpandables.EventConverterFailedToDeserialize
                    .StringFormat(entity.EventTypeName), exception);
        }
    }

    ///<inheritdoc/>
    public override EventEntitySnapshot ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IEventSnapshot eventSnapshop = @event.As<IEventSnapshot>()
            ?? throw new InvalidOperationException(
                $"Event {@event.GetType().Name} is not a snapshot.");

        string eventTypeName = eventSnapshop.GetTypeName();
        string eventTypeFullName = eventSnapshop.GetTypeFullName();
        JsonDocument data = eventSnapshop.ToJsonDocument(options);
        ulong version = eventSnapshop.Version;

        return new EventEntitySnapshot(
            eventSnapshop.Id,
            eventTypeName,
            eventTypeFullName,
            version,
            data,
            eventSnapshop.ObjectId);
    }
}

/// <summary>
/// Represents the <see cref="IAggregate"/> converter.
/// </summary>
public sealed class AggregateEventConverter :
    EventConverter<IAggregate>
{
    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => typeof(IAggregate).IsAssignableFrom(typeToConvert)
            && typeof(IOriginator).IsAssignableFrom(typeToConvert);

    ///<inheritdoc/>
    public override IEvent ConvertFrom(
        IAggregate entity,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        IOriginator originator = entity.As<IOriginator>()
            ?? throw new InvalidOperationException(
                $"Event {entity.GetTypeName()} is must " +
                $"implement '{nameof(IOriginator)}'.");

        try
        {
            string entityTypeName = entity.GetTypeName();
            string entityTypeFullName = entity.GetTypeFullName();
            IMemento memento = originator.CreateMemento();
            JsonDocument data = memento.ToJsonDocument(options);
            ulong version = entity.Version;

            return new EventSnapshot()
            {
                Memento = memento,
                Version = version,
                ObjectId = entity.AggregateId,
                EntityTypeName = entityTypeName,
                EntityTypeFullName = entityTypeFullName,
            };
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                I18nXpandables.EventConverterFailedToDeserialize
                    .StringFormat(entity.GetTypeName()), exception);
        }
    }

    ///<inheritdoc/>
    public override IAggregate ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IEventSnapshot eventSnapshop = @event.As<IEventSnapshot>()
            ?? throw new InvalidOperationException(
                $"Event {@event.GetType().Name} is not a snapshot.");

        try
        {
            Type type = Type.GetType(eventSnapshop.EntityTypeFullName, true)
           ?? throw new InvalidOperationException(
               $"Type '{eventSnapshop.EntityTypeName}' not found.");

            IOriginator instance = Activator
                .CreateInstance(type, true)
                .As<IOriginator>()
                ?? throw new InvalidOperationException(
                    I18nXpandables.AggregateFailedToCreateInstance
                        .StringFormat(type.GetNameWithoutGenericArity()));

            instance.SetMemento(eventSnapshop.Memento);

            return instance as IAggregate
                ?? throw new InvalidOperationException(
                        I18nXpandables.AggregateFailedToCreateInstance
                            .StringFormat(type.GetNameWithoutGenericArity()));
        }
        catch (Exception exception)
             when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                I18nXpandables.EventConverterFailedToDeserialize
                    .StringFormat(eventSnapshop.EntityTypeName), exception);
        }
    }
}

