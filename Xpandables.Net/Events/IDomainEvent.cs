
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents a domain event that includes versioning and an aggregate identifier.
/// </summary>
public interface IDomainEvent : IEvent
{
    /// <summary>
    /// Gets the aggregate identifier associated with the event.
    /// </summary>
    /// <remarks>It's based on the <see cref="Guid.NewGuid" />.</remarks>
    Guid AggregateId { get; init; }

    /// <summary>
    /// Gets the aggregate type name that this event belongs to.
    /// </summary>
    /// <remarks>Uses the full type name for better uniqueness across assemblies.</remarks>
    string AggregateName { get; }

    /// <summary>
    /// Gets the stream version (position) of this event within the aggregate's event stream.
    /// This is used for proper event ordering and optimistic concurrency control.
    /// </summary>
    long StreamVersion { get; init; }

    /// <summary>
    /// Gets the identifier of the event that caused this event (for causation tracking).
    /// </summary>
    /// <remarks>Null if this event was not caused by another event.</remarks>
    Guid? CausationId { get; init; }

    /// <summary>
    /// Gets the correlation identifier for tracking related events across aggregates.
    /// </summary>
    /// <remarks>Null if this event is not part of a correlated flow.</remarks>
    Guid? CorrelationId { get; init; }

    /// <summary>
    /// Gets metadata associated with the event as a JSON-serializable dictionary.
    /// </summary>
    /// <remarks>Values should be JSON-serializable types (string, number, bool, null, or nested objects/arrays).</remarks>
    IReadOnlyDictionary<string, object?> Metadata { get; init; }

    /// <summary>
    /// Sets the stream version of the event.
    /// </summary>
    /// <param name="streamVersion">The stream version to set.</param>
    /// <returns>The event domain with the specified stream version.</returns>
    IDomainEvent WithStreamVersion(long streamVersion);

    /// <summary>
    /// Sets the aggregate identifier of the event.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier to set.</param>
    /// <returns>The event domain with the specified aggregate identifier.</returns>
    IDomainEvent WithAggregateId(Guid aggregateId);

    /// <summary>
    /// Sets the causation identifier of the event.
    /// </summary>
    /// <param name="causationId">The causation identifier to set.</param>
    /// <returns>The event domain with the specified causation identifier.</returns>
    IDomainEvent WithCausation(Guid causationId);

    /// <summary>
    /// Sets the correlation identifier of the event.
    /// </summary>
    /// <param name="correlationId">The correlation identifier to set.</param>
    /// <returns>The event domain with the specified correlation identifier.</returns>
    IDomainEvent WithCorrelation(Guid correlationId);

    /// <summary>
    /// Adds metadata to the event.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value (must be JSON-serializable).</param>
    /// <returns>The event domain with the added metadata.</returns>
    IDomainEvent WithMetadata(string key, object? value);
}

/// <summary>
/// Represents a domain event that includes versioning for a specific aggregate type.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate, which must inherit from <see cref="Aggregate"/>.</typeparam>
public interface IDomainEvent<TAggregate> : IDomainEvent
    where TAggregate : Aggregate
{
    /// <summary>
    /// Gets the name of the aggregate type that this event belongs to.
    /// </summary>
    public new string AggregateName => typeof(TAggregate).FullName!;

    /// <summary>
    /// Sets the stream version of the event.
    /// </summary>
    /// <param name="streamVersion">The stream version to set.</param>
    /// <returns>The event domain with the specified stream version.</returns>
    new IDomainEvent<TAggregate> WithStreamVersion(long streamVersion);

    /// <summary>
    /// Sets the aggregate identifier of the event.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier to set.</param>
    /// <returns>The event domain with the specified aggregate identifier.</returns>
    new IDomainEvent<TAggregate> WithAggregateId(Guid aggregateId);

    /// <summary>
    /// Sets the causation identifier of the event.
    /// </summary>
    /// <param name="causationId">The causation identifier to set.</param>
    /// <returns>The event domain with the specified causation identifier.</returns>
    new IDomainEvent<TAggregate> WithCausation(Guid causationId);

    /// <summary>
    /// Sets the correlation identifier of the event.
    /// </summary>
    /// <param name="correlationId">The correlation identifier to set.</param>
    /// <returns>The event domain with the specified correlation identifier.</returns>
    new IDomainEvent<TAggregate> WithCorrelation(Guid correlationId);

    /// <summary>
    /// Adds metadata to the event.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value (must be JSON-serializable).</param>
    /// <returns>The event domain with the added metadata.</returns>
    new IDomainEvent<TAggregate> WithMetadata(string key, object? value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    string IDomainEvent.AggregateName => AggregateName;

    [EditorBrowsable(EditorBrowsableState.Never)]
    IDomainEvent IDomainEvent.WithStreamVersion(long streamVersion) => WithStreamVersion(streamVersion);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IDomainEvent IDomainEvent.WithAggregateId(Guid aggregateId) => WithAggregateId(aggregateId);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IDomainEvent IDomainEvent.WithCausation(Guid causationId) => WithCausation(causationId);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IDomainEvent IDomainEvent.WithCorrelation(Guid correlationId) => WithCorrelation(correlationId);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IDomainEvent IDomainEvent.WithMetadata(string key, object? value) => WithMetadata(key, value);
}

/// <summary>
/// Represents a domain event that is associated with an aggregate.
/// </summary>
public abstract record DomainEvent : Event, IDomainEvent
{
    /// <inheritdoc />
    public required Guid AggregateId { get; init; }

    /// <inheritdoc />
    public required string AggregateName { get; init; }

    /// <inheritdoc/>
    public long StreamVersion { get; init; }

    /// <inheritdoc />
    public Guid? CausationId { get; init; }

    /// <inheritdoc />
    public Guid? CorrelationId { get; init; }

    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringObjectDictionaryConverter))]
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } =
        new Dictionary<string, object?>();

    /// <inheritdoc />
    public virtual IDomainEvent WithStreamVersion(long streamVersion) =>
        this with { StreamVersion = streamVersion };

    /// <inheritdoc />
    public virtual IDomainEvent WithAggregateId(Guid aggregateId) =>
        this with { AggregateId = aggregateId };

    /// <inheritdoc />
    public virtual IDomainEvent WithCausation(Guid causationId) =>
        this with { CausationId = causationId };

    /// <inheritdoc />
    public virtual IDomainEvent WithCorrelation(Guid correlationId) =>
        this with { CorrelationId = correlationId };

    /// <inheritdoc />
    public virtual IDomainEvent WithMetadata(string key, object? value)
    {
        var newMetadata = new Dictionary<string, object?>(Metadata) { [key] = value };
        return this with { Metadata = newMetadata };
    }
}

