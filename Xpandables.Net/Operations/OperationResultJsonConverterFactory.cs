
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

namespace Xpandables.Net.Operations;

/// <summary>
/// A factory for creating JSON converters for <see cref="OperationResult{TResult}"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="OperationResultJsonConverterFactory"/> class.
/// The <paramref name="useAspNetCoreCompatibility"/> indicates whether to use
/// ASP.NET Core compatibility. The default value is <see langword="false"/>.
/// The ASP.NET Core compatibility is used to serialize only the result of 
/// the operation.
/// </remarks>
/// <param name="useAspNetCoreCompatibility">A value indicating whether to use
/// ASP.NET Core compatibility.</param>
public sealed class OperationResultJsonConverterFactory(
    bool useAspNetCoreCompatibility = false) : JsonConverterFactory
{
    private readonly bool _useAspNetCoreCompatibility = useAspNetCoreCompatibility;

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(IOperationResult)
        || (typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(IOperationResult<>));

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(IOperationResult))
        {
            return _useAspNetCoreCompatibility
                ? new OperationResultAspJsonConverter()
                : new OperationResultJsonConverter();
        }

        Type resultType = typeToConvert.GetGenericArguments()[0];
        Type converterType = _useAspNetCoreCompatibility
            ? typeof(OperationResultAspJsonConverter<>).MakeGenericType(resultType)
            : typeof(OperationResultJsonConverter<>).MakeGenericType(resultType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
