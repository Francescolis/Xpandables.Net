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
/// Provides contextual information and services for event conversion operations, including access to JSON serialization
/// settings and type metadata.
/// </summary>
public interface IEventConverterContext
{
    /// <summary>
    /// Gets the options used to configure JSON serialization and deserialization behavior.
    /// </summary>
    JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    /// Resolves metadata for the specified .NET type to support JSON serialization and deserialization.
    /// </summary>
    /// <param name="type">The type for which to resolve JSON serialization metadata. Cannot be null.</param>
    /// <returns>A <see cref="JsonTypeInfo"/> instance containing metadata for the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the type cannot be resolved to JSON serialization metadata.</exception>
    JsonTypeInfo ResolveJsonTypeInfo(Type type);
}