/// <summary>
/// Represents a domain event that is associated with an aggregate root.
/// </summary>
/// <remarks>
/// Add a private parameterless constructor and decorate it
/// with the <see cref="JsonConstructorAttribute" /> attribute when you
/// are using the base constructor with <typeparamref name="TAggregate" />.
/// </remarks>
public abstract record DomainEvent<TAggregate> : Event, IDomainEvent<TAggregate>
    where TAggregate : Aggregate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent{TAggregate}" /> class.
    /// </summary>
    [JsonConstructor]
    protected DomainEvent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent{TAggregate}" /> class
    /// with the specified aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate associated with this event.</param>
    /// <remarks>Create another constructor for the <see cref="JsonConstructorAttribute" /> when you
    /// use this constructor.</remarks>
    [SetsRequiredMembers]
    protected DomainEvent(TAggregate aggregate)
    {
        AggregateId = aggregate.KeyId;
        AggregateName = typeof(TAggregate).FullName!;
    }

    /// <inheritdoc />
    public required Guid AggregateId { get; init; }

    /// <inheritdoc />
    /// <remarks>This property is initialized with the full name of the aggregate type.</remarks>
    public string AggregateName { get; init; } = typeof(TAggregate).FullName!;

    /// <inheritdoc/>
    public long StreamVersion { get; init; }

    /// <inheritdoc />
    public Guid? CausationId { get; init; }

    /// <inheritdoc />
    public Guid? CorrelationId { get; init; }

    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringObjectDictionaryConverter))]
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } =
        new Dictionary<string, object?>();

    /// <inheritdoc />
    public virtual IDomainEvent<TAggregate> WithStreamVersion(long streamVersion) =>
        this with { StreamVersion = streamVersion };

    /// <inheritdoc />
    public virtual IDomainEvent<TAggregate> WithAggregateId(Guid aggregateId) =>
        this with { AggregateId = aggregateId };

    /// <inheritdoc />
    public virtual IDomainEvent<TAggregate> WithCausation(Guid causationId) =>
        this with { CausationId = causationId };

    /// <inheritdoc />
    public virtual IDomainEvent<TAggregate> WithCorrelation(Guid correlationId) =>
        this with { CorrelationId = correlationId };

    /// <inheritdoc />
    public virtual IDomainEvent<TAggregate> WithMetadata(string key, object? value)
    {
        var newMetadata = new Dictionary<string, object?>(Metadata) { [key] = value };
        return this with { Metadata = newMetadata };
    }
}

/// <summary>
/// Custom JSON converter for Dictionary&lt;string, object?&gt; that handles JSON serialization properly.
/// </summary>
public sealed class JsonStringObjectDictionaryConverter :
    JsonConverter<IReadOnlyDictionary<string, object?>>
{
    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var dictionary = new Dictionary<string, object?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            object? value = ReadValue(ref reader, options);
            dictionary[propertyName] = value;
        }

        return dictionary;
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        IReadOnlyDictionary<string, object?> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            WriteValue(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }

    private static object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out long longValue) ? longValue : reader.GetDouble(),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Null => null,
            JsonTokenType.StartObject => JsonSerializer.Deserialize<Dictionary<string, object?>>(ref reader, options),
            JsonTokenType.StartArray => JsonSerializer.Deserialize<object[]>(ref reader, options),
            _ => throw new JsonException($"Unsupported token type: {reader.TokenType}")
        };

    private static void WriteValue(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}