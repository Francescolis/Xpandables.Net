
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

namespace Xpandables.Net.Collections;
/// <summary>
/// A JSON converter factory for materialized paged data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MaterializedPagedDataJsonConverterFactory"/> class.
/// </remarks>
public sealed class MaterializedPagedDataJsonConverterFactory(IServiceProvider serviceProvider) : JsonConverterFactory
{
    /// <summary>
    /// Gets or sets a value indicating whether to serialize pagination info to headers instead of the body.
    /// </summary>
    public required bool UsePaginationHeaders { get; init; }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(AsyncPagedEnumerableData<>);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var itemType = typeToConvert.GetGenericArguments()[0];
        var converterType = UsePaginationHeaders
            ? typeof(MaterializedPagedDataHeaderJsonConverter<>).MakeGenericType(itemType)
            : typeof(MaterializedPagedDataBodyJsonConverter<>).MakeGenericType(itemType);

        return UsePaginationHeaders
            ? (JsonConverter)Activator.CreateInstance(converterType, serviceProvider)!
            : (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}