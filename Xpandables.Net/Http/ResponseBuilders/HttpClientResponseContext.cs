
/*******************************************************************************
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
********************************************************************************/
using System.Text.Json;

namespace Xpandables.Net.Http.ResponseBuilders;


/// <summary>
/// Contains the context of the current HTTP response being built.
/// </summary>
public record HttpClientResponseContext
{
    /// <summary>
    /// Constructs a new instance of <see cref="HttpClientResponseContext"/>.
    /// </summary>
    /// <param name="response">The response of the HTTP request.</param>
    /// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/> to be used.</param>
    public HttpClientResponseContext(
        HttpResponseMessage response,
        JsonSerializerOptions serializerOptions)
    {
        ResponseMessage = response
            ?? throw new ArgumentNullException(nameof(response));
        SerializerOptions = serializerOptions
            ?? throw new ArgumentNullException(nameof(serializerOptions));
    }

    /// <summary>
    /// Gets the response of the HTTP request.
    /// </summary>
    public HttpResponseMessage ResponseMessage { get; }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> to be used.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; }
}