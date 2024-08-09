/*******************************************************************************
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

namespace Xpandables.Net.Events;

/// <summary>
/// Represents the <see cref="EntityEventDomain"/> converter.
/// </summary>
public sealed class EventDomainConverter : EventConverter
{
    /// <inheritdoc/>
    public override Type Type => typeof(IEventDomain);

    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => Type.IsAssignableFrom(typeToConvert);

    ///<inheritdoc/>
    public override IEvent ConvertFrom(
        IEntityEvent entity,
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
    public override IEntityEvent ConvertTo(
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

        return new EntityEventDomain(
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
/// Represents the <see cref="EntityEventIntegration"/> converter.
/// </summary>
public sealed class EventIntegrationConverter : EventConverter
{
    /// <inheritdoc/>
    public override Type Type => typeof(IEventIntegration);

    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => Type.IsAssignableFrom(typeToConvert);

    ///<inheritdoc/>
    public override IEvent ConvertFrom(
        IEntityEvent entity,
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
    public override IEntityEvent ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(@event);

        IEventIntegration eventIntegration = @event.As<IEventIntegration>()
              ?? throw new InvalidOperationException(
                  $"Event {@event.GetType().Name} is not an integration event.");

        string eventTypeName = eventIntegration.GetTypeName();
        string eventTypeFullName = eventIntegration.GetTypeFullName();
        JsonDocument data = eventIntegration.ToJsonDocument(options);
        ulong version = eventIntegration.Version;

        return new EntityEventIntegration(
            eventIntegration.Id,
            eventTypeName,
            eventTypeFullName,
            version,
            data);
    }
}

/// <summary>
/// Represents the <see cref="EntityEventSnapshot"/> converter.
/// </summary>
public sealed class EventSnapshotConverter : EventConverter
{
    /// <inheritdoc/>
    public override Type Type => typeof(IEventSnapshot);

    ///<inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => Type.IsAssignableFrom(typeToConvert);

    ///<inheritdoc/>
    public override IEvent ConvertFrom(
        IEntityEvent entity,
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
    public override IEntityEvent ConvertTo(
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

        return new EntityEventSnapshot(
            eventSnapshop.Id,
            eventTypeName,
            eventTypeFullName,
            version,
            data,
            eventSnapshop.KeyId);
    }
}