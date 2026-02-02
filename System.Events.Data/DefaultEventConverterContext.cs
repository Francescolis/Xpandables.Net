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
using System.Text.Json.Serialization.Metadata;

namespace System.Events.Data;

/// <summary>
/// Provides a default implementation of the event converter context that supplies JSON serialization options and type
/// metadata for event conversion operations.
/// </summary>
/// <param name="serializerOptions">The JSON serialization options to use when resolving type metadata. Cannot be null.</param>
public sealed class DefaultEventConverterContext(JsonSerializerOptions serializerOptions) : IEventConverterContext
{
    /// <inheritdoc/>
    public JsonSerializerOptions SerializerOptions { get; } = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));

    /// <inheritdoc/>
    public JsonTypeInfo ResolveJsonTypeInfo(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return SerializerOptions.GetTypeInfo(type)
        ?? throw new InvalidOperationException($"No JsonTypeInfo registered for type '{type.FullName}'.");
    }
}
