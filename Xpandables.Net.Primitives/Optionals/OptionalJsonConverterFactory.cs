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
using System.Text.Json.Serialization;

namespace Xpandables.Net.Optionals;

/// <summary>
/// JSON converter factory for Optional&lt;T&gt; types, providing AOT-compatible serialization for .NET 10.
/// </summary>
/// <remarks>
/// <para>
/// This factory automatically discovers and creates converters for all Optional&lt;T&gt; types used in your code.
/// The implementation is generated at compile-time by the Xpandables.Net.Primitives.SourceGeneration analyzer.
/// </para>
/// <para>
/// <strong>How it works:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Primitive types (string, int, bool, etc.) are handled by <see cref="OptionalJsonContext"/></description></item>
/// <item><description>Custom types are discovered by analyzing your code and added to the converter cache automatically</description></item>
/// <item><description>All converters are created at compile-time for optimal AOT compatibility</description></item>
/// </list>
/// <para>
/// <strong>Usage:</strong>
/// </para>
/// <code>
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new OptionalJsonConverterFactory() }
/// };
/// 
/// var json = JsonSerializer.Serialize(myObject, options);
/// </code>
/// </remarks>
public sealed partial class OptionalJsonConverterFactory : JsonConverterFactory
{
    // Implementation is provided by OptionalJsonConverterGenerator
    // in Xpandables.Net.Primitives.SourceGeneration
}
