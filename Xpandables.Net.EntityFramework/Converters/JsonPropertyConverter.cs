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

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Xpandables.Net.Text;

namespace Xpandables.Net.Converters;
/// <summary>
/// Converts property to/from JSON.
/// </summary>
public sealed class JsonPropertyConverter<TProperty> : ValueConverter<TProperty?, string?>
{
    ///<inheritdoc/>
    public JsonPropertyConverter()
        : base(
            v => v == null
                ? null
                : JsonSerializer.Serialize(v, JsonSerializerDefaultOptions.Options),
            v => string.IsNullOrEmpty(v)
                ? default
                : JsonSerializer.Deserialize<TProperty>(v, JsonSerializerDefaultOptions.Options))
    { }
}