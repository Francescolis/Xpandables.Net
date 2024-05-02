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
    /// <typeparamref name="TInterfaceRequest"/> type.
    /// </summary>
    /// <typeparam name="TInterfaceRequest">The type of interface implement by 
    /// the request to build.</typeparam>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    public abstract bool CanBuild<TInterfaceRequest>();
}

/// <summary>
/// Build the request for the <see cref="HttpClient"/>.
/// </summary>
/// <typeparam name="TInterfaceRequest">The type of the interface
/// implemented by the request source.</typeparam>
public abstract class HttpClientRequestBuilder<TInterfaceRequest> :
    HttpClientRequestBuilder
    where TInterfaceRequest : class
{
    /// <inheritdoc/>
    public sealed override Type? Type => typeof(TInterfaceRequest);

    /// <summary>
    /// Builds the request for the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="request">The request source to use.</param>
    /// <param name="requestMessage">The request message to act on.</param>
    /// <param name="serviceProvider">The service provider to use.</param>
    /// <returns>The built request.</returns>
    public abstract HttpRequestMessage Build(
        TInterfaceRequest request,
        HttpRequestMessage requestMessage,
        IServiceProvider serviceProvider);
}