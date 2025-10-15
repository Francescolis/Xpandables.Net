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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Text;

/// <summary>
/// Provides access to the default JSON serialization options used for web-based scenarios.
/// </summary>
/// <remarks>This class supplies a globally accessible set of <see cref="JsonSerializerOptions"/> configured for
/// typical web applications, such as those built with ASP.NET Core. Modifying these options affects the default
/// behavior of JSON serialization and deserialization throughout the application wherever these defaults are
/// used.</remarks>
public static class SerializationDefaultOptions
{
    /// <summary>
    /// Gets or sets the default <see cref="JsonSerializerOptions"/> used for web-based JSON serialization and
    /// deserialization.
    /// </summary>
    /// <remarks>This property provides a set of options optimized for web scenarios, such as those used in
    /// ASP.NET Core. Changing this value affects the default behavior of JSON serialization throughout the application
    /// where these options are used. The property must be set to a non-null value.</remarks>
    public static JsonSerializerOptions Value
    {
        get => _value;
        set => _value = value ?? throw new ArgumentNullException(nameof(value));

    }

    private static JsonSerializerOptions _value =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = true
        };
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
