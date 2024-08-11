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

using Xpandables.Net.Http.Requests;

namespace Xpandables.Net.Http.RequestBuilders;


/// <summary>
/// Contains the context of the current HTTP request being built.
/// </summary>
public record HttpClientRequestContext
{
    /// <summary>
    /// Constructs a new instance of <see cref="HttpClientRequestContext"/>.
    /// </summary>
    /// <param name="attribute">The attribute of the current HTTP request.</param>
    /// <param name="request">The request of the current HTTP request.</param>
    /// <param name="requestMessage">The request message of the current HTTP request.</param>
    /// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/> to be used.</param>
    public HttpClientRequestContext(
        HttpClientAttribute attribute,
        IHttpClientRequest request,
        HttpRequestMessage requestMessage,
        JsonSerializerOptions serializerOptions)
    {
        Attribute = attribute
            ?? throw new ArgumentNullException(nameof(attribute));
        Request = request
            ?? throw new ArgumentNullException(nameof(request));
        RequestMessage = requestMessage
            ?? throw new ArgumentNullException(nameof(requestMessage));
        SerializerOptions = serializerOptions
            ?? throw new ArgumentNullException(nameof(serializerOptions));
    }

    /// <summary>
    /// Gets the attribute of the current HTTP request.
    /// </summary>
    public HttpClientAttribute Attribute { get; }

    /// <summary>
    /// Gets the request of the current HTTP request.
    /// </summary>
    public IHttpClientRequest Request { get; }

    /// <summary>
    /// Gets the request message being built.
    /// </summary>
    public HttpRequestMessage RequestMessage { get; }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> to be used.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; }
}