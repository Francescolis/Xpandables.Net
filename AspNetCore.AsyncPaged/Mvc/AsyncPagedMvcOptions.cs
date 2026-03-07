/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
/// Provides configuration options for MVC controllers that return asynchronous paged results,
/// customizing MVC behavior to support paged result serialization.
/// </summary>
/// <param name="jsonOptions">The JSON options used to configure the JSON serializer for paged output formatting.
/// The <see cref="JsonOptions.JsonSerializerOptions"/> value is captured at construction time.</param>
/// <remarks>
/// <para>This options class is intended for use with ASP.NET Core MVC and configures settings such as endpoint
/// routing, content negotiation, and output formatter registration to enable consistent handling of
/// <see cref="System.Collections.Generic.IAsyncPagedEnumerable{T}"/> results.</para>
/// <para>It is typically registered via dependency injection using
/// <c>services.ConfigureOptions&lt;AsyncPagedMvcOptions&gt;()</c> and applied internally by the framework
/// during MVC option configuration.</para>
/// </remarks>
public sealed class AsyncPagedMvcOptions(IOptions<JsonOptions> jsonOptions) : IConfigureOptions<MvcOptions>
{
    private readonly JsonOptions _jsonOptions = jsonOptions.Value;

    /// <inheritdoc/>
    /// <remarks>
    /// Inserts an <see cref="AsyncPagedTextOutputFormatter"/> at position 0 in the output formatters list,
    /// disables endpoint routing, and enables strict content negotiation (RespectBrowserAcceptHeader,
    /// ReturnHttpNotAcceptable).
    /// </remarks>
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
