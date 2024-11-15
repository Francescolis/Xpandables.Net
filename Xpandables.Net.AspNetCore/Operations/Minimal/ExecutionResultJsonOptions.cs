
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
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Xpandables.Net.Operations.Minimal;

/// <summary>  
/// Configures JSON options for execution results with minimal settings.  
/// </summary>  
public sealed class ExecutionResultJsonOptions : IConfigureOptions<JsonOptions>
{
    /// <inheritdoc/>  
    public void Configure(JsonOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.SerializerOptions.PropertyNameCaseInsensitive = true;
        options.SerializerOptions.PropertyNamingPolicy = null;

        options.SerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
        options.SerializerOptions.Converters
            .Add(new ExecutionResultJsonConverterFactory()
            {
                UseAspNetCoreCompatibility = true
            });
    }
}
