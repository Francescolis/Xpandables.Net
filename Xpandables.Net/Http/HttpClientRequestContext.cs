﻿
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

namespace Xpandables.Net.Http;
/// <summary>
/// Represents the context for an HTTP client request.
/// </summary>
public record HttpClientRequestContext
{
    /// <summary>
    /// Gets the attribute associated with the HTTP client request.
    /// </summary>
    public required HttpClientRequestOptionsAttribute Attribute { get; init; }

    /// <summary>
    /// Gets the HTTP client request.
    /// </summary>
    public required IHttpClientRequest Request { get; init; }

    /// <summary>
    /// Gets the HTTP request message.
    /// </summary>
    public required HttpRequestMessage Message { get; init; }

    /// <summary>
    /// Gets the JSON serializer options.
    /// </summary>
    public required JsonSerializerOptions SerializerOptions { get; init; }
}