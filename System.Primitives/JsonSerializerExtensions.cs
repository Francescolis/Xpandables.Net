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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace System;

/// <summary>
/// Provides extension methods for resolving JSON serialization metadata for .NET types using System.Text.Json.
/// </summary>
/// <remarks>These extension methods enable advanced scenarios for obtaining type metadata that reflects the
/// serialization behavior defined by specific or default JsonSerializerOptions. This can be useful for custom
/// serialization workflows, diagnostics, or integration with frameworks that require access to serialization
/// metadata.</remarks>
public static class JsonSerializerExtensions
{
    extension(JsonSerializer)
    {
        /// <summary>
        /// Resolves metadata for the specified .NET type to support JSON serialization and deserialization using the
        /// provided or default options.
        /// </summary>
        /// <remarks>If <paramref name="options"/> is null, the method uses <see
        /// cref="JsonSerializerOptions.Web"/> as the default configuration. The returned metadata reflects the
        /// serialization behavior defined by the resolved options, including any custom converters or
        /// policies.</remarks>
        /// <param name="type">The type for which to obtain JSON serialization metadata. Cannot be null.</param>
        /// <param name="options">The options to use when resolving metadata. If null, the default web options are used.</param>
        /// <returns>A <see cref="JsonTypeInfo"/> instance containing metadata for the specified type, configured according to
        /// the provided or default options.</returns>
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
        [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
        public static JsonTypeInfo GetJsonTypeInfo(Type type, JsonSerializerOptions? options)
        {
            ArgumentNullException.ThrowIfNull(type);

            // Resolves JsonTypeInfo metadata using the appropriate JsonSerializerOptions configuration,
            // following the semantics of the JsonSerializer reflection methods.

            options ??= JsonSerializerOptions.Web;
            options.MakeReadOnly(populateMissingResolver: true);
            return options.GetTypeInfo(type);
        }

        /// <summary>
        /// Retrieves the JSON type metadata for the specified .NET type from the provided serialization context.
        /// </summary>
        /// <remarks>Use this method to access type-specific serialization information, such as property
        /// mappings and converters, when working with custom or source-generated JSON serialization contexts.</remarks>
        /// <param name="type">The .NET type for which to obtain JSON serialization metadata. Cannot be null.</param>
        /// <param name="context">The serialization context that contains metadata for supported types. Cannot be null.</param>
        /// <returns>A <see cref="JsonTypeInfo"/> instance containing serialization metadata for the specified type.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="type"/> is not supported by the provided <paramref name="context"/>.</exception>
        public static JsonTypeInfo GetJsonTypeInfo(Type type, JsonSerializerContext context)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(context);

            return context.GetTypeInfo(type)
                ?? throw new InvalidOperationException($"The type '{type.FullName}' is not supported by the provided JsonSerializerContext.");
        }
    }
}
