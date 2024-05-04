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

namespace Xpandables.Net.Http;

/// <summary>
/// Build the request for the <see cref="HttpClient"/>.
/// </summary>
public abstract class HttpClientRequestBuilder
{
    /// <summary>
    /// Gets the request type being built by the current builder instance.
    /// </summary>
    public abstract Type? Type { get; }

    /// <summary>
    /// When overridden in a derived class, determines whether the builder
    /// instance can build the specified request implementing the
    /// a specific interface type.
    /// </summary>
    /// <param name="targetType">The type of the target interface.</param>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    public abstract bool CanBuild(Type targetType);

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> to be used.
    /// </summary>
    public static readonly JsonSerializerOptions SerializerOptions
        = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = true,
        };
}

/// <summary>
/// Build the attribute for a request.
/// </summary>
public abstract class HttpClientRequestAttributeBuilder :
    HttpClientRequestBuilder
{
    /// <summary>
    /// Builds the attribute for a <see cref="IHttpClientRequest"/>.
    /// </summary>
    /// <param name="request">The request data source to use.</param>
    /// <param name="serviceProvider">The service provider to use.</param>
    /// <returns>The built attribute.</returns>
    public abstract HttpClientAttribute Build(
        IHttpClientAttributeProvider request,
        IServiceProvider serviceProvider);
}


/// <summary>
/// Build the request for the <see cref="HttpClient"/>.
/// </summary>
/// <typeparam name="TInterfaceRequest">The type of the interface
/// implemented by the request source.</typeparam>
public abstract class HttpClientRequestBuilder<TInterfaceRequest> :
    HttpClientRequestBuilder, IHttpClientRequestBuilder<TInterfaceRequest>
    where TInterfaceRequest : class
{
    /// <inheritdoc/>
    public sealed override Type? Type => typeof(TInterfaceRequest);

    /// <inheritdoc/>
    public abstract HttpRequestMessage Build(
        HttpClientAttribute attribute,
        TInterfaceRequest request,
        HttpRequestMessage requestMessage);
}