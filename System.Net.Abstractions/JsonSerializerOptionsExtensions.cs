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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Net.Abstractions;

/// <summary>
/// Provides extension methods and preconfigured options for customizing JSON serialization behavior with <see
/// cref="JsonSerializerOptions"/> instances.
/// </summary>
/// <remarks>This class includes members that simplify configuring <see cref="JsonSerializerOptions"/> for common
/// scenarios, such as web API serialization. The provided options are compatible with ASP.NET Core defaults and help
/// ensure consistent JSON formatting and property handling across applications.</remarks>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Provides a set of preconfigured JSON serialization options optimized for web API scenarios.
    /// </summary>
    extension(JsonSerializerOptions)
    {
        /// <summary>
        /// Gets a preconfigured set of JSON serialization options suitable for web APIs, using the default web
        /// conventions.
        /// </summary>
        /// <remarks>The returned options use case-insensitive property name matching, indented
        /// formatting, and support for serializing enums as strings. These settings are designed to align with common
        /// web serialization practices and are compatible with ASP.NET Core's default JSON behavior.</remarks>
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public static JsonSerializerOptions DefaultWeb => new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}

/// <summary>
/// Provides serialization context for types using source-generated JSON serialization.
/// </summary>
/// <remarks>This class is typically used to supply serialization metadata and options for the System.Text.Json
/// source generator. It enables efficient, strongly-typed serialization and deserialization of objects at runtime. Use
/// this context when working with APIs that require a JsonSerializerContext instance for custom serialization
/// scenarios.</remarks>
[JsonSerializable(typeof(object))]
public partial class ObjectContext : JsonSerializerContext
{
}
