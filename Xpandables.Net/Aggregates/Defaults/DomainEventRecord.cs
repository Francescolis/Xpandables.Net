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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;

using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Extensions;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates.Defaults;

/// <summary>
/// Represents a domain event to be written.
/// Make use of <see langword="using"/> key work when call or call dispose method.
/// </summary>
[DebuggerDisplay("Id = {" + nameof(Id) + "}")]
public sealed class DomainEventRecord : Entity<Guid>, IDisposable
{
    /// <summary>
    /// Constructs a domain event record from the specified event.
    /// </summary>
    /// <typeparam name="TAggregateId">Type of aggregate Id.</typeparam>
    /// <param name="event">The domain event to act with.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>An instance of domain event record built 
    /// from the domain event.</returns>
    public static DomainEventRecord FromDomainEvent<TAggregateId>(
        IDomainEvent<TAggregateId> @event,
        JsonSerializerOptions options)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(@event);

        Guid aggregateId = @event.AggregateId;
        ulong version = @event.Version;
        string typeName = @event.GetTypeName();
        string typeFullName = @event.GetTypeFullName();
        var data = @event.ToJsonDocument(options);
        var aggregateIdName = typeof(TAggregateId).GetNameWithoutGenericArity();

        return new(aggregateId, aggregateIdName, version, typeName, typeFullName, data);
    }

    /// <summary>
    /// Constructs a domain event from the specified record.
    /// </summary>
    /// <typeparam name="TAggregateId">Type of aggregate Id.</typeparam>
    /// <param name="record">The record to act with.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>An instance of domain event built from the entity.</returns>
    public static IDomainEvent<TAggregateId>? ToDomainEventRecord<TAggregateId>(
        DomainEventRecord record,
        JsonSerializerOptions? options)
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(record);

        if (Type.GetType(record.EventTypeFullName) is not { } eventType)
            return default;

        object? eventObject = record.Data.Deserialize(eventType, options);
        return eventObject as IDomainEvent<TAggregateId>;
    }

    /// <summary>
    /// Gets the representation of the aggregate version.
    /// </summary>
    [ConcurrencyCheck]
    public ulong Version { get; }

    /// <summary>
    /// Contains the string representation of the .Net aggregate Id type name.
    /// </summary>
    public string AggregateIdTypeName { get; }

    /// <summary>
    /// Gets the aggregate identifier.
    /// </summary>
    public Guid AggregateId { get; }

    /// <summary>
    /// Contains the string representation of the .Net event type name.
    /// </summary>
    public string EventTypeName { get; }

    /// <summary>
    /// Contains the string representation of the .Net event full assembly qualified type name.
    /// </summary>
    public string EventTypeFullName { get; }

    /// <summary>
    /// Contains the representation of the event as <see cref="JsonDocument"/>.
    /// </summary>
    public JsonDocument Data { get; }

    /// <summary>
    /// Releases the <see cref="Data"/> resource.
    /// </summary>
    public void Dispose()
    {
        Data?.Dispose();
        GC.SuppressFinalize(this);
    }

    ///inheritdoc/>
    private DomainEventRecord(
        Guid aggregateId,
        string aggregateIdTypeName,
        ulong version,
        string eventTypeName,
        string eventTypeFullName,
        JsonDocument data)
    {
        AggregateId = aggregateId;
        AggregateIdTypeName = aggregateIdTypeName;
        Version = version;
        EventTypeName = eventTypeName;
        EventTypeFullName = eventTypeFullName;
        Data = data;
    }
}
