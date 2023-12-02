
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

namespace Xpandables.Net.Primitives;

/// <summary>
/// Base class for JSON Type Only Converter.
/// </summary>
/// <typeparam name="T">The type to parse to.</typeparam>
public abstract class JsonTypeOnlyConverter<T> : JsonConverter<T>
{
    ///<inheritdoc/>
    public sealed override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DoRead(ref reader);

    /// <summary>
    /// When implemented in derived classes, will return the an instance of <typeparamref name="T"/>
    /// from <paramref name="reader"/> value.
    /// </summary>
    /// <param name="reader">The reader element of the current JSON.</param>
    /// <returns>An object of <typeparamref name="T"/> type if available.</returns>
    protected abstract T DoRead(ref Utf8JsonReader reader);
}
