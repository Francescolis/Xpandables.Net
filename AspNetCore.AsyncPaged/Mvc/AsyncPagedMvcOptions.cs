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
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// Provides configuration options for MVC controllers that return execution results, customizing MVC behavior to
/// support execution result processing.
/// </summary>
/// <param name="jsonOptions">The JSON options used to configure the JSON serializer.</param>
/// <remarks>This options class is intended for use with ASP.NET Core MVC and configures settings such as endpoint
/// routing, content negotiation, and filter registration to enable consistent handling of execution results. It is
/// typically registered via dependency injection and used internally by the framework.</remarks>
public sealed class AsyncPagedMvcOptions(IOptions<JsonOptions> jsonOptions) : IConfigureOptions<MvcOptions>
{
    private readonly JsonOptions _jsonOptions = jsonOptions.Value;
    /// <inheritdoc/>
    public void Configure(MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.EnableEndpointRouting = false;
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;

        options.OutputFormatters
            .Insert(0, new AsyncPagedTextOutputFormatter(_jsonOptions.JsonSerializerOptions));
    }
}
