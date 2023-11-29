
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
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Add <see cref="IOperationResult"/>/<see cref="IOperationResult{TValue}"/> converters
/// to <see cref="JsonOptions"/>. You can derive from this class to customize its behavior.
/// </summary>
/// <remarks>
/// Adds the <see cref="JsonStringEnumConverter"/>, <see cref="OperationResultJsonConverterFactory"/>.
/// </remarks>
public class OperationResultControllerConfigureFormatterOptions : IConfigureOptions<JsonOptions>
{
    ///<inheritdoc/>
    public virtual void Configure(JsonOptions options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));

        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new OperationResultJsonConverterFactory());
        options.JsonSerializerOptions.Converters.Add(new JsonDateOnlyConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonTimeOnlyConverter());
    }
}
