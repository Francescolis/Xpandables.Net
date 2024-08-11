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
using Xpandables.Net.Http.Requests;

namespace Xpandables.Net.Http.RequestBuilders;

/// <summary>
/// Builds the request for <see cref="IHttpClientDistributor"/>.
/// </summary>
public interface IHttpClientRequestBuilder
{
    /// <summary>
    /// Gets the request type being built by the current builder instance.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets the zero based request content execution order.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// When overridden in a derived class, determines whether the builder
    /// instance can build the specified request.
    /// </summary>
    /// <param name="targetType">The type of the target request.</param>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    bool CanBuild(Type targetType);

    /// <summary>
    /// Builds the request for the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="context">The request context to act with.</param>
    void Build(HttpClientRequestContext context);
}


/// <summary>
/// Builds the request for <see cref="IHttpClientDistributor"/>.
/// </summary>
/// <typeparam name="TInterfaceRequest">The type of the interface
/// implemented by the request source.</typeparam>
public interface IHttpClientRequestBuilder<TInterfaceRequest> :
    IHttpClientRequestBuilder
    where TInterfaceRequest : class, IHttpRequest
{
    /// <summary>
    /// Gets the request type being built by the current builder instance.
    /// </summary>
    public new Type Type => typeof(TInterfaceRequest);

    Type IHttpClientRequestBuilder.Type => Type;
}
