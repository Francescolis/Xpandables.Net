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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Events.Domain;

/// <summary>
/// Defines the contract for a domain event within an event-sourced system, providing properties and methods for event
/// stream identification, versioning, causation, correlation, and metadata management.
/// </summary>
/// <remarks>Implementations of this interface represent events that are part of an event stream and support
/// tracking of event relationships and additional metadata. Domain events are typically used to capture state changes
/// and enable event-driven workflows. All properties and methods are intended for use in event sourcing and
/// domain-driven design scenarios.</remarks>
public interface IDomainEvent : IEvent
{
    /// <summary>
    /// Gets the unique identifier for the stream.
    /// </summary>
    Guid StreamId { get; init; }

    /// <summary>
    /// Gets the name of the stream associated with this instance.
    /// </summary>
    string StreamName { get; }

    /// <summary>
    /// Gets the stream version (position) of this event within the event stream.
    /// This is used for proper event ordering and optimistic concurrency control.
    /// </summary>
    long StreamVersion { get; init; }

    /// <summary>
    /// Gets metadata associated with the event as a JSON-serializable dictionary.
    /// </summary>
    /// <remarks>Values should be JSON-serializable primitives types (string, number, bool, null).</remarks>
    IReadOnlyDictionary<string, object?> Metadata { get; init; }

    /// <summary>
    /// Sets the stream version of the event.
    /// </summary>
    /// <param name="streamVersion">The stream version to set.</param>
    /// <returns>The event domain with the specified stream version.</returns>
    IDomainEvent WithStreamVersion(long streamVersion);

    /// <summary>
    /// Sets the stream identifier of the event.
    /// </summary>
    /// <param name="streamId">The stream identifier to set.</param>
    /// <returns>The event domain with the specified stream identifier.</returns>
    IDomainEvent WithStreamId(Guid streamId);

    /// <summary>
    /// Associates the specified stream name with the domain event.
    /// </summary>
    /// <param name="streamName">The name of the stream to associate with the domain event. Cannot be null or empty.</param>
    /// <returns>A new <see cref="IDomainEvent"/> instance with the specified stream name.</returns>
    IDomainEvent WithStreamName(string streamName);

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
/// Represents a domain event that is associated with an aggregate.
/// </summary>
public abstract record DomainEvent : EventBase, IDomainEvent
{
    /// <inheritdoc />
    public required Guid StreamId { get; init; }

    /// <inheritdoc />
    public string StreamName { get; init; } = string.Empty;

    /// <inheritdoc/>
    public long StreamVersion { get; init; }

    /// <inheritdoc />
    [JsonConverter(typeof(DomainEventMetaDataJsonStringConverter))]
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } =
        new Dictionary<string, object?>();

    /// <inheritdoc />
    public virtual IDomainEvent WithStreamVersion(long streamVersion) =>
        this with { StreamVersion = streamVersion };

    /// <inheritdoc />
    public virtual IDomainEvent WithStreamId(Guid streamId) =>
        this with { StreamId = streamId };

    /// <inheritdoc />
    public virtual IDomainEvent WithStreamName(string streamName) =>
        this with { StreamName = streamName };

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
/// Custom JSON converter for Dictionary&lt;string, object?&gt; that handles JSON MetaData serialization properly.
/// </summary>
public sealed class DomainEventMetaDataJsonStringConverter : JsonConverter<IReadOnlyDictionary<string, object?>>
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
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

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
            switch (value)
            {
                case string str:
                    writer.WriteStringValue(str);
                    break;
                case long l:
                    writer.WriteNumberValue(l);
                    break;
                case int i:
                    writer.WriteNumberValue(i);
                    break;
                case double d:
                    writer.WriteNumberValue(d);
                    break;
                case float f:
                    writer.WriteNumberValue(f);
                    break;
                case decimal dec:
                    writer.WriteNumberValue(dec);
                    break;
                case bool b:
                    writer.WriteBooleanValue(b);
                    break;
                default:
                    throw new JsonException($"Unsupported value type: {value.GetType()}");
            }
        }
    }
}