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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Extensions;

/// <summary>
/// Provides with methods to extend use of <see cref="JsonDocument"/>.
/// </summary>
public static class JsonDocumentExtensions
{
    /// <summary>
    /// Converts the current document to a <see cref="JsonDocument"/> using the specified options.
    /// </summary>
    /// <typeparam name="TDocument">The type of document.</typeparam>
    /// <param name="document">An instance of document to parse.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <param name="documentOptions">Options to control the reader behavior during parsing.</param>
    /// <returns>An instance of <see cref="JsonDocument"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> is null.</exception>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="JsonConverter"/>
    /// for <typeparamref name="TDocument"/> 
    /// or its serializable members.</exception>
    /// <exception cref="ArgumentException">The <paramref name="documentOptions"/> 
    /// contains unsupported options.</exception>
    public static JsonDocument ToJsonDocument<TDocument>(
        this TDocument document,
        JsonSerializerOptions? serializerOptions = default,
        JsonDocumentOptions documentOptions = default)
        where TDocument : notnull
    {
        _ = document ?? throw new ArgumentNullException(nameof(document));

        string documentString = JsonSerializer.Serialize(
            document,
            document.GetType(),
            serializerOptions);

        return JsonDocument.Parse(documentString, documentOptions);
    }
}
