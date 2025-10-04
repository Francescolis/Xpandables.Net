
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace System.Net.Async;

/// <summary>
/// Provides configuration options to enable JSON output formatting for asynchronous paged enumerables in ASP.NET Core
/// MVC.
/// </summary>
/// <remarks>This options class is typically registered to configure MVC so that actions returning asynchronous
/// paged enumerables are correctly formatted as JSON responses. The specified serializer options control how the paged
/// data is serialized.</remarks>
/// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/> to use for serializing asynchronous paged enumerable results to JSON. Cannot
/// be null.</param>
public sealed class AsyncPagedEnumerableMvcOptions(JsonSerializerOptions serializerOptions) : IConfigureOptions<MvcOptions>
{
    /// <inheritdoc/>
    public void Configure(MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.OutputFormatters.Insert(0, new AsyncPagedEnumerableJsonOutputFormatter(serializerOptions));
    }
}
